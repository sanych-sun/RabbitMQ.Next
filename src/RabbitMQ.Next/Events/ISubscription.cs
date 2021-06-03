using System.Threading.Tasks;

namespace RabbitMQ.Next.Events
{
    internal interface ISubscription<in TEvent>
    {
        ValueTask<bool> HandleAsync(TEvent eventArgs);
    }
}