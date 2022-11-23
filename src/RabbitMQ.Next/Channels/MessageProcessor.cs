using System;
using System.Collections.Generic;
using RabbitMQ.Next.Methods;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Channels;

internal class MessageProcessor<TMethod> : IMessageProcessor
    where TMethod: struct, IIncomingMethod
{
    private readonly List<IMessageHandler<TMethod>> handlers = new();
    
    public IDisposable WithMessageHandler<T>(IMessageHandler<T> handler)
        where T : struct, IIncomingMethod
    {
        if (typeof(TMethod) != typeof(T))
        {
            throw new InvalidCastException();
        }

        var typed = (IMessageHandler<TMethod>)handler; 
        this.handlers.Add(typed);
        return new HandlerDisposer(this.handlers, typed);
    }

    public bool ProcessMessage(ReadOnlyMemory<byte> methodArgs, PayloadAccessor payload)
    {
        if (this.handlers.Count == 0)
        {
            return false;
        }
            
        var args = methodArgs.ParseMethodArgs<TMethod>();

        for (var i = 0; i < this.handlers.Count; i++)
        {
            var handled = this.handlers[i].Handle(args, payload);
            if (handled)
            {
                return true;
            }
        }

        return false;
    }

    public void Release(Exception ex = null)
    {
        foreach (var handler in this.handlers)
        {
            handler.Release(ex);
        }
            
        this.handlers.Clear();
    }
        
    private sealed class HandlerDisposer : IDisposable
    {
        private readonly List<IMessageHandler<TMethod>> handlers;
        private IMessageHandler<TMethod> handler;

        public HandlerDisposer(List<IMessageHandler<TMethod>> handlers, IMessageHandler<TMethod> handler)
        {
            this.handlers = handlers;
            this.handler = handler;
        }


        public void Dispose()
        {
            if (this.handler == null)
            {
                return;
            }
                
            this.handlers.Remove(this.handler);
            this.handler = null;
        }
    }
}