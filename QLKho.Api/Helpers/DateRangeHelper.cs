namespace QLKho.Api.Helpers;

public static class DateRangeHelper
{
    // Múi giờ Việt Nam (UTC+7), cố định để chạy giống nhau trên Windows và Linux/Docker
    private static readonly TimeSpan VnOffset = TimeSpan.FromHours(7);

    /// <summary>
    /// 00:00 thứ Hai của tuần dương lịch hiện tại (giờ Việt Nam), trả về dạng UTC.
    /// </summary>
    public static DateTime StartOfCurrentWeekUtc()
    {
        var vnNow = DateTime.UtcNow + VnOffset;
        var daysSinceMonday = ((int)vnNow.DayOfWeek + 6) % 7;
        var vnMonday = vnNow.Date.AddDays(-daysSinceMonday);
        return DateTime.SpecifyKind(vnMonday - VnOffset, DateTimeKind.Utc);
    }

    /// <summary>
    /// 00:00 ngày 1 của tháng dương lịch hiện tại (giờ Việt Nam), trả về dạng UTC.
    /// </summary>
    public static DateTime StartOfCurrentMonthUtc()
    {
        var vnNow = DateTime.UtcNow + VnOffset;
        var vnFirst = new DateTime(vnNow.Year, vnNow.Month, 1);
        return DateTime.SpecifyKind(vnFirst - VnOffset, DateTimeKind.Utc);
    }
}
