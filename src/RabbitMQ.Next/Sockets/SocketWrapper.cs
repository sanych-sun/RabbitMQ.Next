using System.IO;
using System.Net.Security;
using System.Net.Sockets;
using RabbitMQ.Next.Buffers;

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

    public void Send(IMemoryAccessor payload)
    {
        var current = payload;
        while (current != null)
        {
            current.WriteTo(this.writeStream);
            current = current.Next;
        }
        
        this.writeStream.Flush();
    }
    
    public int Receive(byte[] buffer, int offset, int minBytes)
    {
        var received = 0;
        while (received < minBytes)
        {
            var readBytes = this.readStream.Read(buffer, offset, buffer.Length - offset);
            if (readBytes == 0 && this.IsConnectionClosedByServer())
            {
                throw new SocketException();
            }

            offset += readBytes;
            received += readBytes;
        }

        return received;
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