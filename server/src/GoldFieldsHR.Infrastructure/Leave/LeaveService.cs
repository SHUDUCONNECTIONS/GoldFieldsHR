using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Leave;
using GoldFieldsHR.Application.Notifications;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Common;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GoldFieldsHR.Infrastructure.Leave;

public class LeaveService(ApplicationDbContext dbContext, INotificationService notificationService) : ILeaveService
{
    public async Task<Result<LeaveRequestDto>> SubmitAsync(
        Guid employeeId, SubmitLeaveRequest request, CancellationToken cancellationToken = default)
    {
        if (request.EndDate < request.StartDate)
        {
            return Result<LeaveRequestDto>.Failure("End date cannot be before the start date.");
        }

        var employee = await dbContext.Employees.FindAsync([employeeId], cancellationToken);
        if (employee is null)
        {
            return Result<LeaveRequestDto>.Failure("Employee profile not found.");
        }

        var entity = new LeaveRequest
        {
            Id = Guid.NewGuid(),
            EmployeeId = employeeId,
            LeaveType = request.LeaveType,
            StartDate = request.StartDate,
            EndDate = request.EndDate,
            Reason = request.Reason,
            ContactNumber = request.ContactNumber,
        };

        dbContext.LeaveRequests.Add(entity);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<LeaveRequestDto>.Success(ToDto(entity, employee.FullName));
    }

    public async Task<IReadOnlyList<LeaveRequestDto>> GetMyRequestsAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        var entities = await dbContext.LeaveRequests
            .Include(r => r.Employee)
            .Where(r => r.EmployeeId == employeeId)
            .OrderByDescending(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);

