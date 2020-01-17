using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next.Transport.Methods.Connection
{
    public static class MethodRegistryExtensions
    {
        public static IMethodRegistryBuilder AddConnectionMethods(this IMethodRegistryBuilder builder)
        {
            builder.Register<StartMethod>((uint)MethodId.ConnectionStart, registration => registration.Use(new StartMethodFrameParser()));
            builder.Register<StartOkMethod>((uint)MethodId.ConnectionStartOk, registration => registration.Use(new StartOkMethodFrameFormatter()));
            builder.Register<TuneMethod>((uint)MethodId.ConnectionTune, registration => registration.Use(new TuneMethodFrameParser()));
            builder.Register<TuneOkMethod>((uint)MethodId.ConnectionTuneOk, registration => registration.Use(new TuneOkMethodFrameFormatter()));
            builder.Register<OpenMethod>((uint)MethodId.ConnectionOpen, registration => registration.Use(new OpenMethodFrameFormatter()));
            builder.Register<OpenOkMethod>((uint)MethodId.ConnectionOpenOk, registration => registration.Use(new OpenOkMethodFrameParser()));
            builder.Register<CloseMethod>((uint) MethodId.ConnectionClose,
                registration => registration
                    .Use(new CloseMethodFrameFormatter())
                    .Use(new CloseMethodFrameParser()));

            builder.Register<CloseOkMethod>((uint) MethodId.ConnectionCloseOk,
                registration => registration
                    .Use(new CloseOkMethodFrameFormatter())
                    .Use(new CloseOkMethodFrameParser()));

            return builder;
        }
    }
}