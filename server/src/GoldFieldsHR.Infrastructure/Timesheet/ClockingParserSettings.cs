namespace GoldFieldsHR.Infrastructure.Timesheet;

public class ClockingParserSettings
{
    public const string SectionName = "ClockingParser";

    /// <summary>
    /// Base URL of the internal Python parser service (server/ClockingReportParser).
    /// It has no auth of its own, so this must only ever point somewhere not
    /// reachable from outside the deployment (localhost in dev, the docker-compose/
    /// Render internal network in production).
    /// </summary>
    public string BaseUrl { get; set; } = "http://localhost:8010";
}
