using System;

namespace RabbitMQ.Next.TopologyBuilder;

public static class StreamQueueDeclarationExtensions
{
    public static IStreamQueueDeclaration MaxSize(this IStreamQueueDeclaration declaration, long size)
    {
        if (size <= 0)
        {
            throw new ArgumentException(nameof(size));
        }
        
        declaration.Argument("x-max-length-bytes", size);
        return declaration;
    }

    public static IStreamQueueDeclaration MaxAge(this IStreamQueueDeclaration declaration, int duration, AgeUnit unit)
    {
        if (duration <= 0)
        {
            throw new ArgumentException(nameof(duration));
        }

        var unitSuffix = unit switch
        {
            AgeUnit.Year => "Y",
            AgeUnit.Month => "M",
            AgeUnit.Day => "D",
            AgeUnit.Hour => "h",
            AgeUnit.Minute => "m",
            AgeUnit.Second => "s",
            _ => throw new ArgumentOutOfRangeException(nameof(unit), unit, null),
        };

        declaration.Argument("max-age", string.Concat(duration.ToString(), unitSuffix));
        
        return declaration;
    }

    public static IStreamQueueDeclaration SegmentSize(this IStreamQueueDeclaration declaration, long size)
    {
        if (size <= 0)
        {
            throw new ArgumentException(nameof(size));
        }
        
        declaration.Argument("x-stream-max-segment-size-bytes", size);
        return declaration;
    }
}

public enum AgeUnit
{
    Undefined = 0,
    Year, 
    Month,
    Day,
    Hour,
    Minute,
    Second,
}
