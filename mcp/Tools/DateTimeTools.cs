using System.ComponentModel;
using System.Globalization;
using ModelContextProtocol.Server;

[McpServerToolType]
internal class DateTimeTools
{
    [McpServerTool]
    [Description("Получить текущую дату и время сервера в ISO 8601")]
    public string GetNowIso()
    {
        return DateTimeOffset.Now.ToString("O");
    }

    [McpServerTool]
    [Description("Получить текущую дату")]
    public string GetCurrentDate()
    {
        return DateOnly.FromDateTime(DateTime.Now)
            .ToString("yyyy.MM.dd");
    }

    [McpServerTool]
    [Description("Получить текущее время")]
    public string GetCurrentTime()
    {
        return TimeOnly.FromDateTime(DateTime.Now)
            .ToString("HH:mm:ss");
    }

    [McpServerTool]
    [Description("Получить текущий timestamp Unix")]
    public long GetUnixTimestamp()
    {
        return DateTimeOffset.UtcNow.ToUnixTimeSeconds();
    }

    [McpServerTool]
    [Description("Получить текущую дату и время UTC")]
    public string GetUtcNow()
    {
        return DateTimeOffset.UtcNow.ToString("O");
    }

    [McpServerTool]
    [Description("Получить дату через указанное количество дней от текущей")]
    public string GetDateAfterDays(
        [Description("количество дней")] int days)
    {
        return DateTime.Now
            .AddDays(days)
            .ToString("yyyy.MM.dd");
    }

    [McpServerTool]
    [Description("Получить дату до указанного количества дней от текущей")]
    public string GetDateBeforeDays(
        [Description("количество дней")] int days)
    {
        return DateTime.Now
            .AddDays(-days)
            .ToString("yyyy.MM.dd");
    }

    [McpServerTool]
    [Description("Преобразовать дату в формат API ОмГТУ YYYY.MM.DD")]
    public string FormatDateForOmgtuApi(
        [Description("дата в любом поддерживаемом формате")] string input)
    {
        var date = DateTime.Parse(
            input,
            CultureInfo.InvariantCulture,
            DateTimeStyles.AssumeLocal);

        return date.ToString("yyyy.MM.dd");
    }

    [McpServerTool]
    [Description("Получить диапазон дат от сегодня + N дней")]
    public object GetDateRange(
        [Description("смещение начала диапазона в днях")] int startOffsetDays,
        [Description("смещение конца диапазона в днях")] int finishOffsetDays)
    {
        var now = DateTime.Now;

        return new
        {
            start = now
                .AddDays(startOffsetDays)
                .ToString("yyyy.MM.dd"),

            finish = now
                .AddDays(finishOffsetDays)
                .ToString("yyyy.MM.dd")
        };
    }

    [McpServerTool]
    [Description("Получить текущий день недели")]
    public string GetDayOfWeek()
    {
        return DateTime.Now.DayOfWeek.ToString();
    }

    [McpServerTool]
    [Description("Проверить високосный ли год")]
    public bool IsLeapYear(
        [Description("год")] int year)
    {
        return DateTime.IsLeapYear(year);
    }
}
