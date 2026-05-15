using System.ComponentModel;
using System.Text.Json;
using ModelContextProtocol.Server;

[McpServerToolType]
internal class RaspOmgtuTools
{
    private static readonly HttpClient _httpClient = new HttpClient
    {
        BaseAddress = new Uri("http://localhost:8080/api/")
    };

    [McpServerTool]
    [Description("Получить расписание группы на указанный период времени по идентификатору группы")]
    public async Task<JsonElement> GetScheduleByGroupId(
        [Description("идентификатор группы (строка)")] string groupId,
        [Description("начальная граница периода (строка). Формат `YYYY.MM.DD`")] string start,
        [Description("конечная граница периода (строка). Формат `YYYY.MM.DD`")] string finish,
        [Description("язык / локаль (целое число). 1 - Русский")] int lng)
    {
        var url = $"schedule/group/{groupId}?start={start}&finish={finish}&lng={lng}";
        using var response = await _httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return JsonDocument.Parse(content).RootElement.Clone();

        // return $"Status: {(int)response.StatusCode}\nResponse: {content}";
    }
}
