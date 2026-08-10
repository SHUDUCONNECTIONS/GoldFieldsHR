using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Reports;

public record RoleHeadcountDto(EmployeeRole Role, int Count);

public record IncidentSeverityCountDto(IncidentSeverity Severity, int Count);

public record ReportsAttendanceDto(int PresentCount, int ActiveEmployeeCount, double PercentPresent);

public record ReportsSummaryDto(
    int TotalEmployees,
    int ActiveEmployees,
    IReadOnlyList<RoleHeadcountDto> HeadcountByRole,
    ReportsAttendanceDto AttendanceToday,
    int OpenIncidents,
    int ClosedIncidents,
    IReadOnlyList<IncidentSeverityCountDto> OpenIncidentsBySeverity,
    int PendingLeaveRequests,
    int ValidCertificates,
    int DueSoonCertificates,
    int ExpiredCertificates,
    int PendingPpeRequests,
    int PpeAwaitingIssue,
    int PendingPermits,
    int OpenPermits);
