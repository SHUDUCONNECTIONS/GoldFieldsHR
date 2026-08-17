using System.Globalization;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Timesheet;

namespace GoldFieldsHR.Infrastructure.Timesheet;

public class ClockingReportParserService(HttpClient httpClient) : IClockingReportParserService
{
    public async Task<Result<ClockingReportParseResultDto>> ParseAsync(
        Stream fileStream,
        string fileName,
        string workDays,
        double hoursPerDay,
        bool rotating,
        CancellationToken cancellationToken = default)
    {
        using var content = new MultipartFormDataContent();
        using var fileContent = new StreamContent(fileStream);
        fileContent.Headers.ContentType = new MediaTypeHeaderValue("application/pdf");
        content.Add(fileContent, "file", fileName);
        content.Add(new StringContent(workDays), "work_days");
        content.Add(new StringContent(hoursPerDay.ToString(CultureInfo.InvariantCulture)), "hours_per_day");
        content.Add(new StringContent(rotating ? "true" : "false"), "rotating");

        HttpResponseMessage response;
        try
        {
            response = await httpClient.PostAsync("api/parse", content, cancellationToken);
        }
        catch (HttpRequestException)
        {
            return Result<ClockingReportParseResultDto>.Failure(
                "Could not reach the clocking report parser service. Is it running?");
        }

        using (response)
        {
            if (!response.IsSuccessStatusCode)
            {
                return Result<ClockingReportParseResultDto>.Failure(
                    $"The clocking report parser rejected the request ({(int)response.StatusCode}).");
            }

            var parsed = await response.Content.ReadFromJsonAsync<ParserApiResponse>(cancellationToken: cancellationToken);
            if (parsed is null)
            {
                return Result<ClockingReportParseResultDto>.Failure("The parser returned an unreadable response.");
            }

            return Result<ClockingReportParseResultDto>.Success(new ClockingReportParseResultDto(
                parsed.Filename,
                parsed.Status,
                parsed.Message,
                parsed.Events,
                parsed.Days,
                parsed.Shifts,
                parsed.TotalHours,
                parsed.XlsxBase64,
                parsed.DownloadName));
        }
    }

    // Mirrors the Python service's snake_case JSON response shape (see
    // ClockingReportParser/main.py's ParseResult) - kept internal so the
    // public ClockingReportParseResultDto can use this app's normal
    // camelCase API convention instead.
    private record ParserApiResponse(
        [property: JsonPropertyName("filename")] string Filename,
        [property: JsonPropertyName("status")] string Status,
        [property: JsonPropertyName("message")] string Message,
        [property: JsonPropertyName("events")] int? Events,
        [property: JsonPropertyName("days")] int? Days,
        [property: JsonPropertyName("shifts")] int? Shifts,
        [property: JsonPropertyName("total_hours")] double? TotalHours,
        [property: JsonPropertyName("xlsx_base64")] string? XlsxBase64,
        [property: JsonPropertyName("download_name")] string? DownloadName);
}
