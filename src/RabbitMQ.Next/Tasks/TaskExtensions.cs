using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Tasks;

public static class TaskExtensions
{
    public static bool Wait<T>(this ValueTask<T> valueTask, TimeSpan? timeout, out T result)
    {
        if (valueTask.IsCompleted)
        {
            result = valueTask.Result;
            return true;
        }
        
        var task = valueTask.AsTask();
        var timeoutMs = -1; // -1 means infinite
        if (timeout.HasValue)
        {
            timeoutMs = (int)timeout.Value.TotalMilliseconds;
        }
        
        if (task.Wait(timeoutMs))
        {
            result = task.Result;
            return true;
        }

        result = default;
        return false;
    }
}
