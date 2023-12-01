using System;
using System.Threading.Tasks;

namespace RabbitMQ.Next;

public interface IAuthMechanism
{
    string Type { get; }

    ValueTask<ReadOnlyMemory<byte>> HandleChallengeAsync(ReadOnlySpan<byte> challenge);
}