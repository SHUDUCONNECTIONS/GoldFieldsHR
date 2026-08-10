namespace GoldFieldsHR.Application.Dashboard;

public record AttendanceSummaryDto(int PresentCount, int ActiveEmployeeCount, double PercentPresent);

public record DashboardSummaryDto(
    AttendanceSummaryDto Attendance,
    int PendingLeaveCount,
    int IncidentsThisMonth,
    double? MedicalCompliancePercent,
    double? TrainingCompliancePercent,
    double? MyAveragePerformanceScore);
