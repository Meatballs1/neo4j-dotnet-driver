﻿// Copyright (c) 2002-2017 "Neo Technology,"
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
using System.Management.Automation;
using System.Text;
using System.Threading.Tasks;
using Neo4j.Driver.V1;

namespace Neo4j.Driver.IntegrationTests.Internals
{
    public class WindowsPowershellRunner
    {
        public static void RunPowershellCommand(string command, string argument)
        {
            RunPowershellCommand(command, new []{argument});
        }
        public static void RunPowershellCommand(string command, string[] arguments)
        {
            using (var powershell = PowerShell.Create())
            {
                powershell.AddCommand(command);
                foreach (var argument in arguments)
                {
                    powershell.AddArgument(argument);
                }
                var results = powershell.Invoke();
                foreach (var result in results)
                {
                    Console.WriteLine(result);
                }
                if (powershell.HadErrors)
                {
                    throw new Neo4jException("Integration", CollectAsString(powershell.Streams.Error));
                }
            }
        }

        private static string CollectAsString(PSDataCollection<ErrorRecord> errors)
        {
            var output = errors.Select(error => error.ToString()).ToList();
            return string.Join(Environment.NewLine, output);
        }
    }
}
