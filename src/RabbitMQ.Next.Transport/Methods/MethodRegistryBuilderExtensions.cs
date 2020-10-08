using RabbitMQ.Next.Transport.Methods.Channel;
using RabbitMQ.Next.Transport.Methods.Exchange;
using RabbitMQ.Next.Transport.Methods.Registry;

namespace RabbitMQ.Next.Transport.Methods
{
    public static class MethodRegistryBuilderExtensions
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
                registration => registration.Use(new EmptyArgsParser<Connection.OpenOkMethod>()));

            builder.Register<Connection.CloseMethod>((uint) MethodId.ConnectionClose,
                registration => registration
                    .Use(new Connection.CloseMethodFormatter())
                    .Use(new Connection.CloseMethodParser()));
            builder.Register<Connection.CloseOkMethod>((uint) MethodId.ConnectionCloseOk,
                registration => registration
                    .Use(new EmptyArgsFormatter<Connection.CloseOkMethod>())
                    .Use(new EmptyArgsParser<Connection.CloseOkMethod>()));

            return builder;
        }

        public static IMethodRegistryBuilder AddChannelMethods(this IMethodRegistryBuilder builder)
        {
            builder.Register<Channel.OpenMethod>((uint) MethodId.ChannelOpen,
                registration => registration.Use(new Channel.OpenMethodFormatter()));
            builder.Register<Channel.OpenOkMethod>((uint) MethodId.ChannelOpenOk,
                registration => registration.Use(new EmptyArgsParser<OpenOkMethod>()));

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
                    .Use(new EmptyArgsFormatter<CloseOkMethod>())
                    .Use(new EmptyArgsParser<CloseOkMethod>()));

            return builder;
        }

        public static IMethodRegistryBuilder AddExchangeMethods(this IMethodRegistryBuilder builder)
        {
            builder.Register<Exchange.DeclareMethod>((uint) MethodId.ExchangeDeclare,
                registration => registration.Use(new Exchange.DeclareMethodFormatter()));
            builder.Register<Exchange.DeclareOkMethod>((uint) MethodId.ExchangeDeclareOk,
                registration => registration.Use(new EmptyArgsParser<DeclareOkMethod>()));

            builder.Register<Exchange.BindMethod>((uint) MethodId.ExchangeBind,
                registration => registration.Use(new Exchange.BindMethodFormatter()));
            builder.Register<Exchange.BindOkMethod>((uint) MethodId.ExchangeBindOk,
                registration => registration.Use(new EmptyArgsParser<BindOkMethod>()));
            
            builder.Register<Exchange.UnbindMethod>((uint) MethodId.ExchangeUnbind,
                registration => registration.Use(new Exchange.UnbindMethodFormatter()));
            builder.Register<Exchange.UnbindOkMethod>((uint) MethodId.ExchangeUnbindOk,
                registration => registration.Use(new EmptyArgsParser<UnbindOkMethod>()));
            
            builder.Register<Exchange.DeleteMethod>((uint) MethodId.ExchangeDelete,
                registration => registration.Use(new Exchange.DeleteMethodFormatter()));
            builder.Register<Exchange.DeleteOkMethod>((uint) MethodId.ExchangeDeleteOk,
                registration => registration.Use(new EmptyArgsParser<DeleteOkMethod>()));
            
            return builder;
        }
    }
}