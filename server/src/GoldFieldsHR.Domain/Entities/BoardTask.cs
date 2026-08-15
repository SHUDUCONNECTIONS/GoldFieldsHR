using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Domain.Entities;

public class BoardTask
{
    public Guid Id { get; set; }

    public Guid BoardId { get; set; }
    public Board? Board { get; set; }

    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid? AssigneeEmployeeId { get; set; }
    public Employee? AssigneeEmployee { get; set; }

    public Guid CreatedByEmployeeId { get; set; }
    public Employee? CreatedByEmployee { get; set; }

    public BoardTaskStatus Status { get; set; } = BoardTaskStatus.Todo;
    public DateOnly? DueDate { get; set; }

    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime? CompletedAtUtc { get; set; }
}
