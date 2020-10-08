using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO.Pipelines;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using RabbitMQ.Next.Transport.Channels;
using RabbitMQ.Next.Transport.Methods.Connection;
using RabbitMQ.Next.Transport.Methods.Registry;
using RabbitMQ.Next.Transport.Sockets;
using RabbitMQ.Next.Transport.Methods;

namespace RabbitMQ.Next.Transport
{
    internal class Connection : ISocketWriter
    {
        private readonly SemaphoreSlim writerSemaphore;
        private readonly ChannelPool channelPool;
        private readonly ConnectionString connectionString;
        private ISocket socket;

        private static readonly byte[] FrameEndPayload = { ProtocolConstants.FrameEndByte };

        public Connection(ConnectionString connectionString)
        {
            var registryBuilder = new MethodRegistryBuilder();
            registryBuilder.AddConnectionMethods();
            registryBuilder.AddChannelMethods();
            registryBuilder.AddExchangeMethods();

            var methodRegistry = registryBuilder.Build();

            Func<int, Channel> channelFactory = (channelNumber) =>
            {
                var pipe = new Pipe();
                var methodSender = new MethodSender(this, methodRegistry, (ushort) channelNumber);
                return new Channel(pipe, methodRegistry, methodSender);
            };

            this.channelPool = new ChannelPool(channelFactory);
            this.writerSemaphore = new SemaphoreSlim(1, 1);
            this.connectionString = connectionString;
        }

        public async Task ConnectAsync()
        {
            this.socket = await EndpointResolver.OpenSocketAsync(this.connectionString.EndPoints);
            var connectionChannel = this.channelPool.Next();

            var socketReadThread = new Thread(this.ReceiveLoop);
            socketReadThread.Name = "RabbitMQ socket reader";
            socketReadThread.Start();

            await this.SendProtocolHeaderAsync();

            var startMethod = await connectionChannel.WaitAsync<StartMethod>();

            // TODO: make it dynamic based on assembly version and allow add extra properties
            var clientProperties = new Dictionary<string, object>()
            {
                ["product"] = "RabbitMQ.Next.Transport",
                ["version"] = "0.1.0",
                ["platform"] = Environment.OSVersion.ToString(),
            };

            var tuneMethod = await connectionChannel.SendAsync<StartOkMethod, TuneMethod>(new StartOkMethod("PLAIN", $"\0{this.connectionString.UserName}\0{this.connectionString.Password}", "en-US", clientProperties));

            await connectionChannel.SendAsync(new TuneOkMethod(tuneMethod.ChannelMax, tuneMethod.MaxFrameSize, tuneMethod.HeartbeatInterval));
            await connectionChannel.SendAsync<OpenMethod, OpenOkMethod>(new OpenMethod(this.connectionString.VirtualHost));
        }

        public async Task CloseAsync()
        {
            // todo: validate state here

            await this.channelPool[0].SendAsync<CloseMethod, CloseOkMethod>(new CloseMethod(ReplyCode.Success, "Goodbye", 0));

            this.CleanUpOnSocketClose();
        }


        public async Task SendAsync(ReadOnlyMemory<byte> payload, CancellationToken cancellation)
        {
            await this.writerSemaphore.WaitAsync(cancellation);
            try
            {
                await this.socket.SendAsync(payload);
                await this.socket.SendAsync(FrameEndPayload);
            }
            finally
            {
                this.writerSemaphore.Release();
            }
        }

        private async Task SendProtocolHeaderAsync()
        {
            await this.socket.SendAsync(ProtocolConstants.AmqpHeader);
        }

        private void ReceiveLoop()
        {
            var headerBuffer = new byte[ProtocolConstants.FrameHeaderSize];
            var endFrameBuffer = new byte[1];

            while (true)
            {
                SocketError responseCode;

                // 1. Read frame header
                this.socket.FillBuffer(headerBuffer, out responseCode);
                if (responseCode != SocketError.Success)
                {
                    this.CleanUpOnSocketClose();
                    return;
                }

                var header = ((ReadOnlySpan<byte>)headerBuffer).ReadFrameHeader();

                // 2. Choose appropriate channel to forward the data
                var targetPipe = this.channelPool[header.Channel].Pipe.Writer;
                targetPipe.Write(headerBuffer);

                // 3. Read frame payload into the channel
                this.socket.Receive(targetPipe, header.PayloadSize, out responseCode);
                if (responseCode != SocketError.Success)
                {
                    this.CleanUpOnSocketClose();
                    return;
                }

                // 4. Ensure there is FrameEnd
                this.socket.FillBuffer(endFrameBuffer, out responseCode);
                if (responseCode != SocketError.Success)
                {
                    this.CleanUpOnSocketClose();
                    return;
                }
                if (endFrameBuffer[0] != ProtocolConstants.FrameEndByte)
                {
                    // TODO: throw connection exception here
                    throw new InvalidOperationException();
                }

                // 5. Make data available for consumers
                targetPipe.FlushAsync().GetAwaiter().GetResult();
            }
        }

        private void CleanUpOnSocketClose()
        {
            this.socket.Dispose();
            this.socket = null;
            this.channelPool.ReleaseAll();
        }
    }
}