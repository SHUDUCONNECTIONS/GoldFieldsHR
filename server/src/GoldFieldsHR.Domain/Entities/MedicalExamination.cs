using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Domain.Entities;

public class MedicalExamination
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateOnly ExamDate { get; set; }
    public DateOnly ExpiryDate { get; set; }
    public FitnessStatus Status { get; set; }
    public string? Restrictions { get; set; }
    public string? Notes { get; set; }

    public Guid ExaminedByEmployeeId { get; set; }
    public Employee? ExaminedByEmployee { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
}
