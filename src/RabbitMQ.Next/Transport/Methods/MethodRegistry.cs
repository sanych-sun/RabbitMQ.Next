using System;
using System.Collections.Generic;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods;

internal static class MethodRegistry
{
    private static readonly Dictionary<Type, object> Formatters = new();
    private static readonly Dictionary<Type, object> Parsers = new();
    private static readonly Dictionary<Type, MethodId> Map = new();
    private static readonly HashSet<uint> MethodsWithContent = new();
    
    static MethodRegistry()
    {
        void Register<TMethod>(MethodId methodId, bool hasContent = false, IMethodFormatter<TMethod> formatter = null, IMethodParser<TMethod> parser = null)
            where TMethod : struct, IMethod
        {
            Map.Add(typeof(TMethod), methodId);
            
            if (hasContent)
            {
                MethodsWithContent.Add((uint)methodId);
            }

            if (formatter != null)
            {
                Formatters.Add(typeof(TMethod), formatter);
            }

            if (parser != null)
            {
                Parsers.Add(typeof(TMethod), parser);
            }
        }

        // ConnectionMethods
        Register(MethodId.ConnectionStart, parser: new Connection.StartMethodParser());
        Register(MethodId.ConnectionStartOk, formatter: new Connection.StartOkMethodFormatter());
        Register(MethodId.ConnectionTune, parser: new Connection.TuneMethodParser());
        Register(MethodId.ConnectionTuneOk, formatter: new Connection.TuneOkMethodFormatter());
        Register(MethodId.ConnectionOpen, formatter: new Connection.OpenMethodFormatter());
        Register(MethodId.ConnectionOpenOk, parser: new EmptyParser<Connection.OpenOkMethod>());
        Register(MethodId.ConnectionClose, formatter: new Connection.CloseMethodFormatter(), parser: new Connection.CloseMethodParser());
        Register(MethodId.ConnectionCloseOk, formatter: new EmptyFormatter<Connection.CloseOkMethod>(), parser: new EmptyParser<Connection.CloseOkMethod>());
        Register(MethodId.ConnectionBlocked, parser: new Connection.BlockedMethodParser());
        Register(MethodId.ConnectionUnblocked, parser: new EmptyParser<Connection.UnblockedMethod>());

        // ChannelMethods
        Register(MethodId.ChannelOpen, formatter: new Channel.OpenMethodFormatter());
        Register(MethodId.ChannelOpenOk, parser: new EmptyParser<Channel.OpenOkMethod>());
        Register(MethodId.ChannelFlow, formatter: new Channel.FlowMethodFormatter(), parser: new Channel.FlowMethodParser());
        Register(MethodId.ChannelFlowOk, formatter: new Channel.FlowOkMethodFormatter(), parser: new Channel.FlowOkMethodParser());
        Register(MethodId.ChannelClose, formatter: new Channel.CloseMethodFormatter(), parser: new Channel.CloseMethodParser());
        Register(MethodId.ChannelCloseOk, formatter: new EmptyFormatter<Channel.CloseOkMethod>(), parser: new EmptyParser<Channel.CloseOkMethod>());

        // ExchangeMethods
        Register(MethodId.ExchangeDeclare, formatter: new Exchange.DeclareMethodFormatter());
        Register(MethodId.ExchangeDeclareOk, parser: new EmptyParser<Exchange.DeclareOkMethod>());
        Register(MethodId.ExchangeBind, formatter: new Exchange.BindMethodFormatter());
        Register(MethodId.ExchangeBindOk, parser: new EmptyParser<Exchange.BindOkMethod>());
        Register(MethodId.ExchangeUnbind, formatter: new Exchange.UnbindMethodFormatter());
        Register(MethodId.ExchangeUnbindOk, parser: new EmptyParser<Exchange.UnbindOkMethod>());
        Register(MethodId.ExchangeDelete, formatter: new Exchange.DeleteMethodFormatter());
        Register(MethodId.ExchangeDeleteOk, parser: new EmptyParser<Exchange.DeleteOkMethod>());

        // QueueMethods
        Register(MethodId.QueueDeclare, formatter: new Queue.DeclareMethodFormatter());
        Register(MethodId.QueueDeclareOk, parser: new Queue.DeclareOkMethodParser());
        Register(MethodId.QueueBind, formatter: new Queue.BindMethodFormatter());
        Register(MethodId.QueueBindOk, parser: new EmptyParser<Queue.BindOkMethod>());
        Register(MethodId.QueueUnbind, formatter: new Queue.UnbindMethodFormatter());
        Register(MethodId.QueueUnbindOk, parser: new EmptyParser<Queue.UnbindOkMethod>());
        Register(MethodId.QueuePurge, formatter: new Queue.PurgeMethodFormatter());
        Register(MethodId.QueuePurgeOk, parser: new Queue.PurgeOkMethodParser());
        Register(MethodId.QueueDelete, formatter: new Queue.DeleteMethodFormatter());
        Register(MethodId.QueueDeleteOk, parser: new Queue.DeleteOkMethodParser());

        // BasicMethods
        Register(MethodId.BasicQos, formatter: new Basic.QosMethodFormatter());
        Register(MethodId.BasicQosOk, parser: new EmptyParser<Basic.QosOkMethod>());
        Register(MethodId.BasicConsume, formatter: new Basic.ConsumeMethodFormatter());
        Register(MethodId.BasicConsumeOk, parser: new Basic.ConsumeOkMethodParser());
        Register(MethodId.BasicCancel, formatter: new Basic.CancelMethodFormatter());
        Register(MethodId.BasicCancelOk, parser: new Basic.CancelOkMethodParser());
        Register(MethodId.BasicPublish, hasContent: true, formatter: new Basic.PublishMethodFormatter());
        Register(MethodId.BasicReturn, hasContent: true, parser: new Basic.ReturnMethodParser());
        Register(MethodId.BasicDeliver, hasContent: true, parser: new Basic.DeliverMethodParser());
        Register(MethodId.BasicGet, formatter: new Basic.GetMethodFormatter());
        Register(MethodId.BasicGetOk, parser: new Basic.GetOkMethodParser());
        Register(MethodId.BasicGetEmpty, parser: new EmptyParser<Basic.GetEmptyMethod>());
        Register(MethodId.BasicAck, formatter: new Basic.AckMethodFormatter(), parser: new Basic.AckMethodParser());
        Register(MethodId.BasicRecover, formatter: new Basic.RecoverMethodFormatter());
        Register(MethodId.BasicRecoverOk, parser: new EmptyParser<Basic.RecoverOkMethod>());
        Register(MethodId.BasicNack, formatter: new Basic.NackMethodFormatter(), parser: new Basic.NackMethodParser());
        
        // ConfirmMethods
        Register(MethodId.ConfirmSelect, formatter: new Confirm.SelectMethodFormatter());
        Register(MethodId.ConfirmSelectOk, parser: new EmptyParser<Confirm.SelectOkMethod>());
    }
    
    public static bool HasContent(MethodId methodId) => MethodsWithContent.Contains((uint)methodId);
    
    public static IMethodParser<TMethod> GetParser<TMethod>()
        where TMethod : struct, IIncomingMethod
    {
        if (Parsers.TryGetValue(typeof(TMethod), out var parser))
        {
            return (IMethodParser<TMethod>)parser;
        }
        
        throw new NotSupportedException();
    }

    public static IMethodFormatter<TMethod> GetFormatter<TMethod>()
        where TMethod : struct, IOutgoingMethod
    {
        if (Formatters.TryGetValue(typeof(TMethod), out var formatter))
        {
            return (IMethodFormatter<TMethod>)formatter;
        }
        
        throw new NotSupportedException();
    }

    public static MethodId GetMethodId<TMethod>()
        where TMethod : struct, IMethod
    {
        if (Map.TryGetValue(typeof(TMethod), out var methodId))
        {
            return methodId;
        }
        
        throw new NotSupportedException();
    }
}