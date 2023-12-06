using System.IO;
using System.Net.Sockets;
using RabbitMQ.Next.Buffers;

namespace RabbitMQ.Next.Sockets;

internal class SocketWrapper : ISocket
{
    private readonly Socket socket;
    private readonly Stream stream;

    public SocketWrapper(Socket socket, Stream stream)
    {
        this.socket = socket;
        this.stream = stream;
    }

    public void Send(IMemoryAccessor payload)
    {
        var current = payload;
        while (current != null)
        {
            current.WriteTo(this.stream);
            current = current.Next;
        }
        
        this.stream.Flush();
    }
    
    public int Receive(byte[] buffer, int offset, int minBytes)
    {
        var received = 0;
        while (received < minBytes)
        {
            var readBytes = this.stream.Read(buffer, offset, buffer.Length - offset);
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
        this.stream.Dispose();
        this.socket?.Dispose();
    }
}