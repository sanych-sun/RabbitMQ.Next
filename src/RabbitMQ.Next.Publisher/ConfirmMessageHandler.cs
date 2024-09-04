using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Channels;
using RabbitMQ.Next.Tasks;
using RabbitMQ.Next.Transport.Methods.Basic;

namespace RabbitMQ.Next.Publisher;

internal class ConfirmMessageHandler : IMessageHandler<AckMethod>, IMessageHandler<NackMethod> 
{
    private static readonly TaskCompletionSource<bool> PositiveCompletedTcs;
    private static readonly TaskCompletionSource<bool> NegativeCompletedTcs;

    private readonly ConcurrentDictionary<ulong, TaskCompletionSource<bool>> pendingConfirms = new();

    static ConfirmMessageHandler()
    {
        PositiveCompletedTcs = new TaskCompletionSource<bool>();
        PositiveCompletedTcs.SetResult(true);

        NegativeCompletedTcs = new TaskCompletionSource<bool>();
        NegativeCompletedTcs.SetResult(false);
    }

    public Task<bool> WaitForConfirmAsync(ulong deliveryTag, CancellationToken cancellation)
    {
        var tcs = this.pendingConfirms.GetOrAdd(deliveryTag, _ => new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously));
        if (tcs.Task.IsCompleted)
        {
            this.pendingConfirms.TryRemove(deliveryTag, out _);
        }

        return tcs.Task.WaitAsync(cancellation);
    }

    public void Handle(AckMethod method, IPayload payload)
    {
        if (method.Multiple)
        {
            this.AckMultiple(method.DeliveryTag, true);
        }
        else
        {
            this.AckSingle(method.DeliveryTag, true);
        }
    }

    public void Handle(NackMethod method, IPayload payload)
    {
        if (method.Multiple)
        {
            this.AckMultiple(method.DeliveryTag, false);
        }
        else
        {
            this.AckSingle(method.DeliveryTag, false);
        }
    }

    public void Release(Exception ex)
    {
        foreach (var task in this.pendingConfirms)
        {
            if (ex == null)
            {
                task.Value.TrySetCanceled();
            }
            else
            {
                task.Value.TrySetException(ex);
            }
        }
        this.pendingConfirms.Clear();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AckSingle(ulong deliveryTag, bool isPositive)
    {
        if (!this.pendingConfirms.TryRemove(deliveryTag, out var tcs))
        {
            tcs = this.pendingConfirms.GetOrAdd(deliveryTag, isPositive ? PositiveCompletedTcs : NegativeCompletedTcs);
        }

        tcs.TrySetResult(isPositive);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AckMultiple(ulong deliveryTag, bool isPositive)
    {
        var items = this.pendingConfirms.Where(t => t.Key <= deliveryTag).ToArray();

        foreach (var (key, value) in items)
        {
            value.TrySetResult(isPositive);
            this.pendingConfirms.TryRemove(key, out _);
        }
    }
}
