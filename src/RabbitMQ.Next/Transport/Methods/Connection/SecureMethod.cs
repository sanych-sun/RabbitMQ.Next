using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection;

public readonly struct SecureMethod : IIncomingMethod
{
    public SecureMethod(ReadOnlyMemory<byte> challenge)
    {
        this.Challenge = challenge;
    }
    
    public MethodId MethodId => MethodId.ConnectionSecure;
    
    public ReadOnlyMemory<byte> Challenge { get; }
}