// Copyright (c) 2002-2017 "Neo Technology,"
// Network Engine for Objects in Lund AB [http://neotechnology.com]
// 
// This file is part of Neo4j.
// 
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
// 
//     http://www.apache.org/licenses/LICENSE-2.0
// 
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Neo4j.Driver.Internal.Connector;
using Neo4j.Driver.V1;

namespace Neo4j.Driver.Internal.Routing
{
    internal class ClusterDiscoveryManager
    {
        private readonly IConnection _conn;
        private readonly ILogger _logger;
        public IEnumerable<Uri> Readers { get; internal set; } = new Uri[0];
        public IEnumerable<Uri> Writers { get; internal set; } = new Uri[0];
        public IEnumerable<Uri> Routers { get; internal set; } = new Uri[0];
        public long ExpireAfterSeconds { get; internal set; }

        private const string ProcedureName = "dbms.cluster.routing.getServers";
        public ClusterDiscoveryManager(IConnection connection, ILogger logger)
        {
            _conn = connection;
            _logger = logger;
        }

        /// <remarks>Throws <see cref="InvalidDiscoveryException"/> if the discovery result is invalid.</remarks>
        public void Rediscovery()
        {
            try
            {
                using (var session = new Session(_conn, _logger))
                {
                    var result = session.Run($"CALL {ProcedureName}");
                    var record = result.Single();

                    foreach (var servers in record["servers"].As<List<Dictionary<string, object>>>())
                    {
                        var addresses = servers["addresses"].As<List<string>>();
                        var role = servers["role"].As<string>();
                        switch (role)
                        {
                            case "READ":
                                Readers = addresses.Select(BoltRoutingUri).ToArray();
                                break;
                            case "WRITE":
                                Writers = addresses.Select(BoltRoutingUri).ToArray();
                                break;
                            case "ROUTE":
                                Routers = addresses.Select(BoltRoutingUri).ToArray();
                                break;
                        }
                    }
                    ExpireAfterSeconds = record["ttl"].As<long>();
                }
            }
            catch (Exception e)
            {
                // for any reason we failed to do a discovery
                throw new InvalidDiscoveryException(
                    $"Error when calling `getServers` procedure: {e.Message}. " +
                    "Please make sure that there is a Neo4j 3.1+ causal cluster up running.");
            }

            if (!Readers.Any() || !Writers.Any() || !Routers.Any())
            {
                throw new InvalidDiscoveryException(
                    $"Invalid discovery result: discovered {Routers.Count()} routers, " +
                    $"{Writers.Count()} writers and {Readers.Count()} readers. A Redisvoery is required.");
            }
        }

        private Uri BoltRoutingUri(string address)
        {
            return new Uri("bolt+routing://" + address);
        }
    }

    internal class InvalidDiscoveryException : Exception
    {
        public InvalidDiscoveryException(string message) : base(message)
        {}
    }
}