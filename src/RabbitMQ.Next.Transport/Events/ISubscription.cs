using System.Threading.Tasks;

namespace RabbitMQ.Next.Transport.Events
{
    internal interface ISubscription<in TEvent>
    {
        ValueTask<bool> HandleAsync(TEvent eventArgs);
    }
}