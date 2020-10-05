using System;
using System.Collections.Generic;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Connection;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Connection
{
    public class StartOkMethodTests
    {
        [Fact]
        public void StartOkMethodCtor()
        {
            var mechanism = "PLAIN";
            var response = "test";
            var locale = "en_US";
            var clientProperties = new Dictionary<string, object>()
            {
                ["exchange_exchange_bindings"] = true
            };

            var startOkMethod = new StartOkMethod(mechanism, response, locale, clientProperties);

            Assert.Equal((uint)MethodId.ConnectionStartOk, startOkMethod.Method);
            Assert.Equal(mechanism, startOkMethod.Mechanism);
            Assert.Equal(response, startOkMethod.Response);
            Assert.Equal(locale, startOkMethod.Locale);
            Assert.Equal(clientProperties, startOkMethod.ClientProperties);
        }

        [Fact]
        public void StartOkMethodFrameFormatter()
        {
            var expected = Helpers.GetFileContent("RabbitMQ.Next.Tests.Transport.Methods.Connection.StartOkMethodPayload.dat");

            var clientProperties = new Dictionary<string, object>()
            {
                ["product"] = "RabbitMQ.Next",
                ["version"] = "0.1.0",
                ["capabilities"] = new Dictionary<string, object>()
                {
                    ["exchange_exchange_bindings"] = true
                }
            };

            var data = new StartOkMethod("PLAIN", "\0test1\0test1", "en_US", clientProperties);
            Span<byte> payload = stackalloc byte[expected.Length];
            new StartOkMethodFrameFormatter().Write(payload, data);

            Assert.Equal(expected, payload.ToArray());
        }
    }
}