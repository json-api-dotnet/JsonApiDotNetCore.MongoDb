namespace TestBuildingBlocks;

public static class TimeExtensions
{
    // The milliseconds precision in DateTime/DateTimeOffset/TimeSpan/TimeOnly values that fakers produce
    // is higher than what MongoDB can store. This results in our resource change tracker to detect
    // that the time stored in the database differs from the time in the request body. While that's
    // technically correct, we don't want such side effects influencing our tests everywhere.

    public static DateTimeOffset TruncateToWholeMilliseconds(this DateTimeOffset value)
    {
        // Because MongoDB does not store the UTC offset in the database, it cannot round-trip
        // values with a non-zero UTC offset.

        DateTime dateTime = value.DateTime.TruncateToWholeMilliseconds();
        return new DateTimeOffset(dateTime, TimeSpan.Zero);
    }

    public static DateTime TruncateToWholeMilliseconds(this DateTime value)
    {
        long ticksInWholeMilliseconds = TruncateTicksInWholeMilliseconds(value.Ticks);
        return new DateTime(ticksInWholeMilliseconds, value.Kind);
    }

    private static long TruncateTicksInWholeMilliseconds(long ticks)
    {
        long ticksToSubtract = ticks % TimeSpan.TicksPerMillisecond;
        return ticks - ticksToSubtract;
    }
}
