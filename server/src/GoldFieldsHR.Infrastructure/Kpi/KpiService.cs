using GoldFieldsHR.Application.Common;
using GoldFieldsHR.Application.Kpi;
using GoldFieldsHR.Domain.Entities;
using GoldFieldsHR.Domain.Enums;
using GoldFieldsHR.Infrastructure.Common;
using GoldFieldsHR.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace GoldFieldsHR.Infrastructure.Kpi;

public class KpiService(ApplicationDbContext dbContext) : IKpiService
{
    public async Task<IReadOnlyList<KpiTemplateSummaryDto>> GetTemplatesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.KpiTemplates
            .OrderBy(t => t.Designation)
            .Select(t => new KpiTemplateSummaryDto(
                t.Id,
                t.Designation,
                t.IsActive,
                t.Categories.Count,
                t.Categories.SelectMany(c => c.Items).Count(),
                t.CreatedAtUtc))
            .ToListAsync(cancellationToken);
    }

    public async Task<Result<KpiTemplateDetailDto>> GetTemplateByIdAsync(
        Guid templateId, CancellationToken cancellationToken = default)
    {
        var template = await dbContext.KpiTemplates
            .Include(t => t.Categories.OrderBy(c => c.DisplayOrder))
            .ThenInclude(c => c.Items.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        return template is null
            ? Result<KpiTemplateDetailDto>.Failure("KPI template not found.")
            : Result<KpiTemplateDetailDto>.Success(ToTemplateDetailDto(template));
    }

    public async Task<Result<KpiTemplateDetailDto>> CreateTemplateAsync(
        CreateKpiTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var template = new KpiTemplate { Id = Guid.NewGuid(), Designation = request.Designation };
        PopulateCategories(template, request);

        dbContext.KpiTemplates.Add(template);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<KpiTemplateDetailDto>.Success(ToTemplateDetailDto(template));
    }

    public async Task<Result<KpiTemplateDetailDto>> UpdateTemplateAsync(
        Guid templateId, CreateKpiTemplateRequest request, CancellationToken cancellationToken = default)
    {
        var template = await dbContext.KpiTemplates
            .Include(t => t.Categories)
            .ThenInclude(c => c.Items)
            .FirstOrDefaultAsync(t => t.Id == templateId, cancellationToken);

        if (template is null)
        {
            return Result<KpiTemplateDetailDto>.Failure("KPI template not found.");
        }

        template.Designation = request.Designation;

        // Full replace rather than diffing — templates are only edited by HR admins and
        // appraisals snapshot their own copy of the item text, so nothing downstream breaks.
        dbContext.KpiTemplateCategories.RemoveRange(template.Categories);
        template.Categories.Clear();
        PopulateCategories(template, request);

        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<KpiTemplateDetailDto>.Success(ToTemplateDetailDto(template));
    }

    public async Task<Result<bool>> DeactivateTemplateAsync(Guid templateId, CancellationToken cancellationToken = default)
    {
        var template = await dbContext.KpiTemplates.FindAsync([templateId], cancellationToken);
        if (template is null)
        {
            return Result<bool>.Failure("KPI template not found.");
        }

        template.IsActive = false;
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<bool>.Success(true);
    }

    public async Task<Result<KpiAppraisalDetailDto>> CreateAppraisalAsync(
        Guid createdByEmployeeId, CreateKpiAppraisalRequest request, CancellationToken cancellationToken = default)
    {
        var creator = await dbContext.Employees.FindAsync([createdByEmployeeId], cancellationToken);
        if (creator is null)
        {
            return Result<KpiAppraisalDetailDto>.Failure("Employee profile not found.");
        }

        var employee = await dbContext.Employees
            .FirstOrDefaultAsync(e => e.EmployeeNumber == request.EmployeeNumber, cancellationToken);
        if (employee is null)
        {
            return Result<KpiAppraisalDetailDto>.Failure($"No employee found with number '{request.EmployeeNumber}'.");
        }

        var template = await dbContext.KpiTemplates
            .Include(t => t.Categories.OrderBy(c => c.DisplayOrder))
            .ThenInclude(c => c.Items.OrderBy(i => i.DisplayOrder))
            .FirstOrDefaultAsync(t => t.Id == request.KpiTemplateId, cancellationToken);
        if (template is null)
        {
            return Result<KpiAppraisalDetailDto>.Failure("KPI template not found.");
        }

        var blastingOfficer = await dbContext.Employees
            .FirstOrDefaultAsync(e => e.EmployeeNumber == request.BlastingOfficerEmployeeNumber, cancellationToken);
        if (blastingOfficer is null)
        {
            return Result<KpiAppraisalDetailDto>.Failure(
                $"No employee found with number '{request.BlastingOfficerEmployeeNumber}' for Blasting Officer.");
        }

        var blastingEngineer = await dbContext.Employees
            .FirstOrDefaultAsync(e => e.EmployeeNumber == request.BlastingEngineerEmployeeNumber, cancellationToken);
        if (blastingEngineer is null)
        {
            return Result<KpiAppraisalDetailDto>.Failure(
                $"No employee found with number '{request.BlastingEngineerEmployeeNumber}' for Blasting Engineer.");
        }

        var appraisal = new KpiAppraisal
        {
            Id = Guid.NewGuid(),
            EmployeeId = employee.Id,
            KpiTemplateId = template.Id,
            PeriodLabel = request.PeriodLabel,
            InductionNumber = request.InductionNumber,
            CreatedByEmployeeId = createdByEmployeeId,
            Checkpoint1Date = request.Checkpoint1Date,
            Checkpoint2Date = request.Checkpoint2Date,
            Checkpoint3Date = request.Checkpoint3Date,
            Checkpoint4Date = request.Checkpoint4Date,
            BlastingOfficerEmployeeId = blastingOfficer.Id,
            BlastingEngineerEmployeeId = blastingEngineer.Id,
        };

        var order = 0;
        foreach (var category in template.Categories)
        {
            foreach (var item in category.Items)
            {
                appraisal.Items.Add(new KpiAppraisalItem
                {
                    Id = Guid.NewGuid(),
                    KpiAppraisalId = appraisal.Id,
                    KpiTemplateItemId = item.Id,
                    DescriptionSnapshot = item.Description,
                    CategoryNameSnapshot = category.Name,
                    SubGroupLabelSnapshot = item.SubGroupLabel,
                    DisplayOrder = order++,
                });
            }
        }

        dbContext.KpiAppraisals.Add(appraisal);
        await dbContext.SaveChangesAsync(cancellationToken);

        return Result<KpiAppraisalDetailDto>.Success(ToDetailDto(
            appraisal, employee.FullName, employee.EmployeeNumber, template.Designation,
            blastingOfficer.FullName, blastingEngineer.FullName));
    }

    public async Task<IReadOnlyList<KpiAppraisalSummaryDto>> GetMyAppraisalsAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        var appraisals = await LoadAppraisalsAsync(a => a.EmployeeId == employeeId, cancellationToken);
        return await ToSummaryDtosAsync(appraisals, cancellationToken);
    }

    public async Task<IReadOnlyList<KpiAppraisalSummaryDto>> GetAppraisalsIManageAsync(
        Guid managerEmployeeId, CancellationToken cancellationToken = default)
    {
        var appraisals = await LoadAppraisalsAsync(a => a.Employee!.ManagerId == managerEmployeeId, cancellationToken);
        return await ToSummaryDtosAsync(appraisals, cancellationToken);
    }

    public async Task<IReadOnlyList<KpiAppraisalSummaryDto>> GetAllAppraisalsAsync(CancellationToken cancellationToken = default)
    {
        var appraisals = await LoadAppraisalsAsync(_ => true, cancellationToken);
        return await ToSummaryDtosAsync(appraisals, cancellationToken);
    }

    public async Task<IReadOnlyList<KpiAppraisalSummaryDto>> GetPendingMySignOffAsync(
        Guid employeeId, CancellationToken cancellationToken = default)
    {
        var appraisals = await LoadAppraisalsAsync(
            a => (a.Status == KpiAppraisalStatus.InProgress && a.BlastingOfficerEmployeeId == employeeId)
                || (a.Status == KpiAppraisalStatus.PendingBlastingEngineerSignOff && a.BlastingEngineerEmployeeId == employeeId),
            cancellationToken);
        return await ToSummaryDtosAsync(appraisals, cancellationToken);
    }

    public async Task<Result<KpiAppraisalDetailDto>> GetAppraisalByIdAsync(
        Guid appraisalId, Guid requesterId, CancellationToken cancellationToken = default)
    {
        var appraisal = await LoadSingleAppraisalAsync(appraisalId, cancellationToken);
        if (appraisal is null)
        {
            return Result<KpiAppraisalDetailDto>.Failure("Appraisal not found.");
        }

        var requester = await dbContext.Employees.FindAsync([requesterId], cancellationToken);
        if (requester is null)
        {
            return Result<KpiAppraisalDetailDto>.Failure("Employee profile not found.");
        }

        if (!CanView(appraisal, requester))
        {
            return Result<KpiAppraisalDetailDto>.Failure("You are not authorized to view this appraisal.");
        }

        var names = await GetEmployeeNamesAsync(
            [appraisal.BlastingOfficerEmployeeId, appraisal.BlastingEngineerEmployeeId], cancellationToken);

        return Result<KpiAppraisalDetailDto>.Success(ToDetailDto(
            appraisal, appraisal.Employee!.FullName, appraisal.Employee.EmployeeNumber, appraisal.KpiTemplate!.Designation,
            names.GetValueOrDefault(appraisal.BlastingOfficerEmployeeId, "—"),
            names.GetValueOrDefault(appraisal.BlastingEngineerEmployeeId, "—")));
    }

    public async Task<Result<KpiAppraisalDetailDto>> SubmitCheckpointScoresAsync(
        Guid appraisalId, Guid submitterId, SubmitCheckpointScoresRequest request, CancellationToken cancellationToken = default)
    {
        var appraisal = await LoadSingleAppraisalAsync(appraisalId, cancellationToken);
        if (appraisal is null)
        {
            return Result<KpiAppraisalDetailDto>.Failure("Appraisal not found.");
        }

        if (appraisal.Status == KpiAppraisalStatus.Finalized)
        {
            return Result<KpiAppraisalDetailDto>.Failure("This appraisal has already been finalized.");
        }

        var submitter = await dbContext.Employees.FindAsync([submitterId], cancellationToken);
        if (submitter is null)
        {
            return Result<KpiAppraisalDetailDto>.Failure("Employee profile not found.");
        }

        if (!CanScore(appraisal, submitter))
        {
            return Result<KpiAppraisalDetailDto>.Failure("You are not authorized to score this appraisal.");
        }

        var itemsById = appraisal.Items.ToDictionary(i => i.Id);
        if (request.Items.Any(entry => !itemsById.ContainsKey(entry.ItemId)))
        {
            return Result<KpiAppraisalDetailDto>.Failure("One or more items do not belong to this appraisal.");
        }

        foreach (var entry in request.Items)
        {
            var item = itemsById[entry.ItemId];
            switch (request.CheckpointNumber)
            {
                case 1:
                    item.Checkpoint1Score = entry.Score;
                    item.Checkpoint1Comment = entry.Comment;
                    break;
                case 2:
                    item.Checkpoint2Score = entry.Score;
                    item.Checkpoint2Comment = entry.Comment;
                    break;
                case 3:
                    item.Checkpoint3Score = entry.Score;
                    item.Checkpoint3Comment = entry.Comment;
                    break;
                case 4:
                    item.Checkpoint4Score = entry.Score;
                    item.Checkpoint4Comment = entry.Comment;
                    break;
            }
        }

        appraisal.LastScoredAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return await BuildDetailResultAsync(appraisal, cancellationToken);
    }

    public async Task<Result<KpiAppraisalDetailDto>> SetItemFlagsAsync(
        Guid appraisalId, Guid submitterId, SetItemFlagsRequest request, CancellationToken cancellationToken = default)
    {
        var appraisal = await LoadSingleAppraisalAsync(appraisalId, cancellationToken);
        if (appraisal is null)
        {
            return Result<KpiAppraisalDetailDto>.Failure("Appraisal not found.");
        }

        if (appraisal.Status == KpiAppraisalStatus.Finalized)
        {
            return Result<KpiAppraisalDetailDto>.Failure("This appraisal has already been finalized.");
        }

        var submitter = await dbContext.Employees.FindAsync([submitterId], cancellationToken);
        if (submitter is null)
        {
            return Result<KpiAppraisalDetailDto>.Failure("Employee profile not found.");
        }

        if (!CanScore(appraisal, submitter))
        {
            return Result<KpiAppraisalDetailDto>.Failure("You are not authorized to update this appraisal.");
        }

        var itemsById = appraisal.Items.ToDictionary(i => i.Id);
        if (request.Items.Any(entry => !itemsById.ContainsKey(entry.ItemId)))
        {
            return Result<KpiAppraisalDetailDto>.Failure("One or more items do not belong to this appraisal.");
        }

        foreach (var entry in request.Items)
        {
            var item = itemsById[entry.ItemId];
            item.InPlace = entry.InPlace;
            item.Ability = entry.Ability;
        }

        await dbContext.SaveChangesAsync(cancellationToken);

        return await BuildDetailResultAsync(appraisal, cancellationToken);
    }

    public async Task<Result<KpiAppraisalDetailDto>> SignAsBlastingOfficerAsync(
        Guid appraisalId, Guid signerId, SignKpiAppraisalRequest request, CancellationToken cancellationToken = default)
    {
        var appraisal = await LoadSingleAppraisalAsync(appraisalId, cancellationToken);
        if (appraisal is null)
        {
            return Result<KpiAppraisalDetailDto>.Failure("Appraisal not found.");
        }

        if (appraisal.Status != KpiAppraisalStatus.InProgress)
        {
            return Result<KpiAppraisalDetailDto>.Failure("This appraisal is not awaiting Blasting Officer sign-off.");
        }

        var signer = await dbContext.Employees.FindAsync([signerId], cancellationToken);
        if (signer is null)
        {
            return Result<KpiAppraisalDetailDto>.Failure("Employee profile not found.");
        }

        if (appraisal.BlastingOfficerEmployeeId != signerId && signer.Role is not (EmployeeRole.HR or EmployeeRole.Executive))
        {
            return Result<KpiAppraisalDetailDto>.Failure("You are not the assigned Blasting Officer for this appraisal.");
        }

        var signatureResult = await ResolveSignatureAsync(appraisal.BlastingOfficerEmployeeId, request.SignaturePngBase64, cancellationToken);
        if (!signatureResult.Succeeded)
        {
            return Result<KpiAppraisalDetailDto>.Failure(signatureResult.Error!);
        }

        appraisal.BlastingOfficerSignedAtUtc = DateTime.UtcNow;
        appraisal.BlastingOfficerSignatureImageData = signatureResult.Value;
        appraisal.Status = KpiAppraisalStatus.PendingBlastingEngineerSignOff;
        await dbContext.SaveChangesAsync(cancellationToken);

        return await BuildDetailResultAsync(appraisal, cancellationToken);
    }

    public async Task<Result<KpiAppraisalDetailDto>> SignAsBlastingEngineerAsync(
        Guid appraisalId, Guid signerId, SignKpiAppraisalRequest request, CancellationToken cancellationToken = default)
    {
        var appraisal = await LoadSingleAppraisalAsync(appraisalId, cancellationToken);
        if (appraisal is null)
        {
            return Result<KpiAppraisalDetailDto>.Failure("Appraisal not found.");
        }

        if (appraisal.Status != KpiAppraisalStatus.PendingBlastingEngineerSignOff)
        {
            return Result<KpiAppraisalDetailDto>.Failure("This appraisal is not awaiting Blasting Engineer sign-off.");
        }

        var signer = await dbContext.Employees.FindAsync([signerId], cancellationToken);
        if (signer is null)
        {
            return Result<KpiAppraisalDetailDto>.Failure("Employee profile not found.");
        }

        if (appraisal.BlastingEngineerEmployeeId != signerId && signer.Role is not (EmployeeRole.HR or EmployeeRole.Executive))
        {
            return Result<KpiAppraisalDetailDto>.Failure("You are not the assigned Blasting Engineer for this appraisal.");
        }

        var signatureResult = await ResolveSignatureAsync(appraisal.BlastingEngineerEmployeeId, request.SignaturePngBase64, cancellationToken);
        if (!signatureResult.Succeeded)
        {
            return Result<KpiAppraisalDetailDto>.Failure(signatureResult.Error!);
        }

        appraisal.BlastingEngineerSignedAtUtc = DateTime.UtcNow;
        appraisal.BlastingEngineerSignatureImageData = signatureResult.Value;
        appraisal.Status = KpiAppraisalStatus.Finalized;
        appraisal.FinalizedAtUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return await BuildDetailResultAsync(appraisal, cancellationToken);
    }

    public async Task<Result<byte[]>> GenerateAppraisalPdfAsync(
        Guid appraisalId, Guid requesterId, CancellationToken cancellationToken = default)
    {
        var appraisal = await LoadSingleAppraisalAsync(appraisalId, cancellationToken);
        if (appraisal is null)
        {
            return Result<byte[]>.Failure("Appraisal not found.");
        }

        var requester = await dbContext.Employees.FindAsync([requesterId], cancellationToken);
        if (requester is null)
        {
            return Result<byte[]>.Failure("Employee profile not found.");
        }

        if (!CanView(appraisal, requester))
        {
            return Result<byte[]>.Failure("You are not authorized to download this document.");
        }

        var names = await GetEmployeeNamesAsync(
            [appraisal.BlastingOfficerEmployeeId, appraisal.BlastingEngineerEmployeeId], cancellationToken);

        var pdfBytes = BuildAppraisalPdf(
            appraisal,
            names.GetValueOrDefault(appraisal.BlastingOfficerEmployeeId, "—"),
            names.GetValueOrDefault(appraisal.BlastingEngineerEmployeeId, "—"));

        return Result<byte[]>.Success(pdfBytes);
    }

    private static void PopulateCategories(KpiTemplate template, CreateKpiTemplateRequest request)
    {
        var categoryOrder = 0;
        foreach (var categoryRequest in request.Categories)
        {
            var category = new KpiTemplateCategory
            {
                Id = Guid.NewGuid(),
                KpiTemplateId = template.Id,
                Name = categoryRequest.Name,
                DisplayOrder = categoryOrder++,
            };

            var itemOrder = 0;
            foreach (var itemRequest in categoryRequest.Items)
            {
                category.Items.Add(new KpiTemplateItem
                {
                    Id = Guid.NewGuid(),
                    KpiTemplateCategoryId = category.Id,
                    Description = itemRequest.Description,
                    SubGroupLabel = itemRequest.SubGroupLabel,
                    DisplayOrder = itemOrder++,
                });
            }

            template.Categories.Add(category);
        }
    }

    private async Task<List<KpiAppraisal>> LoadAppraisalsAsync(
        System.Linq.Expressions.Expression<Func<KpiAppraisal, bool>> predicate, CancellationToken cancellationToken)
    {
        return await dbContext.KpiAppraisals
            .Include(a => a.Employee)
            .Include(a => a.KpiTemplate)
            .Include(a => a.Items)
            .Where(predicate)
            .OrderByDescending(a => a.CreatedAtUtc)
            .ToListAsync(cancellationToken);
    }

    private async Task<KpiAppraisal?> LoadSingleAppraisalAsync(Guid appraisalId, CancellationToken cancellationToken)
    {
        return await dbContext.KpiAppraisals
            .Include(a => a.Employee)
            .Include(a => a.KpiTemplate)
            .Include(a => a.Items)
            .FirstOrDefaultAsync(a => a.Id == appraisalId, cancellationToken);
    }

    private async Task<Result<KpiAppraisalDetailDto>> BuildDetailResultAsync(KpiAppraisal appraisal, CancellationToken cancellationToken)
    {
        var names = await GetEmployeeNamesAsync(
            [appraisal.BlastingOfficerEmployeeId, appraisal.BlastingEngineerEmployeeId], cancellationToken);

        return Result<KpiAppraisalDetailDto>.Success(ToDetailDto(
            appraisal, appraisal.Employee!.FullName, appraisal.Employee.EmployeeNumber, appraisal.KpiTemplate!.Designation,
            names.GetValueOrDefault(appraisal.BlastingOfficerEmployeeId, "—"),
            names.GetValueOrDefault(appraisal.BlastingEngineerEmployeeId, "—")));
    }

    private async Task<List<KpiAppraisalSummaryDto>> ToSummaryDtosAsync(List<KpiAppraisal> appraisals, CancellationToken cancellationToken)
    {
        var signerIds = appraisals
            .SelectMany(a => new[] { a.BlastingOfficerEmployeeId, a.BlastingEngineerEmployeeId })
            .Distinct()
            .ToList();
        var names = await GetEmployeeNamesAsync(signerIds, cancellationToken);

        return appraisals.Select(a =>
        {
            var (overall, categories) = ComputeRollup(a.Items);

            var signedOffBy = new List<string>();
            if (a.BlastingOfficerSignedAtUtc is not null)
            {
                signedOffBy.Add($"{names.GetValueOrDefault(a.BlastingOfficerEmployeeId, "Blasting Officer")} (Blasting Officer)");
            }
            if (a.BlastingEngineerSignedAtUtc is not null)
            {
                signedOffBy.Add($"{names.GetValueOrDefault(a.BlastingEngineerEmployeeId, "Blasting Engineer")} (Blasting Engineer)");
            }

            return new KpiAppraisalSummaryDto(
                a.Id, a.EmployeeId, a.Employee!.FullName, a.Employee.EmployeeNumber, a.KpiTemplate!.Designation,
                a.PeriodLabel, a.Status.ToString(), overall, categories, a.LastScoredAtUtc, signedOffBy, a.CreatedAtUtc);
        }).ToList();
    }

    private static KpiAppraisalDetailDto ToDetailDto(
        KpiAppraisal appraisal, string employeeName, string employeeNumber, string designation,
        string blastingOfficerName, string blastingEngineerName)
    {
        var (overall, categories) = ComputeRollup(appraisal.Items);

        return new KpiAppraisalDetailDto(
            appraisal.Id, appraisal.EmployeeId, employeeName, employeeNumber, designation,
            appraisal.PeriodLabel, appraisal.InductionNumber, appraisal.Status.ToString(),
            appraisal.Checkpoint1Date, appraisal.Checkpoint2Date, appraisal.Checkpoint3Date, appraisal.Checkpoint4Date,
            appraisal.BlastingOfficerEmployeeId, blastingOfficerName, appraisal.BlastingOfficerSignedAtUtc,
            appraisal.BlastingEngineerEmployeeId, blastingEngineerName, appraisal.BlastingEngineerSignedAtUtc,
            appraisal.CreatedAtUtc, appraisal.FinalizedAtUtc, overall, categories,
            appraisal.Items.OrderBy(i => i.DisplayOrder).Select(ToItemDto).ToList());
    }

    private static KpiAppraisalItemDto ToItemDto(KpiAppraisalItem item) => new(
        item.Id, item.CategoryNameSnapshot, item.SubGroupLabelSnapshot, item.DescriptionSnapshot, item.DisplayOrder,
        item.InPlace, item.Ability,
        item.Checkpoint1Score, item.Checkpoint1Comment,
        item.Checkpoint2Score, item.Checkpoint2Comment,
        item.Checkpoint3Score, item.Checkpoint3Comment,
        item.Checkpoint4Score, item.Checkpoint4Comment,
        item.Evaluation);

    private static KpiTemplateDetailDto ToTemplateDetailDto(KpiTemplate template) => new(
        template.Id, template.Designation, template.IsActive, template.CreatedAtUtc,
        template.Categories
            .OrderBy(c => c.DisplayOrder)
            .Select(c => new KpiTemplateCategoryDto(
                c.Id, c.Name, c.DisplayOrder,
                c.Items.OrderBy(i => i.DisplayOrder)
                    .Select(i => new KpiTemplateItemDto(i.Id, i.Description, i.SubGroupLabel, i.DisplayOrder))
                    .ToList()))
            .ToList());

    // Uses the latest checkpoint with a recorded score (checkpoint 4 first) so HR/employees see
    // current standing mid-cycle rather than waiting for all four quarterly checkpoints.
    private static double? ItemScorePercent(KpiAppraisalItem item)
    {
        var latest = item.Checkpoint4Score ?? item.Checkpoint3Score ?? item.Checkpoint2Score ?? item.Checkpoint1Score;
        return latest is null ? null : (latest.Value - 1) / 2.0 * 100.0;
    }

    private static (double? Overall, List<KpiAppraisalCategoryRollupDto> Categories) ComputeRollup(
        IReadOnlyList<KpiAppraisalItem> items)
    {
        var categories = items
            .GroupBy(i => i.CategoryNameSnapshot)
            .OrderBy(g => g.Min(i => i.DisplayOrder))
            .Select(g =>
            {
                var percents = g.Select(ItemScorePercent).Where(p => p.HasValue).Select(p => p!.Value).ToList();
                var scorePercent = percents.Count == 0 ? (double?)null : Math.Round(percents.Average(), 1);
                return new KpiAppraisalCategoryRollupDto(g.Key, scorePercent, g.Count());
            })
            .ToList();

        var allPercents = items.Select(ItemScorePercent).Where(p => p.HasValue).Select(p => p!.Value).ToList();
        var overall = allPercents.Count == 0 ? (double?)null : Math.Round(allPercents.Average(), 1);

        return (overall, categories);
    }

    private static bool CanView(KpiAppraisal appraisal, Employee requester)
    {
        if (appraisal.EmployeeId == requester.Id) return true;
        if (appraisal.Employee!.ManagerId == requester.Id) return true;
        if (appraisal.CreatedByEmployeeId == requester.Id) return true;
        if (appraisal.BlastingOfficerEmployeeId == requester.Id) return true;
        if (appraisal.BlastingEngineerEmployeeId == requester.Id) return true;
        return requester.Role is EmployeeRole.HR or EmployeeRole.Executive;
    }

    private static bool CanScore(KpiAppraisal appraisal, Employee submitter)
    {
        if (appraisal.Employee!.ManagerId == submitter.Id) return true;
        return submitter.Role is EmployeeRole.HR or EmployeeRole.Executive;
    }

    private async Task<Dictionary<Guid, string>> GetEmployeeNamesAsync(IEnumerable<Guid> employeeIds, CancellationToken cancellationToken)
    {
        var ids = employeeIds.Distinct().ToList();
        var employees = await dbContext.Employees
            .Where(e => ids.Contains(e.Id))
            .Select(e => new { e.Id, e.FirstName, e.LastName })
            .ToListAsync(cancellationToken);

        return employees.ToDictionary(e => e.Id, e => $"{e.FirstName} {e.LastName}".Trim());
    }

    private async Task<Result<byte[]>> ResolveSignatureAsync(
        Guid targetEmployeeId, string? signaturePngBase64, CancellationToken cancellationToken)
    {
        var target = await dbContext.Employees.FindAsync([targetEmployeeId], cancellationToken);
        if (target is null)
        {
            return Result<byte[]>.Failure("Employee profile not found.");
        }

        if (target.SignatureImageData is not null)
        {
            return Result<byte[]>.Success(target.SignatureImageData);
        }

        if (string.IsNullOrWhiteSpace(signaturePngBase64))
        {
            return Result<byte[]>.Failure("Please sign to complete this sign-off.");
        }

        byte[] signatureBytes;
        try
        {
            signatureBytes = SignatureImageCodec.Decode(signaturePngBase64);
        }
        catch (FormatException)
        {
            return Result<byte[]>.Failure("The signature image could not be read.");
        }

        target.SignatureImageData = signatureBytes;
        target.SignatureUpdatedAtUtc = DateTime.UtcNow;

        return Result<byte[]>.Success(signatureBytes);
    }

    private static byte[] BuildAppraisalPdf(KpiAppraisal appraisal, string blastingOfficerName, string blastingEngineerName)
    {
        var (overall, categories) = ComputeRollup(appraisal.Items);
        var itemsByCategory = appraisal.Items
            .OrderBy(i => i.DisplayOrder)
            .GroupBy(i => i.CategoryNameSnapshot)
            .OrderBy(g => g.Min(i => i.DisplayOrder))
            .ToList();

        return Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4.Landscape());
                page.Margin(24);
                page.DefaultTextStyle(x => x.FontSize(8));

                page.Header().Column(column =>
                {
                    column.Item().Element(header => PdfBranding.RenderLetterhead(header, "KPI Appraisal — Key Performance Coordinator"));
                    column.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Text($"Name: {appraisal.Employee!.FullName}").FontSize(10);
                        row.RelativeItem().Text($"Indu No: {appraisal.InductionNumber ?? "—"}").FontSize(10);
                        row.RelativeItem().Text($"Designation: {appraisal.KpiTemplate!.Designation}").FontSize(10);
                        row.RelativeItem().Text($"Period: {appraisal.PeriodLabel}").FontSize(10);
                    });
                    column.Item().Text("Key: 3 = Progress meets or exceeds target   2 = Progress does not meet target   1 = No progress")
                        .FontSize(8).FontColor(Colors.Grey.Darken1);
                    column.Item().Text($"Overall score: {(overall.HasValue ? $"{overall}%" : "—")}").FontSize(10).Bold();
                });

                page.Content().PaddingTop(10).Column(column =>
                {
                    column.Spacing(10);

                    foreach (var group in itemsByCategory)
                    {
                        column.Item().Column(categoryColumn =>
                        {
                            var categoryPercent = categories.FirstOrDefault(c => c.Name == group.Key)?.ScorePercent;
                            categoryColumn.Item().Text($"{group.Key}{(categoryPercent.HasValue ? $" — {categoryPercent}%" : "")}")
                                .FontSize(11).Bold();

                            categoryColumn.Item().Table(table =>
                            {
                                table.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn(4);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(1);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                    columns.RelativeColumn(2);
                                });

                                table.Header(header =>
                                {
                                    header.Cell().Text("Desired result and measures").Bold();
                                    header.Cell().Text("In place").Bold();
                                    header.Cell().Text("Ability").Bold();
                                    header.Cell().Text(appraisal.Checkpoint1Date is { } d1 ? $"Review 1 ({d1:d MMM})" : "Review 1").Bold();
                                    header.Cell().Text(appraisal.Checkpoint2Date is { } d2 ? $"Review 2 ({d2:d MMM})" : "Review 2").Bold();
                                    header.Cell().Text(appraisal.Checkpoint3Date is { } d3 ? $"Review 3 ({d3:d MMM})" : "Review 3").Bold();
                                    header.Cell().Text(appraisal.Checkpoint4Date is { } d4 ? $"90-day eval ({d4:d MMM})" : "90-day eval").Bold();
                                    header.Cell().Text("Evaluation").Bold();
                                });

                                string? previousSubGroup = null;
                                foreach (var item in group)
                                {
                                    if (item.SubGroupLabelSnapshot != previousSubGroup)
                                    {
                                        if (item.SubGroupLabelSnapshot is not null)
                                        {
                                            table.Cell().ColumnSpan(8).PaddingTop(3).Text(item.SubGroupLabelSnapshot).Italic().FontSize(8);
                                        }
                                        previousSubGroup = item.SubGroupLabelSnapshot;
                                    }

                                    table.Cell().Text(item.DescriptionSnapshot);
                                    table.Cell().Text(item.InPlace is null ? "—" : item.InPlace.Value ? "Y" : "N");
                                    table.Cell().Text(item.Ability is null ? "—" : item.Ability.Value ? "Y" : "N");
                                    CheckpointCell(table, item.Checkpoint1Score, item.Checkpoint1Comment);
                                    CheckpointCell(table, item.Checkpoint2Score, item.Checkpoint2Comment);
                                    CheckpointCell(table, item.Checkpoint3Score, item.Checkpoint3Comment);
                                    CheckpointCell(table, item.Checkpoint4Score, item.Checkpoint4Comment);
                                    table.Cell().Text(item.Evaluation ?? "");
                                }
                            });
                        });
                    }

                    column.Item().PaddingTop(10).Row(row =>
                    {
                        row.RelativeItem().Column(c => SignatureBlock(
                            c, "Signed (Blasting Officer)", blastingOfficerName,
                            appraisal.BlastingOfficerSignatureImageData, appraisal.BlastingOfficerSignedAtUtc));
                        row.ConstantItem(20);
                        row.RelativeItem().Column(c => SignatureBlock(
                            c, "Signed (Blasting Engineer)", blastingEngineerName,
                            appraisal.BlastingEngineerSignatureImageData, appraisal.BlastingEngineerSignedAtUtc));
                    });
                });

                page.Footer().AlignCenter().Text(
                    $"Generated {DateTime.UtcNow:d MMM yyyy HH:mm} UTC").FontSize(8).FontColor(Colors.Grey.Medium);
            });
        }).GeneratePdf();
    }

    private static void CheckpointCell(TableDescriptor table, int? score, string? comment)
    {
        table.Cell().Column(cell =>
        {
            cell.Item().Text(score.HasValue ? $"{score}/3" : "—").Bold();
            if (!string.IsNullOrWhiteSpace(comment))
            {
                cell.Item().Text(comment).FontSize(7).FontColor(Colors.Grey.Darken1);
            }
        });
    }

    private static void SignatureBlock(
        ColumnDescriptor column, string title, string name, byte[]? signature, DateTime? signedAtUtc)
    {
        column.Item().Text(title).Bold().FontSize(10);

        if (signature is not null)
        {
            column.Item().Height(50).Image(signature).FitArea();
        }
        else
        {
            column.Item().Height(50);
        }

        column.Item().LineHorizontal(0.75f).LineColor(Colors.Grey.Lighten1);
        column.Item().Text(name).FontSize(9);
        column.Item().Text(signedAtUtc is null ? "Not yet signed" : $"{signedAtUtc:d MMM yyyy HH:mm} UTC")
            .FontSize(8).FontColor(Colors.Grey.Darken1);
    }
}
