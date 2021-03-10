namespace RabbitMQ.Next.Abstractions.Buffers
{
    public interface IBufferManager
    {
        byte[] Rent(int minSize = 0);

        void Release(byte[] buffer);
    }
}