using GoldFieldsHR.Application.Dashboard;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Dashboard;

public class DashboardService(ApplicationDbContext dbContext) : IDashboardService
{
    public async Task<DashboardSummaryDto> GetSummaryAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var employee = await dbContext.Employees.FindAsync([employeeId], cancellationToken)
            ?? throw new InvalidOperationException("Employee profile not found.");

        var attendance = await GetAttendanceSummaryAsync(employee.SiteId, cancellationToken);
        var pendingLeaveCount = await GetPendingLeaveCountAsync(employee.Id, employee.Role, cancellationToken);
        var incidentsThisMonth = await GetIncidentsThisMonthAsync(cancellationToken);
        var medicalCompliancePercent = await GetMedicalCompliancePercentAsync(cancellationToken);
        var trainingCompliancePercent = await GetTrainingCompliancePercentAsync(cancellationToken);
        var myAveragePerformanceScore = await GetMyAveragePerformanceScoreAsync(employeeId, cancellationToken);

        return new DashboardSummaryDto(
            attendance,
            pendingLeaveCount,
            incidentsThisMonth,
            medicalCompliancePercent,
            trainingCompliancePercent,
            myAveragePerformanceScore);
    }

    private async Task<AttendanceSummaryDto> GetAttendanceSummaryAsync(Guid siteId, CancellationToken cancellationToken)
    {
        var todayUtc = DateTime.UtcNow.Date;
        var tomorrowUtc = todayUtc.AddDays(1);

        var activeEmployeeCount = await dbContext.Employees
            .CountAsync(e => e.IsActive && e.SiteId == siteId, cancellationToken);

        var presentCount = await dbContext.TimesheetEntries
            .Where(t => t.ClockInUtc >= todayUtc && t.ClockInUtc < tomorrowUtc && t.Employee!.SiteId == siteId)
            .Select(t => t.EmployeeId)
            .Distinct()
            .CountAsync(cancellationToken);

        var percentPresent = activeEmployeeCount == 0 ? 0 : Math.Round(presentCount * 100.0 / activeEmployeeCount, 1);

        return new AttendanceSummaryDto(presentCount, activeEmployeeCount, percentPresent);
    }

    private async Task<int> GetPendingLeaveCountAsync(Guid employeeId, EmployeeRole role, CancellationToken cancellationToken)
    {
        return role == EmployeeRole.LineManager
            ? await dbContext.LeaveRequests.CountAsync(r => r.Status == LeaveRequestStatus.Pending, cancellationToken)
            : await dbContext.LeaveRequests.CountAsync(
                r => r.EmployeeId == employeeId && r.Status == LeaveRequestStatus.Pending, cancellationToken);
    }

    private async Task<int> GetIncidentsThisMonthAsync(CancellationToken cancellationToken)
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1, 0, 0, 0, DateTimeKind.Utc);

        return await dbContext.IncidentReports
            .CountAsync(i => i.CreatedAtUtc >= startOfMonth, cancellationToken);
    }

    private async Task<double?> GetMedicalCompliancePercentAsync(CancellationToken cancellationToken)
    {
        var examinations = await dbContext.MedicalExaminations
            .Select(m => new { m.EmployeeId, m.ExamDate, m.Status })
            .ToListAsync(cancellationToken);

        if (examinations.Count == 0)
        {
            return null;
        }

        var latestByEmployee = examinations
            .GroupBy(m => m.EmployeeId)
            .Select(g => g.OrderByDescending(m => m.ExamDate).First().Status)
            .ToList();

        var compliantCount = latestByEmployee.Count(status => status is FitnessStatus.Fit or FitnessStatus.FitWithRestrictions);
        return Math.Round(compliantCount * 100.0 / latestByEmployee.Count, 1);
    }

    private async Task<double?> GetTrainingCompliancePercentAsync(CancellationToken cancellationToken)
    {
        var expiryDates = await dbContext.Certificates.Select(c => c.ExpiryDate).ToListAsync(cancellationToken);
        if (expiryDates.Count == 0)
        {
            return null;
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var compliantCount = expiryDates.Count(expiry => expiry >= today);
        return Math.Round(compliantCount * 100.0 / expiryDates.Count, 1);
    }

    private async Task<double?> GetMyAveragePerformanceScoreAsync(Guid employeeId, CancellationToken cancellationToken)
    {
        var scores = await dbContext.PerformanceReviews
            .Where(r => r.EmployeeId == employeeId)
            .Select(r => r.Score)
            .ToListAsync(cancellationToken);

        return scores.Count == 0 ? null : Math.Round(scores.Average(), 1);
    }
}
