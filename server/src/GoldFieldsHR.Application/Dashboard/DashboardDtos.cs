using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Application.Dashboard;

public record AttendanceSummaryDto(int PresentCount, int ActiveEmployeeCount, double PercentPresent);

public record RecentShiftRequestDto(
    Guid Id,
    string EmployeeName,
    DateOnly RequestedShiftDate,
    ShiftType RequestedShiftType,
    ShiftRequestStatus Status,
    DateTime CreatedAtUtc);

public record DashboardSummaryDto(
    AttendanceSummaryDto Attendance,
    int PendingLeaveCount,
    int IncidentsThisMonth,
    double? MedicalCompliancePercent,
    double? TrainingCompliancePercent,
    double? MyAveragePerformanceScore,
    IReadOnlyList<RecentShiftRequestDto> RecentShiftRequests);
