using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Sites;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace GoldFieldsHR.Infrastructure.Sites;

public class SiteService(ApplicationDbContext dbContext) : ISiteService
{
    public async Task<IReadOnlyList<SiteDto>> GetActiveAsync(CancellationToken cancellationToken = default)
    {
        var sites = await dbContext.Sites
            .Where(s => s.IsActive)
            .Select(s => new { s.Id, s.Name, s.Location, s.IsActive, EmployeeCount = s.Employees.Count })
            .ToListAsync(cancellationToken);

        return sites.Select(s => new SiteDto(s.Id, s.Name, s.Location, s.IsActive, s.EmployeeCount)).ToList();
    }

    public async Task<IReadOnlyList<SiteDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var sites = await dbContext.Sites
            .OrderBy(s => s.Name)
            .Select(s => new { s.Id, s.Name, s.Location, s.IsActive, EmployeeCount = s.Employees.Count })
            .ToListAsync(cancellationToken);

        return sites.Select(s => new SiteDto(s.Id, s.Name, s.Location, s.IsActive, s.EmployeeCount)).ToList();
    }

    public async Task<Result<SiteDto>> CreateAsync(CreateSiteRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<SiteDto>.Failure("Site name is required.");
        }

        var nameTaken = await dbContext.Sites
            .AnyAsync(s => s.Name.ToLower() == request.Name.ToLower(), cancellationToken);
        if (nameTaken)
        {
            return Result<SiteDto>.Failure($"A site named '{request.Name}' already exists.");
        }

        var site = new Site
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            Location = request.Location,
        };

        dbContext.Sites.Add(site);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<SiteDto>.Success(new SiteDto(site.Id, site.Name, site.Location, site.IsActive, 0));
    }

    public async Task<Result<SiteDto>> UpdateAsync(
        Guid id, UpdateSiteRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return Result<SiteDto>.Failure("Site name is required.");
        }

        var site = await dbContext.Sites.FindAsync([id], cancellationToken);
        if (site is null)
        {
            return Result<SiteDto>.Failure("Site not found.");
        }

        var nameTaken = await dbContext.Sites
            .AnyAsync(s => s.Id != id && s.Name.ToLower() == request.Name.ToLower(), cancellationToken);
        if (nameTaken)
        {
            return Result<SiteDto>.Failure($"A site named '{request.Name}' already exists.");
        }

        site.Name = request.Name;
        site.Location = request.Location;
        await dbContext.SaveChangesAsync(cancellationToken);

        var employeeCount = await dbContext.Employees.CountAsync(e => e.SiteId == id, cancellationToken);
        return Result<SiteDto>.Success(new SiteDto(site.Id, site.Name, site.Location, site.IsActive, employeeCount));
    }

    public async Task<Result<SiteDto>> SetActiveStatusAsync(
        Guid id, SetSiteActiveStatusRequest request, CancellationToken cancellationToken = default)
    {
        var site = await dbContext.Sites.FindAsync([id], cancellationToken);
        if (site is null)
        {
            return Result<SiteDto>.Failure("Site not found.");
        }

        if (!request.IsActive)
        {
            var hasActiveEmployees = await dbContext.Employees
                .AnyAsync(e => e.SiteId == id && e.IsActive, cancellationToken);
            if (hasActiveEmployees)
            {
                return Result<SiteDto>.Failure("Cannot deactivate a site with active employees assigned to it.");
            }
        }

        site.IsActive = request.IsActive;
        await dbContext.SaveChangesAsync(cancellationToken);

        var employeeCount = await dbContext.Employees.CountAsync(e => e.SiteId == id, cancellationToken);
        return Result<SiteDto>.Success(new SiteDto(site.Id, site.Name, site.Location, site.IsActive, employeeCount));
    }
}
