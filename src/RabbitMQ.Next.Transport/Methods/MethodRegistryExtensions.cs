using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next.Transport.Methods
{
    public static class MethodRegistryExtensions
    {
        public static IMethodRegistryBuilder AddConnectionMethods(this IMethodRegistryBuilder builder)
        {
            builder.Register<Connection.StartMethod>((uint)MethodId.ConnectionStart,
                registration => registration.Use(new Connection.StartMethodParser()));
            builder.Register<Connection.StartOkMethod>((uint)MethodId.ConnectionStartOk,
                registration => registration.Use(new Connection.StartOkMethodFormatter()));

            builder.Register<Connection.TuneMethod>((uint)MethodId.ConnectionTune,
                registration => registration.Use(new Connection.TuneMethodParser()));
            builder.Register<Connection.TuneOkMethod>((uint)MethodId.ConnectionTuneOk,
                registration => registration.Use(new Connection.TuneOkMethodFormatter()));

            builder.Register<Connection.OpenMethod>((uint)MethodId.ConnectionOpen,
                registration => registration.Use(new Connection.OpenMethodFormatter()));
            builder.Register<Connection.OpenOkMethod>((uint)MethodId.ConnectionOpenOk,
                registration => registration.Use(new Connection.OpenOkMethodParser()));

            builder.Register<Connection.CloseMethod>((uint) MethodId.ConnectionClose,
                registration => registration
                    .Use(new Connection.CloseMethodFormatter())
                    .Use(new Connection.CloseMethodParser()));
            builder.Register<Connection.CloseOkMethod>((uint) MethodId.ConnectionCloseOk,
                registration => registration
                    .Use(new Connection.CloseOkMethodFormatter())
                    .Use(new Connection.CloseOkMethodParser()));

            return builder;
        }

        public static IMethodRegistryBuilder AddChannelMethods(this IMethodRegistryBuilder builder)
        {
            builder.Register<Channel.OpenMethod>((uint) MethodId.ChannelOpen,
                registration => registration.Use(new Channel.OpenMethodFormatter()));
            builder.Register<Channel.OpenOkMethod>((uint) MethodId.ChannelOpenOk,
                registration => registration.Use(new Channel.OpenOkMethodParser()));

            builder.Register<Channel.FlowMethod>((uint) MethodId.ChannelFlow,
                registration => registration
                    .Use(new Channel.FlowMethodFormatter())
                    .Use(new Channel.FlowMethodParser()));
            builder.Register<Channel.FlowOkMethod>((uint) MethodId.ChannelFlowOk,
                registration => registration
                    .Use(new Channel.FlowOkMethodFormatter())
                    .Use(new Channel.FlowOkMethodParser()));

            builder.Register<Channel.CloseMethod>((uint) MethodId.ChannelClose,
                registration => registration
                    .Use(new Channel.CloseMethodFormatter())
                    .Use(new Channel.CloseMethodParser()));
            builder.Register<Channel.CloseOkMethod>((uint) MethodId.ChannelCloseOk,
                registration => registration
                    .Use(new Channel.CloseOkMethodFormatter())
                    .Use(new Channel.CloseOkMethodParser()));

            return builder;
        }
    }
}