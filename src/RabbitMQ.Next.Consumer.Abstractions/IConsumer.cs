using System.Threading;
using System.Threading.Tasks;

namespace RabbitMQ.Next.Consumer.Abstractions
{
    public interface IConsumer
    {
        Task ConsumeAsync(CancellationToken cancellation);

        Task AckAsync(string deliveryTag, bool multiple);

        Task NackAsync(string deliveryTag, bool multiple);
    }
}