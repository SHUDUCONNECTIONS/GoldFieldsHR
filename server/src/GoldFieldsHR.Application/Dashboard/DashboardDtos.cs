using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Dashboard;

public record RecentShiftRequestDto(
    Guid Id,
    string EmployeeName,
    DateOnly RequestedShiftDate,
    ShiftType RequestedShiftType,
    ShiftRequestStatus Status,
    DateTime CreatedAtUtc);

public record DashboardSummaryDto(
    int PendingLeaveCount,
    int IncidentsThisMonth,
    double? MedicalCompliancePercent,
    double? TrainingCompliancePercent,
    double? MyAveragePerformanceScore,
    IReadOnlyList<RecentShiftRequestDto> RecentShiftRequests);
