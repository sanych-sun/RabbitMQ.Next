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

    public void Send(MemoryBlock payload)
    {
        var current = payload;
        while (current != null)
        {
            this.writeStream.Write(current.Buffer, current.Start, current.Length);
            current = current.Next;
        }
        
        this.writeStream.Flush();
    }
    
    public void Receive(MemoryBlock buffer)
    {
        var received = 0;
        while (received < buffer.Length)
        {
            var readBytes = this.readStream.Read(buffer.Buffer, received, buffer.Length - received);
            if (readBytes == 0 && this.IsConnectionClosedByServer())
            {
                throw new SocketException();
            }
            
            received += readBytes;
        }
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