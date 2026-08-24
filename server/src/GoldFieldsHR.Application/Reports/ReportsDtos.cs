using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Reports;

public record RoleHeadcountDto(EmployeeRole Role, int Count);

public record ReportsSummaryDto(
    int TotalEmployees,
    int ActiveEmployees,
    IReadOnlyList<RoleHeadcountDto> HeadcountByRole,
    int PendingLeaveRequests,
    int ValidCertificates,
    int DueSoonCertificates,
    int ExpiredCertificates,
    int PendingPpeRequests,
    int PpeAwaitingIssue,
    int PendingLegalAppointments,
    int ActiveLegalAppointments);
