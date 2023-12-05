using System;
using RabbitMQ.Next.Methods;

namespace RabbitMQ.Next.Transport.Methods.Connection;

public readonly struct SecureOkMethod : IOutgoingMethod
{
    public SecureOkMethod(ReadOnlyMemory<byte> response)
    {
        this.Response = response;
    }
    
    public MethodId MethodId => MethodId.ConnectionSecureOk;
    
    public ReadOnlyMemory<byte> Response { get; }
}