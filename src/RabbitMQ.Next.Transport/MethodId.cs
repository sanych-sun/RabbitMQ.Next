namespace RabbitMQ.Next.Transport
{
    internal enum MethodId : uint
    {
        // @formatter:off
        // Connection class methods:
        ConnectionStart          = (ClassId.Connection << 16) | 10,
        ConnectionStartOk        = (ClassId.Connection << 16) | 11,
        ConnectionSecure         = (ClassId.Connection << 16) | 20,
        ConnectionSecureOk       = (ClassId.Connection << 16) | 21,
        ConnectionTune           = (ClassId.Connection << 16) | 30,
        ConnectionTuneOk         = (ClassId.Connection << 16) | 31,
        ConnectionOpen           = (ClassId.Connection << 16) | 40,
        ConnectionOpenOk         = (ClassId.Connection << 16) | 41,
        ConnectionClose          = (ClassId.Connection << 16) | 50,
        ConnectionCloseOk        = (ClassId.Connection << 16) | 51,
        ConnectionBlocked        = (ClassId.Connection << 16) | 60,
        ConnectionUnblocked      = (ClassId.Connection << 16) | 61,
        ConnectionUpdateSecret   = (ClassId.Connection << 16) | 70,
        ConnectionUpdateSecretOk = (ClassId.Connection << 16) | 71,

        // Channel class methods:
        ChannelOpen              = (ClassId.Channel << 16) | 10,
        ChannelOpenOk            = (ClassId.Channel << 16) | 11,
        ChannelFlow              = (ClassId.Channel << 16) | 20,
        ChannelFlowOk            = (ClassId.Channel << 16) | 21,
        ChannelClose             = (ClassId.Channel << 16) | 40,
        ChannelCloseOk           = (ClassId.Channel << 16) | 41,

        // Exchange class methods:
        ExchangeDeclare          = (ClassId.Exchange << 16) | 10,
        ExchangeDeclareOk        = (ClassId.Exchange << 16) | 11,
        ExchangeDelete           = (ClassId.Exchange << 16) | 20,
        ExchangeDeleteOk         = (ClassId.Exchange << 16) | 21,
        ExchangeBind             = (ClassId.Exchange << 16) | 30,
        ExchangeBindOk           = (ClassId.Exchange << 16) | 31,
        ExchangeUnbind           = (ClassId.Exchange << 16) | 40,
        ExchangeUnbindOk         = (ClassId.Exchange << 16) | 51,

        // Queue class methods:
        QueueDeclare             = (ClassId.Queue << 16) | 10,
        QueueDeclareOk           = (ClassId.Queue << 16) | 11,
        QueueBind                = (ClassId.Queue << 16) | 20,
        QueueBindOk              = (ClassId.Queue << 16) | 21,
        QueuePurge               = (ClassId.Queue << 16) | 30,
        QueuePurgeOk             = (ClassId.Queue << 16) | 31,
        QueueDelete              = (ClassId.Queue << 16) | 40,
        QueueDeleteOk            = (ClassId.Queue << 16) | 41,
        QueueUnbind              = (ClassId.Queue << 16) | 50,
        QueueUnbindOk            = (ClassId.Queue << 16) | 51,
            
        // Basic class methods:
        BasicQos                 = (ClassId.Basic<< 16) | 10,
        BasicQosOk               = (ClassId.Basic<< 16) | 11,
        BasicConsume             = (ClassId.Basic<< 16) | 20,
        BasicConsumeOk           = (ClassId.Basic<< 16) | 21,
        BasicCancel              = (ClassId.Basic<< 16) | 30,
        BasicCancelOk            = (ClassId.Basic<< 16) | 31,
        BasicPublish             = (ClassId.Basic<< 16) | 40,
        BasicReturn              = (ClassId.Basic<< 16) | 50,
        BasicDeliver             = (ClassId.Basic<< 16) | 60,
        BasicGet                 = (ClassId.Basic<< 16) | 70,
        BasicGetOk               = (ClassId.Basic<< 16) | 71,
        BasicGetEmpty            = (ClassId.Basic<< 16) | 72,
        BasicAck                 = (ClassId.Basic<< 16) | 80,
        BasicRecover             = (ClassId.Basic<< 16) | 110,
        BasicRecoverOk           = (ClassId.Basic<< 16) | 111,
        BasicNack                = (ClassId.Basic<< 16) | 120,

        // @formatter:on
    }
}