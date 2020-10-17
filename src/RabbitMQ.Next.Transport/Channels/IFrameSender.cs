using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Methods;

namespace RabbitMQ.Next.Transport.Channels
{
    public interface IFrameSender
    {
        // TODO: Add parameters or methods to sent content header and content body
        Task SendMethodAsync<TMethod>(TMethod method)
            where TMethod : struct, IOutgoingMethod;
    }
}