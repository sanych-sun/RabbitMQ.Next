namespace RabbitMQ.Next.Transport.Buffers
{
    internal interface IBufferManager
    {
        byte[] Rent();

        void Release(byte[] buffer);

        int BufferSize { get; }
    }
}