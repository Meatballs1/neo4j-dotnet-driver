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
using FluentAssertions;
using Neo4j.Driver.Internal;
using Neo4j.Driver.V1;
using Xunit;

namespace Neo4j.Driver.Tests
{
    public class ConfigTests
    {
        public class DefaultConfigTests
        {
            [Fact]
            public void DefaultConfigShouldGiveCorrectValueBack()
            {
                var config = Config.DefaultConfig;
                config.EncryptionLevel.Should().Be(EncryptionLevel.Encrypted);
                config.TrustStrategy.Should().Be(TrustStrategy.TrustAllCertificates);
                config.Logger.Should().BeOfType<DebugLogger>();
                config.MaxIdleSessionPoolSize.Should().Be(10);
            }
        }

        public class ConfigBuilderTests
        {
            [Fact]
            public void ShouldUseDefaultValueIfNotSpecified()
            {
                var config = new Config {EncryptionLevel = EncryptionLevel.Encrypted};

                config.EncryptionLevel.Should().Be(EncryptionLevel.Encrypted);
                config.TrustStrategy.Should().Be(TrustStrategy.TrustAllCertificates);
                config.Logger.Should().BeOfType<DebugLogger>();
                config.MaxIdleSessionPoolSize.Should().Be(10);
            }

            [Fact]
            public void WithLoggingShouldModifyTheSingleValue()
            {
                var config = Config.Builder.WithLogger(null).ToConfig();
                config.EncryptionLevel.Should().Be(EncryptionLevel.Encrypted);
                config.TrustStrategy.Should().Be(TrustStrategy.TrustAllCertificates);
                config.Logger.Should().BeNull();
                config.MaxIdleSessionPoolSize.Should().Be(10);
            }

            [Fact]
            public void WithPoolSizeShouldModifyTheSingleValue()
            {
                var config = Config.Builder.WithMaxIdleSessionPoolSize(3).ToConfig();
                config.EncryptionLevel.Should().Be(EncryptionLevel.Encrypted);
                config.TrustStrategy.Should().Be(TrustStrategy.TrustAllCertificates);
                config.Logger.Should().BeOfType<DebugLogger>();
                config.MaxIdleSessionPoolSize.Should().Be(3);
            }

            [Fact]
            public void WithEncryptionLevelShouldModifyTheSingleValue()
            {
                var config = Config.Builder.WithEncryptionLevel(EncryptionLevel.None).ToConfig();
                config.EncryptionLevel.Should().Be(EncryptionLevel.None);
                config.TrustStrategy.Should().Be(TrustStrategy.TrustAllCertificates);
                config.Logger.Should().BeOfType<DebugLogger>();
                config.MaxIdleSessionPoolSize.Should().Be(10);
            }

            [Fact]
            public void WithTrustStrategyShouldModifyTheSingleValue()
            {
                var config = Config.Builder.WithTrustStrategy(TrustStrategy.TrustSystemCaSignedCertificates).ToConfig();
                config.EncryptionLevel.Should().Be(EncryptionLevel.Encrypted);
                config.TrustStrategy.Should().Be(TrustStrategy.TrustSystemCaSignedCertificates);
                config.Logger.Should().BeOfType<DebugLogger>();
                config.MaxIdleSessionPoolSize.Should().Be(10);
            }

            [Fact]
            public void ChangingNewConfigShouldNotAffectOtherConfig()
            {
                var config = Config.DefaultConfig;
                var config1 = Config.Builder.WithMaxIdleSessionPoolSize(3).ToConfig();
                var config2 = Config.Builder.WithLogger(null).ToConfig();
                

                config2.Logger.Should().BeNull();
                config2.MaxIdleSessionPoolSize.Should().Be(10);

                config1.MaxIdleSessionPoolSize.Should().Be(3);
                config1.Logger.Should().BeOfType<DebugLogger>();

                config.EncryptionLevel.Should().Be(EncryptionLevel.Encrypted);
                config.TrustStrategy.Should().Be(TrustStrategy.TrustAllCertificates);
                config.Logger.Should().BeOfType<DebugLogger>();
                config.MaxIdleSessionPoolSize.Should().Be(10);
            }
        }
    }
}
