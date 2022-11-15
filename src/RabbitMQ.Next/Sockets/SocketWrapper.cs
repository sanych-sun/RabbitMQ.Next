using System;
using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using RabbitMQ.Next.Transport;

namespace RabbitMQ.Next.Sockets;

internal class SocketWrapper : ISocket
{
    private readonly Socket socket;
    private readonly Stream readStream;
    private readonly Stream writeStream;

    public SocketWrapper(Socket socket, Endpoint endpoint)
    {
        this.socket = socket;

        Stream stream = new NetworkStream(socket)
        {
            ReadTimeout = 60000,
            WriteTimeout = 60000,
        };

        if (endpoint.UseSsl)
        {
            var sslStream = new SslStream(stream, false);
            sslStream.AuthenticateAsClient(endpoint.Host);

            stream = sslStream;
        }

        this.readStream = stream;
        this.writeStream = stream;
    }

    public void Send(ArraySegment<byte> payload)
        => this.writeStream.Write(payload.Array, payload.Offset, payload.Count);

    public void Flush() => this.writeStream.Flush();

    public int Receive(ArraySegment<byte> buffer)
    {
        var result = this.readStream.Read(buffer.Array, 0 , buffer.Count);

        if (result == 0 && this.IsConnectionClosedByServer())
        {
            throw new SocketException();
        }

        return result;
    }

    private bool IsConnectionClosedByServer()
        => this.socket.Poll(1000, SelectMode.SelectRead) && this.socket.Available == 0;

    public void Dispose()
    {
        this.readStream?.Dispose();
        this.writeStream?.Dispose();
        this.socket?.Dispose();
    }
}