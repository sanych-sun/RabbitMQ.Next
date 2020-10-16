namespace RabbitMQ.Next.Abstractions
{
    public enum ReplyCode : ushort
    {
        Success = 200,
        ContentTooLarge = 311,
        NoRoute = 312,
        NoConsumers = 313,
        ConnectionForced = 320,
        InvalidPath = 402,
        AccessRefused = 403,
        NotFound = 404,
        ResourceLocked = 405,
        PreconditionFailed = 406,
        FrameError = 501,
        SyntaxError = 502,
        CommandInvalid = 503,
        ChannelError = 504,
        UnexpectedFrame = 505,
        ResourceError = 506,
        NotAllowed = 530,
        NotImplemented = 540,
        InternalError = 541
    }
}