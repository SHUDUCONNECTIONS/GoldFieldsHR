using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Tests;

public static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new ApplicationDbContext(options);
    }

    public static Employee AddEmployee(
        this ApplicationDbContext dbContext, EmployeeRole role, string? employeeNumber = null)
    {
        var site = dbContext.Sites.FirstOrDefault();
        if (site is null)
        {
            site = new Site { Id = Guid.NewGuid(), Name = "Test Site", Location = "Test Location" };
            dbContext.Sites.Add(site);
        }

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            UserId = Guid.NewGuid(),
            EmployeeNumber = employeeNumber ?? $"EMP-{Guid.NewGuid():N}"[..12],
            FirstName = "Test",
            LastName = role.ToString(),
            JobTitle = "Test Role",
            Role = role,
            SiteId = site.Id,
        };

        dbContext.Employees.Add(employee);
        dbContext.SaveChanges();

        return employee;
    }
}
