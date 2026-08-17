using GoldFieldsHR.Application.Reports;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Reports;

public class ReportsService(ApplicationDbContext dbContext) : IReportsService
{
    private const int CertificateDueSoonThresholdDays = 30;

    public async Task<ReportsSummaryDto> GetSummaryAsync(CancellationToken cancellationToken = default)
    {
        var totalEmployees = await dbContext.Employees.CountAsync(cancellationToken);
        var activeEmployees = await dbContext.Employees.CountAsync(e => e.IsActive, cancellationToken);

        var headcountByRole = (await dbContext.Employees
            .GroupBy(e => e.Role)
            .Select(g => new { Role = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken))
            .Select(r => new RoleHeadcountDto(r.Role, r.Count))
            .OrderBy(r => r.Role)
            .ToList();

        var openIncidents = await dbContext.IncidentReports
            .CountAsync(i => i.Status != IncidentStatus.Closed, cancellationToken);
        var closedIncidents = await dbContext.IncidentReports
            .CountAsync(i => i.Status == IncidentStatus.Closed, cancellationToken);

        var openIncidentsBySeverity = (await dbContext.IncidentReports
            .Where(i => i.Status != IncidentStatus.Closed)
            .GroupBy(i => i.Severity)
            .Select(g => new { Severity = g.Key, Count = g.Count() })
            .ToListAsync(cancellationToken))
            .Select(s => new IncidentSeverityCountDto(s.Severity, s.Count))
            .OrderBy(s => s.Severity)
            .ToList();

        var pendingLeaveRequests = await dbContext.LeaveRequests
            .CountAsync(
                r => r.Status == LeaveRequestStatus.PendingLineManagerApproval || r.Status == LeaveRequestStatus.PendingHRApproval,
                cancellationToken);

        var (validCertificates, dueSoonCertificates, expiredCertificates) = await GetCertificateCountsAsync(cancellationToken);

        var pendingPpeRequests = await dbContext.PpeRequests
            .CountAsync(r => r.Status == PpeRequestStatus.Pending, cancellationToken);
        var ppeAwaitingIssue = await dbContext.PpeRequests
            .CountAsync(r => r.Status == PpeRequestStatus.Approved, cancellationToken);

        var pendingLegalAppointments = await dbContext.LegalAppointments
            .CountAsync(p => p.Status == LegalAppointmentStatus.Pending, cancellationToken);
        var activeLegalAppointments = await dbContext.LegalAppointments
            .CountAsync(p => p.Status == LegalAppointmentStatus.Active, cancellationToken);

        return new ReportsSummaryDto(
            totalEmployees,
            activeEmployees,
            headcountByRole,
            openIncidents,
            closedIncidents,
            openIncidentsBySeverity,
            pendingLeaveRequests,
            validCertificates,
            dueSoonCertificates,
            expiredCertificates,
            pendingPpeRequests,
            ppeAwaitingIssue,
            pendingLegalAppointments,
            activeLegalAppointments);
    }

    private async Task<(int Valid, int DueSoon, int Expired)> GetCertificateCountsAsync(CancellationToken cancellationToken)
    {
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var dueSoonCutoff = today.AddDays(CertificateDueSoonThresholdDays);

        var expired = await dbContext.Certificates.CountAsync(c => c.ExpiryDate < today, cancellationToken);
        var dueSoon = await dbContext.Certificates
            .CountAsync(c => c.ExpiryDate >= today && c.ExpiryDate <= dueSoonCutoff, cancellationToken);
        var valid = await dbContext.Certificates.CountAsync(c => c.ExpiryDate > dueSoonCutoff, cancellationToken);

        return (valid, dueSoon, expired);
    }
}
