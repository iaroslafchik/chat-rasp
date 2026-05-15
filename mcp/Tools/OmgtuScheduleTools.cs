using System.ComponentModel;
using System.Text;
using System.Text.Json;
using ModelContextProtocol.Server;

[McpServerToolType]
internal class OmgtuScheduleTools
{
    private static readonly HttpClient _httpClient = new()
    {
        BaseAddress = new Uri("http://localhost:8080/api/")
    };

    private static async Task<JsonElement> GetJsonAsync(string url)
    {
        using var response = await _httpClient.GetAsync(url);

        // API может возвращать ошибки внутри JSON даже при HTTP 200
        response.EnsureSuccessStatusCode();

        var content = await response.Content.ReadAsStringAsync();

        return JsonDocument.Parse(content).RootElement.Clone();
    }

    /// <summary>
    /// Преобразует JSON-массив в CSV-строку с заголовками.
    /// </summary>
    private static string ConvertJsonArrayToCsv(JsonElement jsonElement)
    {
        // Если это не массив — возможно, ошибка API (объект с полем "error")
        if (jsonElement.ValueKind != JsonValueKind.Array)
        {
            // Попытаемся извлечь сообщение об ошибке, если оно есть
            if (jsonElement.ValueKind == JsonValueKind.Object &&
                jsonElement.TryGetProperty("error", out var errorProp))
            {
                return $"Ошибка API: {errorProp.GetString()}";
            }
            // Иначе возвращаем "сырой" JSON как есть
            return jsonElement.GetRawText();
        }

        var array = jsonElement.EnumerateArray().ToList();
        if (array.Count == 0)
            return string.Empty; // пустой массив -> пустой CSV

        // Определяем заголовки из первого объекта
        var firstObj = array[0];
        if (firstObj.ValueKind != JsonValueKind.Object)
            return firstObj.GetRawText(); // не объект — возвращаем как есть

        var headers = firstObj.EnumerateObject().Select(p => p.Name).ToList();
        var csvBuilder = new StringBuilder();

        // Записываем заголовки
        csvBuilder.AppendLine(string.Join(",", headers.Select(EscapeCsvField)));

        // Записываем строки данных
        foreach (var item in array)
        {
            if (item.ValueKind != JsonValueKind.Object)
                continue;

            var row = new List<string>();
            foreach (var header in headers)
            {
                if (item.TryGetProperty(header, out var value))
                {
                    string rawValue = value.ValueKind switch
                    {
                        JsonValueKind.String => value.GetString() ?? string.Empty,
                        JsonValueKind.Number => value.GetRawText(),
                        JsonValueKind.True => "true",
                        JsonValueKind.False => "false",
                        JsonValueKind.Null => string.Empty,
                        _ => value.GetRawText()
                    };
                    row.Add(EscapeCsvField(rawValue));
                }
                else
                {
                    row.Add(string.Empty);
                }
            }
            csvBuilder.AppendLine(string.Join(",", row));
        }

        return csvBuilder.ToString();
    }

    private static string EscapeCsvField(string field)
    {
        if (string.IsNullOrEmpty(field))
            return string.Empty;

        bool needQuotes = field.Contains(',') || field.Contains('"') || field.Contains('\n') || field.Contains('\r');
        if (needQuotes)
        {
            return "\"" + field.Replace("\"", "\"\"") + "\"";
        }
        return field;
    }

    [McpServerTool]
    [Description("""
        Поиск объектов расписания ОмГТУ.

        Используется перед получением расписания для определения корректного идентификатора сущности.

        Поддерживаемые типы:
        - group — учебная группа
        - person — преподаватель
        - auditorium — аудитория
        - student — студент

        Возвращает CSV со столбцами:
        - id — внутренний идентификатор
        - label — отображаемое название
        - description — дополнительное описание
        - type — тип сущности

        Инструмент подходит для:
        - проверки существования группы
        - поиска преподавателя по фамилии
        - поиска аудитории
        - определения id перед вызовом расписания
        """)]
    public async Task<string> Search(
            [Description("поисковая строка")] string term,
            [Description("тип объекта: person, auditorium, student, group")] string type)
    {
        var url =
            $"search?term={Uri.EscapeDataString(term)}&type={Uri.EscapeDataString(type)}";

        var json = await GetJsonAsync(url);
        return ConvertJsonArrayToCsv(json);
    }

