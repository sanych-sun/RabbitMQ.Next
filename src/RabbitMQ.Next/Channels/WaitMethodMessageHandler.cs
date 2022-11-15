using System;
using System.Threading.Tasks;
using RabbitMQ.Next.Messaging;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Channels;

internal class WaitMethodMessageHandler<TMethod> : IMessageHandler<TMethod>
    where TMethod: struct, IIncomingMethod
{
    private readonly TaskCompletionSource<TMethod> tcs;

    public WaitMethodMessageHandler()
    {
        this.tcs = new TaskCompletionSource<TMethod>(TaskCreationOptions.RunContinuationsAsynchronously);
    }

    public Task<TMethod> WaitTask => this.tcs.Task;


    public bool Handle(TMethod method, IPayload payload)
    {
        this.tcs.TrySetResult(method);
        return true;
    }

    public void Release(Exception ex = null)
    {
        if (ex == null)
        {
            this.tcs.TrySetCanceled();
        }
        else
        {
            this.tcs.TrySetException(ex);
        }
    }
}