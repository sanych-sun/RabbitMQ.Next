using System.Collections.Generic;
using RabbitMQ.Next.Transport.Methods.Connection;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Connection;

public class ModelTests
{
    [Fact]
    public void StartMethod()
    {
        const byte versionMajor = (byte)0;
        const byte versionMinor = (byte)9;
        const string mechanisms = "test";
        const string locales = "en_US";
        var props = new Dictionary<string,object>
        {
            ["key"] = "value",
        };

        var startMethod = new StartMethod(versionMajor, versionMinor, mechanisms, locales, props);

        Assert.Equal(MethodId.ConnectionStart, startMethod.MethodId);
        Assert.Equal(versionMajor, startMethod.VersionMajor);
        Assert.Equal(versionMinor, startMethod.VersionMinor);
        Assert.Equal(mechanisms, startMethod.Mechanisms);
        Assert.Equal(locales, startMethod.Locales);
        Assert.Equal(props, startMethod.ServerProperties);
    }

    [Fact]
    public void StartOkMethod()
    {
        const string mechanism = "PLAIN";
        var response = "ab"u8.ToArray();
        const string locale = "en_US";
        var clientProperties = new Dictionary<string, object>
        {
            ["exchange_exchange_bindings"] = true,
        };

        var startOkMethod = new StartOkMethod(mechanism, response, locale, clientProperties);

        Assert.Equal(MethodId.ConnectionStartOk, startOkMethod.MethodId);
        Assert.Equal(mechanism, startOkMethod.Mechanism);
        Assert.Equal(response, startOkMethod.Response);
        Assert.Equal(locale, startOkMethod.Locale);
        Assert.Equal(clientProperties, startOkMethod.ClientProperties);
    }
    
    [Fact]
    public void SecureMethod()
    {
        var challenge = "ping"u8.ToArray();
        
        var secureMethod = new SecureMethod(challenge);

        Assert.Equal(MethodId.ConnectionSecure, secureMethod.MethodId);
        Assert.Equal(challenge, secureMethod.Challenge);
    }

    [Fact]
    public void SecureOkMethod()
    {
        var response = "pong"u8.ToArray();
        
        var startOkMethod = new SecureOkMethod(response);

        Assert.Equal(MethodId.ConnectionSecureOk, startOkMethod.MethodId);
        Assert.Equal(response, startOkMethod.Response);
    }
    
    [Fact]
    public void TuneMethod()
    {
        const ushort channelMax = 256;
        const uint maxFrameSize = 4096;
        const ushort heartbeatInterval = 120;

        var tuneMethod = new TuneMethod(channelMax, maxFrameSize, heartbeatInterval);

        Assert.Equal(MethodId.ConnectionTune, tuneMethod.MethodId);
        Assert.Equal(channelMax, tuneMethod.ChannelMax);
        Assert.Equal(maxFrameSize, tuneMethod.MaxFrameSize);
        Assert.Equal(heartbeatInterval, tuneMethod.HeartbeatInterval);
    }

    [Fact]
    public void TuneOkMethod()
    {
        const ushort channelMax = 256;
        const uint maxFrameSize = 4096;
        const ushort heartbeatInterval = 120;

        var tuneMethod = new TuneOkMethod(channelMax, maxFrameSize, heartbeatInterval);

        Assert.Equal(MethodId.ConnectionTuneOk, tuneMethod.MethodId);
        Assert.Equal(channelMax, tuneMethod.ChannelMax);
        Assert.Equal(maxFrameSize, tuneMethod.MaxFrameSize);
        Assert.Equal(heartbeatInterval, tuneMethod.HeartbeatInterval);
    }

    [Fact]
    public void OpenMethod()
    {
        const string vHost = "/";

        var openMethod = new OpenMethod(vHost);

        Assert.Equal(MethodId.ConnectionOpen, openMethod.MethodId);
        Assert.Equal(vHost, openMethod.VirtualHost);
    }

    [Fact]
    public void OpenOkMethod()
    {
        var openOkMethod = new OpenOkMethod();

        Assert.Equal(MethodId.ConnectionOpenOk, openOkMethod.MethodId);
    }

    [Fact]
    public void CloseMethod()
    {
        var method = new CloseMethod(504, "SomeError", MethodId.BasicAck);

        Assert.Equal(MethodId.ConnectionClose, method.MethodId);
        Assert.Equal(504, method.StatusCode);
        Assert.Equal("SomeError", method.Description);
        Assert.Equal(MethodId.BasicAck, method.FailedMethodId);
    }

    [Fact]
    public void CloseOkMethod()
    {
        var method = new CloseOkMethod();

        Assert.Equal(MethodId.ConnectionCloseOk, method.MethodId);
    }

    [Fact]
    public void BlockedMethod()
    {
        const string reason = "just because";
        var method = new BlockedMethod(reason);

        Assert.Equal(MethodId.ConnectionBlocked, method.MethodId);
        Assert.Equal(reason, method.Reason);
    }

    [Fact]
    public void UnblockedMethod()
    {
        var method = new UnblockedMethod();

        Assert.Equal(MethodId.ConnectionUnblocked, method.MethodId);
    }
}
