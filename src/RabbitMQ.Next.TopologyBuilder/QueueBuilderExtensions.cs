using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.TopologyBuilder.Abstractions;
using RabbitMQ.Next.Transport.Methods.Queue;

namespace RabbitMQ.Next.TopologyBuilder
{
    internal static class QueueBuilderExtensions
    {
        public static async Task ApplyAsync(this QueueBuilder builder, IChannel channel)
        {
            await channel.SendAsync<DeclareMethod, DeclareOkMethod>(builder.ToMethod());

            if (builder.Bindings == null)
            {
                return;
            }

            for (var index = 0; index < builder.Bindings.Count; index++)
            {
                var binding = builder.Bindings[index];

                await channel.SendAsync<BindMethod, BindOkMethod>(binding.ToMethod());
            }
        }
    }
}