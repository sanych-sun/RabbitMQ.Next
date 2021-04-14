using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Transport
{
    public static class AsyncManualResetEventExtensions
    {
        public static void SetAfter(this AsyncManualResetEvent wait, TimeSpan duration)
        {
            var delayTask = Task.Delay(duration);

            Task.WhenAny(wait.WaitAsync().AsTask(), delayTask)
                .ContinueWith(finished =>
                {
                    if (finished.Result == delayTask)
                    {
                        wait.Set();
                    }
                });
        }
    }
}