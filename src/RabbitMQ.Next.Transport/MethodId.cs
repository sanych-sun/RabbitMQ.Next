namespace RabbitMQ.Next.Transport
{
    internal enum MethodId : uint
    {
        // @formatter:off
        // Connection class methods:
        ConnectionStart     = (ClassId.Connection << 16) | 10,
        ConnectionStartOk   = (ClassId.Connection << 16) | 11,
        ConnectionSecure    = (ClassId.Connection << 16) | 20,
        ConnectionSecureOk  = (ClassId.Connection << 16) | 21,
        ConnectionTune      = (ClassId.Connection << 16) | 30,
        ConnectionTuneOk    = (ClassId.Connection << 16) | 31,
        ConnectionOpen      = (ClassId.Connection << 16) | 40,
        ConnectionOpenOk    = (ClassId.Connection << 16) | 41,
        ConnectionClose     = (ClassId.Connection << 16) | 50,
        ConnectionCloseOk   = (ClassId.Connection << 16) | 51,
        Blocked             = (ClassId.Connection << 16) | 60,
        Unblocked           = (ClassId.Connection << 16) | 61,
        UpdateSecret        = (ClassId.Connection << 16) | 70,
        UpdateSecretOk      = (ClassId.Connection << 16) | 71,

        // Channel class methods:


        // @formatter:on
    }
}