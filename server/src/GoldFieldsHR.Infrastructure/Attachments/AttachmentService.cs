using GoldFieldsHR.Application.Attachments;
using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace GoldFieldsHR.Infrastructure.Attachments;

public class AttachmentService(ApplicationDbContext dbContext, IOptions<FileStorageSettings> storageOptions) : IAttachmentService
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024;

    private static readonly HashSet<string> AllowedContentTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "application/pdf",
        "image/jpeg",
        "image/png",
    };

    public async Task<Result<AttachmentDto>> UploadAsync(
        AttachmentEntityType entityType, Guid entityId, Guid uploaderEmployeeId,
        UploadAttachmentRequest request, CancellationToken cancellationToken = default)
    {
        if (request.Content.Length == 0)
        {
            return Result<AttachmentDto>.Failure("The uploaded file is empty.");
        }

        if (request.Content.Length > MaxFileSizeBytes)
        {
            return Result<AttachmentDto>.Failure("File exceeds the 10MB limit.");
        }

        if (!AllowedContentTypes.Contains(request.ContentType))
        {
            return Result<AttachmentDto>.Failure("Only PDF, JPEG, and PNG files are allowed.");
        }

        var uploader = await dbContext.Employees.FindAsync([uploaderEmployeeId], cancellationToken);
        if (uploader is null)
        {
            return Result<AttachmentDto>.Failure("Employee profile not found.");
        }

        var access = await GetAccessAsync(entityType, entityId, uploader, cancellationToken);
        if (access is null)
        {
            return Result<AttachmentDto>.Failure("The record you're attaching a file to could not be found.");
        }

        if (!access.Value.CanUpload)
        {
            return Result<AttachmentDto>.Failure("You are not authorized to attach files to this record.");
        }

        var root = ResolveRootPath();
        Directory.CreateDirectory(root);
        var storedFileName = $"{Guid.NewGuid()}{Path.GetExtension(request.FileName)}";
        await File.WriteAllBytesAsync(Path.Combine(root, storedFileName), request.Content, cancellationToken);

        var attachment = new Attachment
        {
            Id = Guid.NewGuid(),
            EntityType = entityType,
            EntityId = entityId,
            FileName = request.FileName,
            StoredFileName = storedFileName,
            ContentType = request.ContentType,
            SizeBytes = request.Content.Length,
            UploadedByEmployeeId = uploaderEmployeeId,
        };

        dbContext.Attachments.Add(attachment);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<AttachmentDto>.Success(ToDto(attachment, uploader.FullName));
    }

    public async Task<Result<IReadOnlyList<AttachmentDto>>> GetForEntityAsync(
        AttachmentEntityType entityType, Guid entityId, Guid requesterId, CancellationToken cancellationToken = default)
    {
        var requester = await dbContext.Employees.FindAsync([requesterId], cancellationToken);
        if (requester is null)
        {
            return Result<IReadOnlyList<AttachmentDto>>.Failure("Employee profile not found.");
        }

        var access = await GetAccessAsync(entityType, entityId, requester, cancellationToken);
        if (access is null)
        {
            return Result<IReadOnlyList<AttachmentDto>>.Failure("Record not found.");
        }

        if (!access.Value.CanView)
        {
            return Result<IReadOnlyList<AttachmentDto>>.Failure("You are not authorized to view attachments on this record.");
        }

        var attachments = await dbContext.Attachments
            .Include(a => a.UploadedByEmployee)
            .Where(a => a.EntityType == entityType && a.EntityId == entityId)
            .OrderByDescending(a => a.UploadedAtUtc)
            .ToListAsync(cancellationToken);

        return Result<IReadOnlyList<AttachmentDto>>.Success(
            attachments.Select(a => ToDto(a, a.UploadedByEmployee!.FullName)).ToList());
    }

    public async Task<Result<AttachmentContentDto>> DownloadAsync(
        Guid attachmentId, Guid requesterId, CancellationToken cancellationToken = default)
    {
        var attachment = await dbContext.Attachments.FindAsync([attachmentId], cancellationToken);
        if (attachment is null)
        {
            return Result<AttachmentContentDto>.Failure("Attachment not found.");
        }

        var requester = await dbContext.Employees.FindAsync([requesterId], cancellationToken);
        if (requester is null)
        {
            return Result<AttachmentContentDto>.Failure("Employee profile not found.");
        }

        var access = await GetAccessAsync(attachment.EntityType, attachment.EntityId, requester, cancellationToken);
        if (access is null || !access.Value.CanView)
        {
            return Result<AttachmentContentDto>.Failure("You are not authorized to view this attachment.");
        }

        var filePath = Path.Combine(ResolveRootPath(), attachment.StoredFileName);
        if (!File.Exists(filePath))
        {
            return Result<AttachmentContentDto>.Failure("The file could not be found on disk.");
        }

        var content = await File.ReadAllBytesAsync(filePath, cancellationToken);
        return Result<AttachmentContentDto>.Success(new AttachmentContentDto(attachment.FileName, attachment.ContentType, content));
    }

    private async Task<(bool CanUpload, bool CanView)?> GetAccessAsync(
        AttachmentEntityType entityType, Guid entityId, Employee requester, CancellationToken cancellationToken)
    {
        switch (entityType)
        {
            case AttachmentEntityType.Policy:
                var policyExists = await dbContext.Policies.AnyAsync(p => p.Id == entityId, cancellationToken);
                if (!policyExists) return null;
                // Policies are visible org-wide; only HR (who publishes them) can attach documents.
                return (requester.Role == EmployeeRole.HR, true);

            case AttachmentEntityType.Certificate:
                var certificate = await dbContext.Certificates
                    .FirstOrDefaultAsync(c => c.Id == entityId, cancellationToken);
                if (certificate is null) return null;
                var isCertOwner = certificate.EmployeeId == requester.Id;
                var isCertManager = requester.Role == EmployeeRole.HR;
                return (isCertManager, isCertOwner || isCertManager);

            case AttachmentEntityType.MedicalExamination:
                var exam = await dbContext.MedicalExaminations
                    .FirstOrDefaultAsync(m => m.Id == entityId, cancellationToken);
                if (exam is null) return null;
                var isExamOwner = exam.EmployeeId == requester.Id;
                var isExamManager = requester.Role == EmployeeRole.Medical;
                return (isExamManager, isExamOwner || isExamManager);

            case AttachmentEntityType.IncidentReport:
                var incident = await dbContext.IncidentReports
                    .FirstOrDefaultAsync(i => i.Id == entityId, cancellationToken);
                if (incident is null) return null;
                var isIncidentOwner = incident.EmployeeId == requester.Id;
                var isIncidentReviewer = requester.Role is EmployeeRole.SafetyOfficer or EmployeeRole.HR or EmployeeRole.Executive;
                return (isIncidentOwner || isIncidentReviewer, isIncidentOwner || isIncidentReviewer);

            case AttachmentEntityType.LeaveRequest:
                var leaveRequest = await dbContext.LeaveRequests
                    .FirstOrDefaultAsync(l => l.Id == entityId, cancellationToken);
                if (leaveRequest is null) return null;
                var isLeaveOwner = leaveRequest.EmployeeId == requester.Id;
                var isLeaveReviewer = requester.Role is EmployeeRole.LineManager or EmployeeRole.HR or EmployeeRole.Executive;
                // Only the employee attaches their own medical certificate; owner + reviewers can view it.
                return (isLeaveOwner, isLeaveOwner || isLeaveReviewer);

            case AttachmentEntityType.LegalAppointment:
                var appointment = await dbContext.LegalAppointments
                    .FirstOrDefaultAsync(a => a.Id == entityId, cancellationToken);
                if (appointment is null) return null;
                var isAppointmentOwner = appointment.EmployeeId == requester.Id;
                var isAppointmentManager = requester.Role == EmployeeRole.SafetyOfficer;
                return (isAppointmentManager, isAppointmentOwner || isAppointmentManager);

            case AttachmentEntityType.BoardTask:
                var task = await dbContext.BoardTasks
                    .FirstOrDefaultAsync(t => t.Id == entityId, cancellationToken);
                if (task is null) return null;
                var isTaskAssignee = task.AssigneeEmployeeId == requester.Id;
                var isBoardOwner = await dbContext.Boards
                    .AnyAsync(b => b.Id == task.BoardId && b.OwnerEmployeeId == requester.Id, cancellationToken);
                return (isTaskAssignee || isBoardOwner, isTaskAssignee || isBoardOwner);

            default:
                return null;
        }
    }

    private string ResolveRootPath() =>
        string.IsNullOrWhiteSpace(storageOptions.Value.UploadsRootPath)
            ? Path.Combine(AppContext.BaseDirectory, "App_Data", "uploads")
            : storageOptions.Value.UploadsRootPath;

    private static AttachmentDto ToDto(Attachment attachment, string uploadedByName) => new(
        attachment.Id,
        attachment.EntityType,
        attachment.EntityId,
        attachment.FileName,
        attachment.ContentType,
        attachment.SizeBytes,
        uploadedByName,
        attachment.UploadedAtUtc);
}
