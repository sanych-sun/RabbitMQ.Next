using System;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Channels;

internal class WaitMethodMessageHandler<TMethod> : IMessageHandler<TMethod>
    where TMethod: struct, IIncomingMethod
{
    private readonly TaskCompletionSource<TMethod> tcs;

    public WaitMethodMessageHandler(CancellationToken cancellation)
    {
        this.tcs = new TaskCompletionSource<TMethod>(TaskCreationOptions.RunContinuationsAsynchronously);
        if (cancellation.CanBeCanceled)
        {
            cancellation.Register(state => ((TaskCompletionSource<TMethod>)state).TrySetCanceled(), this.tcs);
        }
    }

    public Task<TMethod> WaitTask => this.tcs.Task;


    public void Handle(TMethod method, IPayload payload)
    {
        this.tcs.TrySetResult(method);
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