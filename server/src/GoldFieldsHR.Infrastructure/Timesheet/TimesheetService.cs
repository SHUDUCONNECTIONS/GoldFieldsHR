using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Notifications;
using GoldFieldsHR.Application.Timesheet;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Timesheet;

public class TimesheetService(ApplicationDbContext dbContext, INotificationService notificationService) : ITimesheetService
{
    public async Task<Result<TimesheetEntryDto>> ClockInAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var hasOpenEntry = await dbContext.TimesheetEntries
            .AnyAsync(t => t.EmployeeId == employeeId && t.ClockOutUtc == null, cancellationToken);

        if (hasOpenEntry)
        {
            return Result<TimesheetEntryDto>.Failure("You are already clocked in.");
        }

        var entry = new TimesheetEntry
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            ClockInUtc = DateTime.UtcNow
        };

        dbContext.TimesheetEntries.Add(entry);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<TimesheetEntryDto>.Success(ToDto(entry));
    }

    public async Task<Result<TimesheetEntryDto>> ClockOutAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var entry = await dbContext.TimesheetEntries
            .Where(t => t.EmployeeId == employeeId && t.ClockOutUtc == null)
            .OrderByDescending(t => t.ClockInUtc)
            .FirstOrDefaultAsync(cancellationToken);

        if (entry is null)
        {
            return Result<TimesheetEntryDto>.Failure("You are not currently clocked in.");
        }

        entry.ClockOutUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<TimesheetEntryDto>.Success(ToDto(entry));
    }

    public async Task<TimesheetEntryDto?> GetOpenEntryAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var entry = await dbContext.TimesheetEntries
            .Where(t => t.EmployeeId == employeeId && t.ClockOutUtc == null)
            .OrderByDescending(t => t.ClockInUtc)
            .FirstOrDefaultAsync(cancellationToken);

        return entry is null ? null : ToDto(entry);
    }

    public async Task<IReadOnlyList<TimesheetEntryDto>> GetHistoryAsync(Guid employeeId, CancellationToken cancellationToken = default)
    {
        var entries = await dbContext.TimesheetEntries
            .Where(t => t.EmployeeId == employeeId)
            .OrderByDescending(t => t.ClockInUtc)
            .Take(50)
            .ToListAsync(cancellationToken);

        return entries.Select(ToDto).ToList();
    }

    public async Task<Result<TimesheetCorrectionDto>> SubmitCorrectionAsync(
        Guid employeeId, SubmitTimesheetCorrectionRequest request, CancellationToken cancellationToken = default)
    {
        if (request.RequestedClockInUtc is null && request.RequestedClockOutUtc is null)
        {
            return Result<TimesheetCorrectionDto>.Failure("Provide at least one corrected time.");
        }

        var entry = await dbContext.TimesheetEntries
            .FirstOrDefaultAsync(t => t.Id == request.TimesheetEntryId, cancellationToken);
        if (entry is null)
        {
            return Result<TimesheetCorrectionDto>.Failure("Timesheet entry not found.");
        }

        if (entry.EmployeeId != employeeId)
        {
            return Result<TimesheetCorrectionDto>.Failure("You can only request corrections for your own timesheet entries.");
        }

        var effectiveClockOut = request.RequestedClockOutUtc ?? entry.ClockOutUtc;
        var effectiveClockIn = request.RequestedClockInUtc ?? entry.ClockInUtc;
        if (effectiveClockOut is not null && effectiveClockOut <= effectiveClockIn)
        {
            return Result<TimesheetCorrectionDto>.Failure("Clock-out time must be after clock-in time.");
        }

        var hasPending = await dbContext.TimesheetCorrectionRequests
            .AnyAsync(r => r.TimesheetEntryId == request.TimesheetEntryId && r.Status == TimesheetCorrectionStatus.Pending, cancellationToken);
        if (hasPending)
        {
            return Result<TimesheetCorrectionDto>.Failure("This entry already has a pending correction request.");
        }

        var employee = await dbContext.Employees.FindAsync([employeeId], cancellationToken);
        if (employee is null)
        {
            return Result<TimesheetCorrectionDto>.Failure("Employee profile not found.");
        }

        var correction = new TimesheetCorrectionRequest
        {
            Id = Guid.NewGuid(),
            TimesheetEntryId = entry.Id,
            EmployeeId = employeeId,
            RequestedClockInUtc = request.RequestedClockInUtc,
            RequestedClockOutUtc = request.RequestedClockOutUtc,
            Reason = request.Reason,
        };

        dbContext.TimesheetCorrectionRequests.Add(correction);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<TimesheetCorrectionDto>.Success(ToCorrectionDto(correction, entry, employee.FullName));
    }

    public async Task<IReadOnlyList<TimesheetCorrectionDto>> GetMyCorrectionsAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        var corrections = await dbContext.TimesheetCorrectionRequests
            .Include(r => r.TimesheetEntry)
            .Include(r => r.Employee)
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return corrections.Select(r => ToCorrectionDto(r, r.TimesheetEntry!, r.Employee!.FullName)).ToList();
    }

    public async Task<IReadOnlyList<TimesheetCorrectionDto>> GetPendingCorrectionApprovalsAsync(
        Guid reviewerId, CancellationToken cancellationToken = default)
    {
        var corrections = await dbContext.TimesheetCorrectionRequests
            .Include(r => r.TimesheetEntry)
            .Include(r => r.Employee)
            .Where(r => r.Status == TimesheetCorrectionStatus.Pending)
            .OrderBy(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return corrections
            .Select(r => ToCorrectionDto(r, r.TimesheetEntry!, r.Employee!.FullName, r.Employee.ManagerId == reviewerId))
            .OrderByDescending(dto => dto.IsDirectReport)
            .ToList();
    }

    public async Task<Result<TimesheetCorrectionDto>> ReviewCorrectionAsync(
        Guid correctionId, Guid reviewerId, ReviewTimesheetCorrectionRequest review, CancellationToken cancellationToken = default)
    {
        var correction = await dbContext.TimesheetCorrectionRequests
            .Include(r => r.TimesheetEntry)
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == correctionId, cancellationToken);

        if (correction is null)
        {
            return Result<TimesheetCorrectionDto>.Failure("Correction request not found.");
        }

        if (correction.Status != TimesheetCorrectionStatus.Pending)
        {
            return Result<TimesheetCorrectionDto>.Failure("This correction request has already been reviewed.");
        }

        correction.ReviewerId = reviewerId;
        correction.ReviewedAtUtc = DateTime.UtcNow;
        correction.Status = review.Approve ? TimesheetCorrectionStatus.Approved : TimesheetCorrectionStatus.Rejected;
        correction.RejectionReason = review.Approve ? null : review.RejectionReason;

        // Capture the pre-correction times for the response before mutating the entry.
        var entry = correction.TimesheetEntry!;
        var originalClockIn = entry.ClockInUtc;
        var originalClockOut = entry.ClockOutUtc;

        if (review.Approve)
        {
            if (correction.RequestedClockInUtc is not null)
            {
                entry.ClockInUtc = correction.RequestedClockInUtc.Value;
            }
            if (correction.RequestedClockOutUtc is not null)
            {
                entry.ClockOutUtc = correction.RequestedClockOutUtc.Value;
            }
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            correction.EmployeeId,
            review.Approve
                ? "Your timesheet correction request was approved."
                : "Your timesheet correction request was rejected.",
            "/timesheet",
            cancellationToken);

        return Result<TimesheetCorrectionDto>.Success(new TimesheetCorrectionDto(
            correction.Id,
            correction.TimesheetEntryId,
            correction.EmployeeId,
            correction.Employee!.FullName,
            originalClockIn,
            originalClockOut,
            correction.RequestedClockInUtc,
            correction.RequestedClockOutUtc,
            correction.Reason,
            correction.Status,
            correction.CreatedAtUtc,
            correction.ReviewedAtUtc,
            correction.RejectionReason,
            false));
    }

    private static TimesheetEntryDto ToDto(TimesheetEntry entry) => new(
        entry.Id,
        entry.ClockInUtc,
        entry.ClockOutUtc,
        entry.ClockOutUtc.HasValue ? (entry.ClockOutUtc.Value - entry.ClockInUtc).TotalHours : null);

    private static TimesheetCorrectionDto ToCorrectionDto(
        TimesheetCorrectionRequest correction, TimesheetEntry entry, string employeeName, bool isDirectReport = false) => new(
        correction.Id,
        correction.TimesheetEntryId,
        correction.EmployeeId,
        employeeName,
        entry.ClockInUtc,
        entry.ClockOutUtc,
        correction.RequestedClockInUtc,
        correction.RequestedClockOutUtc,
        correction.Reason,
        correction.Status,
        correction.CreatedAtUtc,
        correction.ReviewedAtUtc,
        correction.RejectionReason,
        isDirectReport);
}
