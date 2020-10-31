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
                registration => registration.Use(new EmptyArgsParser<Channel.OpenOkMethod>()));

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
                    .Use(new EmptyArgsFormatter<Channel.CloseOkMethod>())
                    .Use(new EmptyArgsParser<Channel.CloseOkMethod>()));

            return builder;
        }

        public static IMethodRegistryBuilder AddExchangeMethods(this IMethodRegistryBuilder builder)
        {
            builder.Register<Exchange.DeclareMethod>((uint) MethodId.ExchangeDeclare,
                registration => registration.Use(new Exchange.DeclareMethodFormatter()));
            builder.Register<Exchange.DeclareOkMethod>((uint) MethodId.ExchangeDeclareOk,
                registration => registration.Use(new EmptyArgsParser<Exchange.DeclareOkMethod>()));

            builder.Register<Exchange.BindMethod>((uint) MethodId.ExchangeBind,
                registration => registration.Use(new Exchange.BindMethodFormatter()));
            builder.Register<Exchange.BindOkMethod>((uint) MethodId.ExchangeBindOk,
                registration => registration.Use(new EmptyArgsParser<Exchange.BindOkMethod>()));
            
            builder.Register<Exchange.UnbindMethod>((uint) MethodId.ExchangeUnbind,
                registration => registration.Use(new Exchange.UnbindMethodFormatter()));
            builder.Register<Exchange.UnbindOkMethod>((uint) MethodId.ExchangeUnbindOk,
                registration => registration.Use(new EmptyArgsParser<Exchange.UnbindOkMethod>()));
            
            builder.Register<Exchange.DeleteMethod>((uint) MethodId.ExchangeDelete,
                registration => registration.Use(new Exchange.DeleteMethodFormatter()));
            builder.Register<Exchange.DeleteOkMethod>((uint) MethodId.ExchangeDeleteOk,
                registration => registration.Use(new EmptyArgsParser<Exchange.DeleteOkMethod>()));
            
            return builder;
        }

        public static IMethodRegistryBuilder AddQueueMethods(this IMethodRegistryBuilder builder)
        {
            builder.Register<Queue.DeclareMethod>((uint) MethodId.QueueDeclare,
                registration => registration.Use(new Queue.DeclareMethodFormatter()));
            builder.Register<Queue.DeclareOkMethod>((uint) MethodId.QueueDeclareOk,
                registration => registration.Use(new Queue.DeclareOkMethodParser()));

            builder.Register<Queue.BindMethod>((uint) MethodId.QueueBind,
                registration => registration.Use(new Queue.BindMethodFormatter()));
            builder.Register<Queue.BindOkMethod>((uint) MethodId.QueueBindOk,
                registration => registration.Use(new EmptyArgsParser<Queue.BindOkMethod>()));
            
            builder.Register<Queue.UnbindMethod>((uint) MethodId.QueueUnbind,
                registration => registration.Use(new Queue.UnbindMethodFormatter()));
            builder.Register<Queue.UnbindOkMethod>((uint) MethodId.QueueUnbindOk,
                registration => registration.Use(new EmptyArgsParser<Queue.UnbindOkMethod>()));
            
            builder.Register<Queue.PurgeMethod>((uint) MethodId.QueuePurge,
                registration => registration.Use(new Queue.PurgeMethodFormatter()));
            builder.Register<Queue.PurgeOkMethod>((uint) MethodId.QueuePurgeOk,
                registration => registration.Use(new Queue.PurgeOkMethodParser()));

            builder.Register<Queue.DeleteMethod>((uint) MethodId.QueueDelete,
                registration => registration.Use(new Queue.DeleteMethodFormatter()));
            builder.Register<Queue.DeleteOkMethod>((uint) MethodId.QueueDeleteOk,
                registration => registration.Use(new Queue.DeleteOkMethodParser()));
            
            return builder;
        }

        public static IMethodRegistryBuilder AddBasicMethods(this IMethodRegistryBuilder builder)
        {
            builder.Register<Basic.PublishMethod>((uint) MethodId.BasicPublish,
                registration => registration
                    .HasContent()
                    .Use(new Basic.PublishMethodFormatter()));

            return builder;
        }
    }
}