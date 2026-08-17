using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace GoldFieldsHR.Infrastructure.Persistence;

public static class DatabaseSeeder
{
    // Dev-only bootstrap credentials. Since public self-registration only allows the
    // Employee role, at least one HR account must already exist to assign other roles.
    private const string BootstrapPassword = "Bootstrap@123";

    public static async Task SeedAsync(IServiceProvider services)
    {
        await SeedRolesAsync(services);
        var site = await SeedDefaultSiteAsync(services);
        await SeedBootstrapAccountAsync(services, site, "hr.admin@goldfieldshr.local", "HR-ADMIN-001", EmployeeRole.HR);
        await SeedBootstrapAccountAsync(services, site, "exec.admin@goldfieldshr.local", "EXEC-ADMIN-001", EmployeeRole.Executive);
        await SeedKpiTemplatesAsync(services);
    }

    private static async Task SeedKpiTemplatesAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        if (await dbContext.KpiTemplates.AnyAsync())
        {
            return;
        }

        dbContext.KpiTemplates.AddRange(Kpi.KpiTemplateSeedData.BuildTemplates());
        await dbContext.SaveChangesAsync();
    }

    private static async Task SeedRolesAsync(IServiceProvider services)
    {
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole<Guid>>>();

        foreach (var role in Enum.GetValues<EmployeeRole>())
        {
            var roleName = role.ToString();
            if (!await roleManager.RoleExistsAsync(roleName))
            {
                await roleManager.CreateAsync(new IdentityRole<Guid>(roleName));
            }
        }
    }

    private static async Task<Site> SeedDefaultSiteAsync(IServiceProvider services)
    {
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        var site = await dbContext.Sites.FirstOrDefaultAsync();
        if (site is null)
        {
            site = new Site
            {
                Id = Guid.NewGuid(),
                Name = "Kusasalethu Mine",
                Location = "South Africa"
            };
            dbContext.Sites.Add(site);
            await dbContext.SaveChangesAsync();
        }

        return site;
    }

    private static async Task SeedBootstrapAccountAsync(
        IServiceProvider services, Site site, string email, string employeeNumber, EmployeeRole role)
    {
        var userManager = services.GetRequiredService<UserManager<AppUser>>();
        var dbContext = services.GetRequiredService<ApplicationDbContext>();

        if (await userManager.FindByEmailAsync(email) is not null)
        {
            return;
        }

        var user = new AppUser { UserName = email, Email = email };
        var createResult = await userManager.CreateAsync(user, BootstrapPassword);
        if (!createResult.Succeeded)
        {
            return;
        }

        await userManager.AddToRoleAsync(user, role.ToString());

        var employee = new Employee
        {
            Id = Guid.NewGuid(),
            UserId = user.Id,
            EmployeeNumber = employeeNumber,
            FirstName = role.ToString(),
            LastName = "Admin",
            JobTitle = $"{role} Administrator",
            Role = role,
            SiteId = site.Id,
        };

        user.EmployeeId = employee.Id;

        dbContext.Employees.Add(employee);
        await dbContext.SaveChangesAsync();
        await userManager.UpdateAsync(user);
    }
}
