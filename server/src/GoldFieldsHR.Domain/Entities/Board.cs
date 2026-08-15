namespace GoldFieldsHR.Domain.Entities;

public class Board
{
    public Guid Id { get; set; }

    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }

    public Guid OwnerEmployeeId { get; set; }
    public Employee? OwnerEmployee { get; set; }

    public Guid? SiteId { get; set; }
    public Site? Site { get; set; }

    public bool IsArchived { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<BoardMember> Members { get; set; } = new List<BoardMember>();
    public ICollection<BoardTask> Tasks { get; set; } = new List<BoardTask>();
}
