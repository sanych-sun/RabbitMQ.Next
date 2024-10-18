using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace RabbitMQ.Next.DependencyInjection;

public static class ServiceCollectionExtensions
{
#if NET8_0_OR_GREATER
    public static IServiceCollection AddRabbitMQConnection(
        this IServiceCollection serviceCollection,
        Action<IConnectionBuilder> connectionBuilder,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        object serviceKey = null)
    {
        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(IConnection),
                serviceKey,
                (sp, _) =>
                {
                    var builder = ConnectionBuilder.Default;
                    connectionBuilder(builder);
                    return builder.Build();
                },
                lifetime));

        return serviceCollection;
    }
#else
    public static IServiceCollection AddRabbitMQConnection(
        this IServiceCollection serviceCollection,
        Action<IConnectionBuilder> connectionBuilder,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        serviceCollection.TryAdd(
            new ServiceDescriptor(
                typeof(IConnection),
                sp =>
                {
                    var builder = ConnectionBuilder.Default;
                    connectionBuilder(builder);
                    return builder.Build();
                },
                lifetime));

        return serviceCollection;
    }
#endif
}
