using System.Threading.Tasks;
using RabbitMQ.Next.Abstractions.Channels;
using RabbitMQ.Next.Transport.Methods.Exchange;

namespace RabbitMQ.Next.TopologyBuilder
{
    internal static class ExchangeBuilderExtensions
    {
        public static async Task ApplyAsync(this ExchangeBuilder builder, IChannel channel)
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