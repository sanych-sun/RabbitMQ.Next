using System;
using System.Collections.Generic;

namespace RabbitMQ.Next;

public static class ConnectionBuilderExtensions
{
    public static IConnectionBuilder UseDefaults(this IConnectionBuilder builder)
        => builder
            .DefaultClientProperties();

    public static IConnectionBuilder DefaultClientProperties(this IConnectionBuilder builder)
        => builder
            .ClientProperty("product", "RabbitMQ.Next")
            .ClientProperty("version", "0.1.0") // todo: make this auto update by CI
            .ClientProperty("platform", Environment.OSVersion.ToString())
            .ClientProperty("capabilities", new Dictionary<string, object>
            {
                ["authentication_failure_close"] = true,
            });
}