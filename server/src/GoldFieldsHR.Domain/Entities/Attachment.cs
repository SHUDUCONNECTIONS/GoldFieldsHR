using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Domain.Entities;

public class Attachment
{
    public Guid Id { get; set; }

    public AttachmentEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }

    public string FileName { get; set; } = string.Empty;
    public string StoredFileName { get; set; } = string.Empty;
    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }

    public Guid UploadedByEmployeeId { get; set; }
    public Employee? UploadedByEmployee { get; set; }
    public DateTime UploadedAtUtc { get; set; } = DateTime.UtcNow;
}
