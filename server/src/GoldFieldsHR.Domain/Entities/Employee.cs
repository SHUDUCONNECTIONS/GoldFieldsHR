using GoldFieldsHR.Domain.Enums;

namespace GoldFieldsHR.Domain.Entities;

public class Employee
{
    public Guid Id { get; set; }

    // Links to the Infrastructure-layer Identity user (AppUser.Id) that owns this record.
    public Guid UserId { get; set; }

    public string EmployeeNumber { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string JobTitle { get; set; } = string.Empty;
    public EmployeeRole Role { get; set; }
    public EmployeeRole? RequestedRole { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public Guid SiteId { get; set; }
    public Site? Site { get; set; }

    public Guid? ManagerId { get; set; }
    public Employee? Manager { get; set; }

    public string FullName => $"{FirstName} {LastName}".Trim();
}