        return entities.Select(r => ToDto(r, r.Employee!.FullName)).ToList();
    }

    public async Task<IReadOnlyList<LeaveRequestDto>> GetPendingLineManagerApprovalsAsync(
        Guid reviewerId, CancellationToken cancellationToken = default)
    {
        var entities = await GetEntitiesByStatusAsync(LeaveRequestStatus.PendingLineManagerApproval, cancellationToken);

        // Direct reports surface first; every other pending request still follows as a site-wide fallback.
        return entities
            .Select(r => ToDto(r, r.Employee!.FullName, r.Employee.ManagerId == reviewerId))
            .OrderByDescending(dto => dto.IsDirectReport)
            .ToList();
    }

    public async Task<IReadOnlyList<LeaveRequestDto>> GetPendingHRApprovalsAsync(
        CancellationToken cancellationToken = default)
    {
        var entities = await GetEntitiesByStatusAsync(LeaveRequestStatus.PendingHRApproval, cancellationToken);
        return entities.Select(r => ToDto(r, r.Employee!.FullName)).ToList();
    }

    public async Task<Result<LeaveRequestDto>> LineManagerReviewAsync(
        Guid requestId, Guid reviewerId, ReviewLeaveRequest review, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.LeaveRequests
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (entity is null)
        {
            return Result<LeaveRequestDto>.Failure("Leave request not found.");
        }

        if (entity.Status != LeaveRequestStatus.PendingLineManagerApproval)
        {
            return Result<LeaveRequestDto>.Failure("This request is no longer awaiting line manager approval.");
        }

        var signatureResult = await ResolveReviewerSignatureAsync(reviewerId, review.SignaturePngBase64, cancellationToken);
        if (!signatureResult.Succeeded)
        {
            return Result<LeaveRequestDto>.Failure(signatureResult.Error!);
        }

        entity.LineManagerReviewerId = reviewerId;
        entity.LineManagerReviewedAtUtc = DateTime.UtcNow;
        entity.LineManagerSignatureImageData = review.Approve ? signatureResult.Value : null;
        entity.Status = review.Approve ? LeaveRequestStatus.PendingHRApproval : LeaveRequestStatus.Rejected;
        entity.RejectionReason = review.Approve ? null : review.RejectionReason;

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            entity.EmployeeId,
            review.Approve
                ? $"Your {entity.LeaveType} leave request ({entity.StartDate:d MMM} - {entity.EndDate:d MMM}) was approved by your Line Manager and is now awaiting HR approval."
                : $"Your {entity.LeaveType} leave request ({entity.StartDate:d MMM} - {entity.EndDate:d MMM}) was rejected by your Line Manager.",
            "/leave",
            cancellationToken);

        return Result<LeaveRequestDto>.Success(ToDto(entity, entity.Employee!.FullName));
    }

    public async Task<Result<LeaveRequestDto>> HRReviewAsync(
        Guid requestId, Guid reviewerId, ReviewLeaveRequest review, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.LeaveRequests
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (entity is null)
        {
            return Result<LeaveRequestDto>.Failure("Leave request not found.");
        }

        if (entity.Status != LeaveRequestStatus.PendingHRApproval)
        {
            return Result<LeaveRequestDto>.Failure("This request is no longer awaiting HR approval.");
        }

        var signatureResult = await ResolveReviewerSignatureAsync(reviewerId, review.SignaturePngBase64, cancellationToken);
        if (!signatureResult.Succeeded)
        {
            return Result<LeaveRequestDto>.Failure(signatureResult.Error!);
        }

        entity.HRReviewerId = reviewerId;
        entity.HRReviewedAtUtc = DateTime.UtcNow;
        entity.HRSignatureImageData = review.Approve ? signatureResult.Value : null;
        entity.Status = review.Approve ? LeaveRequestStatus.Approved : LeaveRequestStatus.Rejected;
        entity.RejectionReason = review.Approve ? null : review.RejectionReason;

        await dbContext.SaveChangesAsync(cancellationToken);

        await notificationService.CreateAsync(
            entity.EmployeeId,
            review.Approve
                ? $"Your {entity.LeaveType} leave request ({entity.StartDate:d MMM} - {entity.EndDate:d MMM}) was fully approved. You can download the signed leave form from the Leave page."
                : $"Your {entity.LeaveType} leave request ({entity.StartDate:d MMM} - {entity.EndDate:d MMM}) was rejected by HR.",
            "/leave",
            cancellationToken);

        return Result<LeaveRequestDto>.Success(ToDto(entity, entity.Employee!.FullName));
    }

    public async Task<Result<byte[]>> GenerateSignedDocumentAsync(
        Guid requestId, Guid requesterId, CancellationToken cancellationToken = default)
    {
        var entity = await dbContext.LeaveRequests
            .Include(r => r.Employee)
            .FirstOrDefaultAsync(r => r.Id == requestId, cancellationToken);

        if (entity is null)
        {
            return Result<byte[]>.Failure("Leave request not found.");
        }

        if (entity.Status != LeaveRequestStatus.Approved)
        {
            return Result<byte[]>.Failure("This leave request has not been fully approved yet.");
        }

        var requester = await dbContext.Employees.FindAsync([requesterId], cancellationToken);
        if (requester is null)
        {
            return Result<byte[]>.Failure("Employee profile not found.");
        }

        var isOwner = entity.EmployeeId == requesterId;
        var isReviewer = requester.Role is EmployeeRole.LineManager or EmployeeRole.HR or EmployeeRole.Executive;
        if (!isOwner && !isReviewer)
        {
            return Result<byte[]>.Failure("You are not authorized to download this document.");
        }

        var lineManagerName = entity.LineManagerReviewerId is null
            ? null
            : (await dbContext.Employees.FindAsync([entity.LineManagerReviewerId.Value], cancellationToken))?.FullName;
        var hrName = entity.HRReviewerId is null
            ? null
            : (await dbContext.Employees.FindAsync([entity.HRReviewerId.Value], cancellationToken))?.FullName;

        var pdfBytes = BuildLeaveDocument(entity, lineManagerName, hrName);
        return Result<byte[]>.Success(pdfBytes);
    }

    private async Task<Result<byte[]>> ResolveReviewerSignatureAsync(
        Guid reviewerId, string? signaturePngBase64, CancellationToken cancellationToken)
    {
        var reviewer = await dbContext.Employees.FindAsync([reviewerId], cancellationToken);
        if (reviewer is null)
        {
            return Result<byte[]>.Failure("Employee profile not found.");
        }

        if (reviewer.SignatureImageData is not null)
        {
            return Result<byte[]>.Success(reviewer.SignatureImageData);
        }

        if (string.IsNullOrWhiteSpace(signaturePngBase64))
        {
            return Result<byte[]>.Failure("Please sign to approve or reject this request.");
        }

        byte[] signatureBytes;
        try
        {
            signatureBytes = SignatureImageCodec.Decode(signaturePngBase64);
        }
        catch (FormatException)
        {
            return Result<byte[]>.Failure("The signature image could not be read.");
        }

        reviewer.SignatureImageData = signatureBytes;
        reviewer.SignatureUpdatedAtUtc = DateTime.UtcNow;

        return Result<byte[]>.Success(signatureBytes);
    }

    private async Task<List<LeaveRequest>> GetEntitiesByStatusAsync(
        LeaveRequestStatus status, CancellationToken cancellationToken)
    {
        return await dbContext.LeaveRequests
            .Include(r => r.Employee)
            .Where(r => r.Status == status)
            .OrderBy(r => r.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    private static byte[] BuildLeaveDocument(LeaveRequest entity, string? lineManagerName, string? hrName)
    {
        var days = entity.EndDate.DayNumber - entity.StartDate.DayNumber + 1;

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(36);
                page.DefaultTextStyle(x => x.FontSize(11));

                page.Header().Element(header => PdfBranding.RenderLetterhead(header, "Leave Application — Approved"));

                page.Content().PaddingTop(20).Column(column =>
                {
                    column.Spacing(6);

                    column.Item().Table(table =>
                    {
                        table.ColumnsDefinition(columns =>
                        {
                            columns.RelativeColumn(1);
                            columns.RelativeColumn(2);
                        });

                        void Row(string label, string value)
                        {
                            table.Cell().Text(label).Bold();
                            table.Cell().Text(value);
                        }

                        Row("Employee", entity.Employee!.FullName);
                        Row("Employee number", entity.Employee.EmployeeNumber);
                        Row("Leave type", entity.LeaveType.ToString());
                        Row("Dates", $"{entity.StartDate:d MMM yyyy} - {entity.EndDate:d MMM yyyy} ({days} day{(days == 1 ? "" : "s")})");
                        Row("Reason", entity.Reason);
                        Row("Contact number during leave", entity.ContactNumber);
                        Row("Submitted", $"{entity.CreatedAtUtc:d MMM yyyy HH:mm} UTC");
                    });

                    column.Item().PaddingTop(20).Text("Approvals").FontSize(13).Bold();

                    column.Item().Row(row =>
                    {
                        row.RelativeItem().Column(c => SignatureBlock(c, "Line Manager", lineManagerName, entity.LineManagerSignatureImageData, entity.LineManagerReviewedAtUtc));
                        row.ConstantItem(20);
                        row.RelativeItem().Column(c => SignatureBlock(c, "HR", hrName, entity.HRSignatureImageData, entity.HRReviewedAtUtc));
                    });
                });

                page.Footer().AlignCenter().Text(
                    $"Generated {DateTime.UtcNow:d MMM yyyy HH:mm} UTC").FontSize(9).FontColor(Colors.Grey.Medium);
            });
        }).GeneratePdf();
    }

    private static void SignatureBlock(
        ColumnDescriptor column, string title, string? reviewerName, byte[]? signature, DateTime? reviewedAtUtc)
    {
        column.Item().Text(title).Bold();

        if (signature is not null)
        {
            column.Item().Height(50).Image(signature).FitArea();
        }
        else
        {
            column.Item().Height(50);
        }

        column.Item().LineHorizontal(0.75f).LineColor(Colors.Grey.Lighten1);
        column.Item().Text(reviewerName ?? "—").FontSize(10);
        column.Item().Text(reviewedAtUtc is null ? "—" : $"{reviewedAtUtc:d MMM yyyy HH:mm} UTC").FontSize(9).FontColor(Colors.Grey.Darken1);
    }

    private static LeaveRequestDto ToDto(LeaveRequest entity, string employeeName, bool isDirectReport = false) => new(
        entity.Id,
        entity.EmployeeId,
        employeeName,
        entity.LeaveType,
        entity.StartDate,
        entity.EndDate,
        entity.EndDate.DayNumber - entity.StartDate.DayNumber + 1,
        entity.Reason,
        entity.ContactNumber,
        entity.Status,
        entity.CreatedAtUtc,
        entity.LineManagerReviewedAtUtc,
        entity.HRReviewedAtUtc,
        entity.RejectionReason,
        isDirectReport);
}
