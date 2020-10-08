using System;
using System.Collections.Generic;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods;
using RabbitMQ.Next.Transport.Methods.Connection;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Connection
{
    public class SerializationTests : SerializationTestBase
    {
        public SerializationTests()
            : base(builder => builder.AddConnectionMethods())
        {
        }

        [Fact]
        public void StartMethodParser()
        {
            var method = new StartMethod(0, 9, "PLAIN AMQPLAIN", "en_US",
                new Dictionary<string, object>
                {
                    ["capabilities"] = new Dictionary<string, object>
                    {
                        ["publisher_confirms"] = true,
                        ["exchange_exchange_bindings"] = true,
                        ["basic.nack"] = true,
                        ["consumer_cancel_notify"] = true,
                        ["connection.blocked"] = true,
                        ["consumer_priorities"] = true,
                        ["authentication_failure_close"] = true,
                        ["per_consumer_qos"] = true,
                        ["direct_reply_to"] = true,
                    },
                    ["cluster_name"] = "rabbit@my-rabbit",
                    ["copyright"] = "Copyright (c) 2007-2019 Pivotal Software, Inc.",
                    ["information"] = "Licensed under the MPL 1.1. Website: https://rabbitmq.com",
                    ["platform"] = "Erlang/OTP 22.2.1",
                    ["product"] = "RabbitMQ",
                    ["version"] = "3.8.2"
                });

            this.TestParser(method, new StartMethodComparer());
        }

        [Fact]
        public void StartOkMethodFormatter()
        {
            var clientProperties = new Dictionary<string, object>()
            {
                ["product"] = "RabbitMQ.Next",
                ["version"] = "0.1.0",
                ["capabilities"] = new Dictionary<string, object>()
                {
                    ["exchange_exchange_bindings"] = true
                }
            };
            var method = new StartOkMethod("PLAIN", "\0test1\0test1", "en_US", clientProperties);

            this.TestFormatter(method);
        }

        [Fact]
        public void TuneMethodParser()
            => this.TestParser(new TuneMethod(2047, 131072, 60));

        [Fact]
        public void TuneOkMethodFormatter()
            => this.TestFormatter(new TuneOkMethod(2047, 131072, 60));

        [Fact]
        public void OpenMethodFormatter()
            => this.TestFormatter(new OpenMethod("/"));

        [Fact]
        public void OpenOkMethodParser()
            => this.TestParser(new OpenOkMethod());

        [Fact]
        public void CloseMethodFormatter()
            => this.TestFormatter(new CloseMethod(ReplyCode.Success, "Goodbye", 0));

        [Fact]
        public void CloseMethodParser()
            => this.TestParser(new CloseMethod(ReplyCode.Success, "Goodbye", 0));

        [Fact]
        public void CloseOkMethodFormatter()
            => this.TestFormatter(new CloseOkMethod());

        [Fact]
        public void CloseOkMethodParser()
            => this.TestParser(new CloseOkMethod());

        // todo: move this to some aux namespace
        private class StartMethodComparer : IEqualityComparer<StartMethod>
        {
            public bool Equals(StartMethod x, StartMethod y)
            {
                return x.VersionMajor == y.VersionMajor
                       && x.VersionMinor == y.VersionMinor
                       && Helpers.DictionaryEquals(x.ServerProperties, y.ServerProperties)
                       && x.Mechanisms == y.Mechanisms
                       && x.Locales == y.Locales;
            }

            public int GetHashCode(StartMethod obj)
            {
                return HashCode.Combine(obj.VersionMajor, obj.VersionMinor, obj.ServerProperties, obj.Mechanisms, obj.Locales);
            }
        }
    }
}