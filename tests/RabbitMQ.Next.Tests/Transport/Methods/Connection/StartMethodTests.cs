using System;
using System.Collections.Generic;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Connection;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Connection
{
    public class StartMethodTests
    {
        [Fact]
        public void StartMethodCtor()
        {
            var versionMajor = (byte)0;
            var versionMinor = (byte)9;
            var mechanisms = "test";
            var locales = "en_US";
            var props = new Dictionary<string,object>
            {
                ["key"] = "value",
            };

            var startMethod = new StartMethod(versionMajor, versionMinor, mechanisms, locales, props);

            Assert.Equal((uint)MethodId.ConnectionStart, startMethod.Method);
            Assert.Equal(versionMajor, startMethod.VersionMajor);
            Assert.Equal(versionMinor, startMethod.VersionMinor);
            Assert.Equal(mechanisms, startMethod.Mechanisms);
            Assert.Equal(locales, startMethod.Locales);
            Assert.Equal(props, startMethod.ServerProperties);
        }

        [Fact]
        public void StartMethodFrameParser()
        {
            var payload = Helpers.GetFileContent("RabbitMQ.Next.Tests.Transport.Methods.Connection.StartMethodPayload.dat");
            var parser = new StartMethodParser();
            var data = parser.Parse(payload);
            var dataBoxed = parser.ParseMethod(payload);

            var expected = new StartMethod(0, 9, "PLAIN AMQPLAIN", "en_US",
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

            Assert.Equal(expected, data, new StartMethodComparer());
            Assert.Equal(expected, (StartMethod)dataBoxed, new StartMethodComparer());
        }

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