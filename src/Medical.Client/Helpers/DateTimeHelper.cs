using System.Globalization;
using Google.Protobuf.WellKnownTypes;

namespace Medical.Client.Helpers;

public static class DateTimeHelper
{
    public static DateTime ToUtcTime(this DateTime localDateTime)
    {
        if (localDateTime.Kind == DateTimeKind.Utc) return localDateTime;
        return TimeZoneInfo.ConvertTimeToUtc(
            DateTime.SpecifyKind(localDateTime, DateTimeKind.Local),
            TimeZoneInfo.Local);
    }
    
    public static DateTime ToLocalDateTime(this Google.Protobuf.WellKnownTypes.Timestamp timestamp)
    {
        return TimeZoneInfo.ConvertTimeFromUtc(
            timestamp.ToDateTime(),
            TimeZoneInfo.Local);
    }

    public static string ToLocalTimeString(this Google.Protobuf.WellKnownTypes.Timestamp timestamp)
    {
        return timestamp.ToLocalDateTime().ToString("g");
    }
    
    public static string ToFormattedDayOfWeek(this DayOfWeek dayOfWeek)
    {
        return CultureInfo.CurrentCulture.DateTimeFormat.GetDayName(dayOfWeek);
    }
}