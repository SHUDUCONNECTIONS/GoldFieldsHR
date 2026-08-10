namespace GoldFieldsHR.Domain.Entities;

public class TimesheetEntry
{
    public Guid Id { get; set; }

    public Guid EmployeeId { get; set; }
    public Employee? Employee { get; set; }

    public DateTime ClockInUtc { get; set; }
    public DateTime? ClockOutUtc { get; set; }
}
