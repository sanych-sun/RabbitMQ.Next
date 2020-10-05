using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public static class MethodRegistryExtensions
    {
        public static IMethodRegistryBuilder AddConnectionMethods(this IMethodRegistryBuilder builder)
        {
            builder.Register<StartMethod>((uint)MethodId.ConnectionStart, registration => registration.Use(new StartMethodParser()));
            builder.Register<StartOkMethod>((uint)MethodId.ConnectionStartOk, registration => registration.Use(new StartOkMethodFormatter()));
            builder.Register<TuneMethod>((uint)MethodId.ConnectionTune, registration => registration.Use(new TuneMethodParser()));
            builder.Register<TuneOkMethod>((uint)MethodId.ConnectionTuneOk, registration => registration.Use(new TuneOkMethodFormatter()));
            builder.Register<OpenMethod>((uint)MethodId.ConnectionOpen, registration => registration.Use(new OpenMethodFormatter()));
            builder.Register<OpenOkMethod>((uint)MethodId.ConnectionOpenOk, registration => registration.Use(new OpenOkMethodParser()));
            builder.Register<CloseMethod>((uint) MethodId.ConnectionClose,
                registration => registration
                    .Use(new CloseMethodFormatter())
                    .Use(new CloseMethodParser()));

            builder.Register<CloseOkMethod>((uint) MethodId.ConnectionCloseOk,
                registration => registration
                    .Use(new CloseOkMethodFormatter())
                    .Use(new CloseOkMethodParser()));

            return builder;
        }
    }
}