using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Domain.Entities;

public class Acknowledgment
{
    public Guid Id { get; set; }

    public AcknowledgmentEntityType EntityType { get; set; }
    public Guid EntityId { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
