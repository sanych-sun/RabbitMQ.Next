namespace RabbitMQ.Next.Abstractions
{
    public interface IAuthMechanism
    {
        string Type { get; }

        string ToResponse();
    }
}