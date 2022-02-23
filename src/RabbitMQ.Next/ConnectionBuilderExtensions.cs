using System;
using System.Collections.Generic;
using RabbitMQ.Next.Transport.Methods;

namespace RabbitMQ.Next
{
    public static class ConnectionBuilderExtensions
    {
        public static IConnectionBuilder UseDefaults(this IConnectionBuilder builder)
            => builder
                .DefaultMethodRegistry()
                .DefaultClientProperties();

        public static IConnectionBuilder DefaultMethodRegistry(this IConnectionBuilder builder)
            => builder.ConfigureMethodRegistry(
                registry => registry
                    .AddConnectionMethods()
                    .AddChannelMethods()
                    .AddExchangeMethods()
                    .AddQueueMethods()
                    .AddBasicMethods()
                    .AddConfirmMethods());


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
}