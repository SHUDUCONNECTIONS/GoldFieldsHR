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
    double? MedicalCompliancePercent,
    double? TrainingCompliancePercent,
    double? MyKpiOverallScorePercent,
    IReadOnlyList<RecentShiftRequestDto> RecentShiftRequests);
