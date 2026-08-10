using Microsoft.AspNetCore.Identity;

namespace GoldFieldsHR.Infrastructure.Identity;

public class AppUser : IdentityUser<Guid>
{
    public Guid EmployeeId { get; set; }
}
