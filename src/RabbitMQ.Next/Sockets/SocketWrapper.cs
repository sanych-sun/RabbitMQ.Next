using System;
using System.IO;
using System.Net.Sockets;
using System.Threading.Tasks;

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
    
    public Task FlushAsync()
        => this.stream.FlushAsync();

    public ValueTask SendAsync(ReadOnlyMemory<byte> payload)
        => this.stream.WriteAsync(payload);
    
    public async ValueTask<int> ReceiveAsync(Memory<byte> buffer, int minBytes)
    {
        var received = 0;
        while (received < minBytes)
        {
            var readBytes = await this.stream.ReadAsync(buffer).ConfigureAwait(false);
            if (readBytes == 0 && this.IsConnectionClosedByServer())
            {
                throw new SocketException();
            }

            buffer = buffer[readBytes..];
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