    [McpServerTool]
    [Description("""
        Получить расписание учебной группы ОмГТУ за указанный период.

        Возвращает CSV-таблицу занятий.

        Каждая строка содержит:
        - auditorium — аудитория
        - beginLesson — время начала пары
        - building — корпус
        - createddate — дата публикации расписания
        - date — дата занятия
        - dayOfWeek — номер дня недели
        - dayOfWeekString — сокращённое название дня недели
        - discipline — дисциплина
        - endLesson — время окончания пары
        - kindOfWork — тип занятия (лекция, лабораторная, практика и т.д.)
        - lecturer — преподаватель
        - modifieddate — дата последнего изменения записи
        - stream — поток
        - subGroup — подгруппа
        - listGroups — список групп

        Особенности:
        - пустой CSV означает отсутствие занятий
        - ошибки API возвращаются текстом
        - максимальный период запроса: 180 дней
        - формат даты: YYYY.MM.DD

        Инструмент предназначен для:
        - просмотра расписания группы
        - поиска занятий по дате
        - определения времени и аудитории занятий
        - анализа изменений расписания
        """)]
    public async Task<string> GetGroupSchedule(
        [Description("идентификатор группы")] int id,
        [Description("начальная дата. Формат YYYY.MM.DD")] string start,
        [Description("конечная дата. Формат YYYY.MM.DD")] string finish,
        [Description("язык интерфейса. 1 - русский")] int lng = 1)
    {
        var url =
            $"schedule/group/{id}?start={start}&finish={finish}&lng={lng}";

        var json = await GetJsonAsync(url);
        return ConvertJsonArrayToCsv(json);
    }

    [McpServerTool]
    [Description("""
        Получить расписание преподавателя ОмГТУ за указанный период.

        Возвращает CSV-таблицу занятий преподавателя.

        Каждая строка содержит:
        - auditorium — аудитория
        - beginLesson — время начала пары
        - building — корпус
        - createddate — дата публикации расписания
        - date — дата занятия
        - dayOfWeek — номер дня недели
        - dayOfWeekString — день недели
        - discipline — дисциплина
        - endLesson — время окончания пары
        - kindOfWork — тип занятия
        - lecturer — имя преподавателя
        - modifieddate — дата изменения расписания
        - stream — поток
        - subGroup — подгруппа
        - listGroups — список групп

        Особенности:
        - можно использовать для проверки занятости преподавателя
        - пустой CSV означает отсутствие занятий
        - максимальный период запроса: 180 дней
        - формат даты: YYYY.MM.DD
        """)]
    public async Task<string> GetPersonSchedule(
        [Description("идентификатор преподавателя")] int id,
        [Description("начальная дата. Формат YYYY.MM.DD")] string start,
        [Description("конечная дата. Формат YYYY.MM.DD")] string finish,
        [Description("язык интерфейса. 1 - русский")] int lng = 1)
    {
        var url =
            $"schedule/person/{id}?start={start}&finish={finish}&lng={lng}";

        var json = await GetJsonAsync(url);
        return ConvertJsonArrayToCsv(json);
    }

    [McpServerTool]
    [Description("""
        Получить расписание аудитории ОмГТУ за указанный период.

        Возвращает CSV-таблицу всех занятий, проходящих в указанной аудитории.

        Каждая строка содержит:
        - auditorium — номер аудитории
        - beginLesson — время начала занятия
        - building — корпус
        - createddate — дата создания расписания
        - date — дата занятия
        - dayOfWeek — номер дня недели
        - dayOfWeekString — день недели
        - discipline — дисциплина
        - endLesson — время окончания занятия
        - kindOfWork — тип занятия
        - lecturer — преподаватель
        - modifieddate — дата последнего изменения
        - stream — поток
        - subGroup — подгруппа
        - listGroups — список групп

        Инструмент подходит для:
        - проверки свободных аудиторий
        - поиска занятости аудитории
        - анализа загрузки кабинетов

        Особенности:
        - пустой CSV означает отсутствие занятий
        - максимальный период запроса: 180 дней
        - формат даты: YYYY.MM.DD
        """)]
    public async Task<string> GetAuditoriumSchedule(
        [Description("идентификатор аудитории")] int id,
        [Description("начальная дата. Формат YYYY.MM.DD")] string start,
        [Description("конечная дата. Формат YYYY.MM.DD")] string finish,
        [Description("язык интерфейса. 1 - русский")] int lng = 1)
    {
        var url =
            $"schedule/auditorium/{id}?start={start}&finish={finish}&lng={lng}";

        var json = await GetJsonAsync(url);
        return ConvertJsonArrayToCsv(json);
    }

    [McpServerTool]
    [Description("""
        Получить ICS-календарь расписания учебной группы.

        Возвращает содержимое iCalendar (.ics), совместимое с:
        - Google Calendar
        - Apple Calendar
        - Outlook
        - Thunderbird
        - мобильными календарями

        Используется для:
        - импорта расписания в календарь
        - синхронизации занятий
        - создания подписки на расписание

        Особенности:
        - формат даты: YYYY.MM.DD
        - максимальный период: 180 дней
        - язык: lng=1 для русского
        """)]
    public async Task<string> GetGroupScheduleIcs(
        [Description("идентификатор группы")] int id,
        [Description("начальная дата. Формат YYYY.MM.DD")] string start,
        [Description("конечная дата. Формат YYYY.MM.DD")] string finish,
        [Description("язык интерфейса. 1 - русский")] int lng = 1)
    {
        var url =
            $"schedule/group/{id}.ics?start={start}&finish={finish}&lng={lng}";

        using var response = await _httpClient.GetAsync(url);

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync();
    }
}
