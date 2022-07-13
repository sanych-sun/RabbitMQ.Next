namespace RabbitMQ.Next;

public interface IAuthMechanism
{
    string Type { get; }

    string ToResponse();
}