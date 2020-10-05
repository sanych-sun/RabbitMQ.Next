namespace RabbitMQ.Next.Transport
{
    internal enum ClassId : ushort
    {
        Connection = 10,
        Channel = 20,
        Exchange = 40,
        Queue = 50,
        Basic = 60,
        Confirm = 85,
        Tx = 90,
    }
}