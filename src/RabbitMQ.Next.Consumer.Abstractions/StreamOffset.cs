using System;

namespace RabbitMQ.Next.Consumer;

public readonly struct StreamOffset
{
    public static StreamOffset Next = new (OffsetType.Next, 0);
    public static StreamOffset First = new (OffsetType.First, 0);
    public static StreamOffset Last = new (OffsetType.Last, 0);
    public static StreamOffset FromOffsetValue(long offset) => new (OffsetType.Offset, offset);
    public static StreamOffset FromTimestamp(DateTimeOffset timestamp) => new (OffsetType.Timestamp, timestamp.ToUnixTimeSeconds());
    public static StreamOffset FromYears(int years) => new (OffsetType.Year, years);
    public static StreamOffset FromMonths(int months) => new (OffsetType.Month, months);
    public static StreamOffset FromDays(int days) => new (OffsetType.Day, days);
    public static StreamOffset FromHours(int hours) => new (OffsetType.Hour, hours);
    public static StreamOffset FromMinutes(int minutes) => new (OffsetType.Minute, minutes);
    public static StreamOffset FromSeconds(int seconds) => new (OffsetType.Second, seconds);

    public StreamOffset(OffsetType type, long argument)
    {
        this.Type = type;
        this.Argument = argument;
    }

    public OffsetType Type { get; }
    
    public long Argument { get; }
}

public enum OffsetType
{
    Next = 0, // default one
    First,
    Last,
    
    Offset,
    Timestamp,
    
    Year, 
    Month,
    Day,
    Hour,
    Minute,
    Second
}
