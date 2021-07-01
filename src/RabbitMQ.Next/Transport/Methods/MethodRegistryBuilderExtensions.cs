using RabbitMQ.Next.Abstractions;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Methods
{
    public static class MethodRegistryBuilderExtensions
    {
        public static IMethodRegistryBuilder AddConnectionMethods(this IMethodRegistryBuilder builder)
        {
            builder.Register<Connection.StartMethod>(MethodId.ConnectionStart,
                registration => registration.Sync().Use(new Connection.StartMethodParser()));
            builder.Register<Connection.StartOkMethod>(MethodId.ConnectionStartOk,
                registration => registration.Sync().Use(new Connection.StartOkMethodFormatter()));

            builder.Register<Connection.TuneMethod>(MethodId.ConnectionTune,
                registration => registration.Sync().Use(new Connection.TuneMethodParser()));
            builder.Register<Connection.TuneOkMethod>(MethodId.ConnectionTuneOk,
                registration => registration.Sync().Use(new Connection.TuneOkMethodFormatter()));

            builder.Register<Connection.OpenMethod>(MethodId.ConnectionOpen,
                registration => registration.Sync().Use(new Connection.OpenMethodFormatter()));
            builder.Register<Connection.OpenOkMethod>(MethodId.ConnectionOpenOk,
                registration => registration.Sync().Use(new EmptyArgsParser<Connection.OpenOkMethod>()));

            builder.Register<Connection.CloseMethod>(MethodId.ConnectionClose,
                registration => registration
                    .Sync()
                    .Use(new Connection.CloseMethodFormatter())
                    .Use(new Connection.CloseMethodParser()));
            builder.Register<Connection.CloseOkMethod>(MethodId.ConnectionCloseOk,
                registration => registration
                    .Sync()
                    .Use(new EmptyArgsFormatter<Connection.CloseOkMethod>())
                    .Use(new EmptyArgsParser<Connection.CloseOkMethod>()));

            builder.Register<Connection.BlockedMethod>(MethodId.ConnectionBlocked,
                registration => registration.Use(new Connection.BlockedMethodParser()));

            builder.Register<Connection.UnblockedMethod>(MethodId.ConnectionUnblocked,
                registration => registration.Use(new EmptyArgsParser<Connection.UnblockedMethod>()));

            return builder;
        }

        public static IMethodRegistryBuilder AddChannelMethods(this IMethodRegistryBuilder builder)
        {
            builder.Register<Channel.OpenMethod>(MethodId.ChannelOpen,
                registration => registration.Sync().Use(new Channel.OpenMethodFormatter()));
            builder.Register<Channel.OpenOkMethod>(MethodId.ChannelOpenOk,
                registration => registration.Sync().Use(new EmptyArgsParser<Channel.OpenOkMethod>()));

            builder.Register<Channel.FlowMethod>(MethodId.ChannelFlow,
                registration => registration
                    .Sync()
                    .Use(new Channel.FlowMethodFormatter())
                    .Use(new Channel.FlowMethodParser()));
            builder.Register<Channel.FlowOkMethod>(MethodId.ChannelFlowOk,
                registration => registration
                    .Use(new Channel.FlowOkMethodFormatter())
                    .Use(new Channel.FlowOkMethodParser()));

            builder.Register<Channel.CloseMethod>(MethodId.ChannelClose,
                registration => registration
                    .Sync()
                    .Use(new Channel.CloseMethodFormatter())
                    .Use(new Channel.CloseMethodParser()));
            builder.Register<Channel.CloseOkMethod>(MethodId.ChannelCloseOk,
                registration => registration
                    .Sync()
                    .Use(new EmptyArgsFormatter<Channel.CloseOkMethod>())
                    .Use(new EmptyArgsParser<Channel.CloseOkMethod>()));

            return builder;
        }

        public static IMethodRegistryBuilder AddExchangeMethods(this IMethodRegistryBuilder builder)
        {
            builder.Register<Exchange.DeclareMethod>(MethodId.ExchangeDeclare,
                registration => registration.Sync().Use(new Exchange.DeclareMethodFormatter()));
            builder.Register<Exchange.DeclareOkMethod>(MethodId.ExchangeDeclareOk,
                registration => registration.Sync().Use(new EmptyArgsParser<Exchange.DeclareOkMethod>()));

            builder.Register<Exchange.BindMethod>(MethodId.ExchangeBind,
                registration => registration.Sync().Use(new Exchange.BindMethodFormatter()));
            builder.Register<Exchange.BindOkMethod>(MethodId.ExchangeBindOk,
                registration => registration.Sync().Use(new EmptyArgsParser<Exchange.BindOkMethod>()));
            
            builder.Register<Exchange.UnbindMethod>(MethodId.ExchangeUnbind,
                registration => registration.Sync().Use(new Exchange.UnbindMethodFormatter()));
            builder.Register<Exchange.UnbindOkMethod>(MethodId.ExchangeUnbindOk,
                registration => registration.Sync().Use(new EmptyArgsParser<Exchange.UnbindOkMethod>()));
            
            builder.Register<Exchange.DeleteMethod>(MethodId.ExchangeDelete,
                registration => registration.Sync().Use(new Exchange.DeleteMethodFormatter()));
            builder.Register<Exchange.DeleteOkMethod>(MethodId.ExchangeDeleteOk,
                registration => registration.Sync().Use(new EmptyArgsParser<Exchange.DeleteOkMethod>()));
            
            return builder;
        }

        public static IMethodRegistryBuilder AddQueueMethods(this IMethodRegistryBuilder builder)
        {
            builder.Register<Queue.DeclareMethod>(MethodId.QueueDeclare,
                registration => registration.Sync().Use(new Queue.DeclareMethodFormatter()));
            builder.Register<Queue.DeclareOkMethod>(MethodId.QueueDeclareOk,
                registration => registration.Sync().Use(new Queue.DeclareOkMethodParser()));

            builder.Register<Queue.BindMethod>(MethodId.QueueBind,
                registration => registration.Sync().Use(new Queue.BindMethodFormatter()));
            builder.Register<Queue.BindOkMethod>(MethodId.QueueBindOk,
                registration => registration.Sync().Use(new EmptyArgsParser<Queue.BindOkMethod>()));
            
            builder.Register<Queue.UnbindMethod>(MethodId.QueueUnbind,
                registration => registration.Sync().Use(new Queue.UnbindMethodFormatter()));
            builder.Register<Queue.UnbindOkMethod>(MethodId.QueueUnbindOk,
                registration => registration.Sync().Use(new EmptyArgsParser<Queue.UnbindOkMethod>()));
            
            builder.Register<Queue.PurgeMethod>(MethodId.QueuePurge,
                registration => registration.Sync().Use(new Queue.PurgeMethodFormatter()));
            builder.Register<Queue.PurgeOkMethod>(MethodId.QueuePurgeOk,
                registration => registration.Sync().Use(new Queue.PurgeOkMethodParser()));

            builder.Register<Queue.DeleteMethod>(MethodId.QueueDelete,
                registration => registration.Sync().Use(new Queue.DeleteMethodFormatter()));
            builder.Register<Queue.DeleteOkMethod>(MethodId.QueueDeleteOk,
                registration => registration.Sync().Use(new Queue.DeleteOkMethodParser()));
            
            return builder;
        }

        public static IMethodRegistryBuilder AddBasicMethods(this IMethodRegistryBuilder builder)
        {
            builder.Register<Basic.QosMethod>(MethodId.BasicQos,
                registration => registration.Sync().Use(new Basic.QosMethodFormatter()));

            builder.Register<Basic.QosOkMethod>(MethodId.BasicQosOk,
                registration => registration.Sync().Use(new EmptyArgsParser<Basic.QosOkMethod>()));

            builder.Register<Basic.ConsumeMethod>(MethodId.BasicConsume,
                registration => registration.Sync().Use(new Basic.ConsumeMethodFormatter()));
            
            builder.Register<Basic.ConsumeOkMethod>(MethodId.BasicConsumeOk,
                registration => registration.Sync().Use(new Basic.ConsumeOkMethodParser()));
            
            builder.Register<Basic.CancelMethod>(MethodId.BasicCancel,
                registration => registration.Sync().Use(new Basic.CancelMethodFormatter()));
            
            builder.Register<Basic.CancelOkMethod>(MethodId.BasicCancelOk,
                registration => registration.Sync().Use(new Basic.CancelOkMethodParser()));

            builder.Register<Basic.PublishMethod>(MethodId.BasicPublish,
                registration => registration
                    .HasContent()
                    .Use(new Basic.PublishMethodFormatter()));

            builder.Register<Basic.ReturnMethod>(MethodId.BasicReturn,
                registration => registration
                    .HasContent()
                    .Use(new Basic.ReturnMethodParser()));

            builder.Register<Basic.DeliverMethod>(MethodId.BasicDeliver,
                registration => registration
                    .HasContent()
                    .Use(new Basic.DeliverMethodParser()));
            
            builder.Register<Basic.GetMethod>(MethodId.BasicGet,
                registration => registration.Sync().Use(new Basic.GetMethodFormatter()));
            
            builder.Register<Basic.GetOkMethod>(MethodId.BasicGetOk,
                registration => registration.Sync().Use(new Basic.GetOkMethodParser()));
            
            builder.Register<Basic.GetEmptyMethod>(MethodId.BasicGetEmpty,
                registration => registration.Sync().Use(new EmptyArgsParser<Basic.GetEmptyMethod>()));
            
            builder.Register<Basic.AckMethod>(MethodId.BasicAck,
                registration => registration
                    .Use(new Basic.AckMethodFormatter())
                    .Use(new Basic.AckMethodParser()));
            
            builder.Register<Basic.RecoverMethod>(MethodId.BasicRecover,
                registration => registration.Use(new Basic.RecoverMethodFormatter()));

            builder.Register<Basic.RecoverOkMethod>(MethodId.BasicRecoverOk,
                registration => registration.Use(new EmptyArgsParser<Basic.RecoverOkMethod>()));

            builder.Register<Basic.NackMethod>(MethodId.BasicNack,
                registration => registration
                    .Use(new Basic.NackMethodFormatter())
                    .Use(new Basic.NackMethodParser()));

            return builder;
        }

        public static IMethodRegistryBuilder AddConfirmMethods(this IMethodRegistryBuilder builder)
        {
            builder.Register<Confirm.SelectMethod>(MethodId.ConfirmSelect,
                registration => registration.Sync().Use(new Confirm.SelectMethodFormatter()));

            builder.Register<Confirm.SelectOkMethod>(MethodId.ConfirmSelectOk,
                registration => registration.Sync().Use(new EmptyArgsParser<Confirm.SelectOkMethod>()));

            return builder;
        }
    }
}