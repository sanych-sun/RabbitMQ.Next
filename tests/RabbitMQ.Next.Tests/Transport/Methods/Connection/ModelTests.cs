using System.Collections.Generic;
using RabbitMQ.Next.Transport;
using RabbitMQ.Next.Transport.Methods.Connection;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Connection
{
    public class ModelTests
    {
        [Fact]
        public void StartMethod()
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
        public void StartOkMethod()
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
        public void TuneMethod()
        {
            ushort channelMax = 256;
            uint maxFrameSize = 4096;
            ushort heartbeatInterval = 120;

            var tuneMethod = new TuneMethod(channelMax, maxFrameSize, heartbeatInterval);

            Assert.Equal((uint)MethodId.ConnectionTune, tuneMethod.Method);
            Assert.Equal(channelMax, tuneMethod.ChannelMax);
            Assert.Equal(maxFrameSize, tuneMethod.MaxFrameSize);
            Assert.Equal(heartbeatInterval, tuneMethod.HeartbeatInterval);
        }

        [Fact]
        public void TuneOkMethod()
        {
            ushort channelMax = 256;
            uint maxFrameSize = 4096;
            ushort heartbeatInterval = 120;

            var tuneMethod = new TuneOkMethod(channelMax, maxFrameSize, heartbeatInterval);

            Assert.Equal((uint)MethodId.ConnectionTuneOk, tuneMethod.Method);
            Assert.Equal(channelMax, tuneMethod.ChannelMax);
            Assert.Equal(maxFrameSize, tuneMethod.MaxFrameSize);
            Assert.Equal(heartbeatInterval, tuneMethod.HeartbeatInterval);
        }

        [Fact]
        public void OpenMethod()
        {
            var vHost = "/";

            var openMethod = new OpenMethod(vHost);

            Assert.Equal((uint)MethodId.ConnectionOpen, openMethod.Method);
            Assert.Equal(vHost, openMethod.VirtualHost);
        }

        [Fact]
        public void OpenOkMethod()
        {
            var openOkMethod = new OpenOkMethod();

            Assert.Equal((uint)MethodId.ConnectionOpenOk, openOkMethod.Method);
        }

        [Fact]
        public void CloseMethod()
        {
            var method = new CloseMethod(504, "SomeError", 42);

            Assert.Equal((uint)MethodId.ConnectionClose, method.Method);
            Assert.Equal(504, method.StatusCode);
            Assert.Equal("SomeError", method.Description);
            Assert.Equal((uint)42, method.FailedMethodId);
        }

        [Fact]
        public void CloseOkMethod()
        {
            var method = new CloseOkMethod();

            Assert.Equal((uint)MethodId.ConnectionCloseOk, method.Method);
        }

        [Fact]
        public void BlockedMethod()
        {
            var reason = "just because";
            var method = new BlockedMethod(reason);

            Assert.Equal((uint)MethodId.ConnectionBlocked, method.Method);
            Assert.Equal(reason, method.Reason);
        }

        [Fact]
        public void UnblockedMethod()
        {
            var method = new UnblockedMethod();

            Assert.Equal((uint)MethodId.ConnectionUnblocked, method.Method);
        }
    }
}