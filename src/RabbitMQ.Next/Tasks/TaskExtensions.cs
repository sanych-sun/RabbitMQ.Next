using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Tasks;

public static class TaskExtensions
{
    public static (bool IsCompleted, T Result) Wait<T>(this ValueTask<T> valueTask, TimeSpan? timeout)
    {
        if (valueTask.IsCompleted)
        {
            return (true, valueTask.Result);
        }
        
        var task = valueTask.AsTask();
        var timeoutMs = -1; // -1 means infinite
        if (timeout.HasValue)
        {
            timeoutMs = (int)timeout.Value.TotalMilliseconds;
        }
        
        if (task.Wait(timeoutMs))
        {
            return (true, task.Result);
        }

        return (false, default);
    }
}
