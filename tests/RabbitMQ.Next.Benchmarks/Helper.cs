using System;
using System.Text;

namespace RabbitMQ.Next.Benchmarks;

internal static class Helper
{
    private const string TextFragment = "Lorem ipsum dolor sit amet, ne putent ornatus expetendis vix. Ea sed suas accusamus. Possim prodesset maiestatis sea te, graeci tractatos evertitur ad vix, sit an sale regione facilisi. Vel cu suscipit perfecto voluptaria. Diam soleat eos ex, his liber causae saperet et.";

    public static string BuildDummyText(int length)
    {
        var builder = new StringBuilder(length);

        while (builder.Length < length)
        {
            builder.Append(TextFragment);
        }

        return builder.ToString(0, length);
    }

    // TODO: find a proper way to store the connection in config
    public static Uri RabbitMqConnection { get; } = new Uri("amqp://test2:test2@localhost:5672/");
}