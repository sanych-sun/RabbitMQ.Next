using System;
using System.Collections;
using System.Collections.Generic;
using RabbitMQ.Next.Tests.Mocks;
using RabbitMQ.Next.Transport.Methods.Connection;
using Xunit;

namespace RabbitMQ.Next.Tests.Transport.Methods.Connection;

public class SerializationTests : SerializationTestBase
{
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
        var method = new StartOkMethod("PLAIN",  "\0test1\0test1"u8.ToArray(), "en_US", clientProperties);

        this.TestFormatter(method);
    }

    [Fact]
    public void SecureMethodParser()
        => this.TestParser(new SecureMethod("ping"u8.ToArray()), new SecureMethodComparer());
    
    [Fact]
    public void SecureOkMethodFormatter()
        => this.TestFormatter(new SecureOkMethod("pong"u8.ToArray()));

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
        => this.TestFormatter(new CloseMethod(200, "Goodbye", 0));

    [Fact]
    public void CloseMethodParser()
        => this.TestParser(new CloseMethod(200, "Goodbye", 0));

    [Fact]
    public void CloseOkMethodFormatter()
        => this.TestFormatter(new CloseOkMethod());

    [Fact]
    public void CloseOkMethodParser()
        => this.TestParser(new CloseOkMethod());

    [Fact]
    public void BlockedMethodParser()
        => this.TestParser(new BlockedMethod("just because"));

    [Fact]
    public void UnblockedMethodParser()
        => this.TestParser(new UnblockedMethod());

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

    private class SecureMethodComparer : IEqualityComparer<SecureMethod>
    {
        public bool Equals(SecureMethod x, SecureMethod y)
            => x.Challenge.Span.SequenceEqual(y.Challenge.Span);

        public int GetHashCode(SecureMethod obj)
            => obj.Challenge.GetHashCode();
    }
}