using FluentValidation;
using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Kpi;

public class CreateKpiTemplateItemRequestValidator : AbstractValidator<CreateKpiTemplateItemRequest>
{
    public CreateKpiTemplateItemRequestValidator()
    {
        RuleFor(x => x.Description).NotEmpty().MaximumLength(500);
        RuleFor(x => x.SubGroupLabel).MaximumLength(100);
    }
}

public class CreateKpiTemplateCategoryRequestValidator : AbstractValidator<CreateKpiTemplateCategoryRequest>
{
    public CreateKpiTemplateCategoryRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new CreateKpiTemplateItemRequestValidator());
    }
}

public class CreateKpiTemplateRequestValidator : AbstractValidator<CreateKpiTemplateRequest>
{
    public CreateKpiTemplateRequestValidator()
    {
        RuleFor(x => x.Designation).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Categories).NotEmpty();
        RuleForEach(x => x.Categories).SetValidator(new CreateKpiTemplateCategoryRequestValidator());
    }
}

public class CreateKpiAppraisalRequestValidator : AbstractValidator<CreateKpiAppraisalRequest>
{
    public CreateKpiAppraisalRequestValidator()
    {
        RuleFor(x => x.EmployeeNumber).NotEmpty().MaximumLength(50)
            .Matches(ValidationPatterns.EmployeeNumber).WithMessage("Employee number can only contain letters, numbers, and hyphens.");
        RuleFor(x => x.KpiTemplateId).NotEmpty();
        RuleFor(x => x.PeriodLabel).NotEmpty().MaximumLength(50);
        RuleFor(x => x.InductionNumber).MaximumLength(50);
        RuleFor(x => x.BlastingOfficerEmployeeNumber).NotEmpty().MaximumLength(50)
            .Matches(ValidationPatterns.EmployeeNumber).WithMessage("Employee number can only contain letters, numbers, and hyphens.");
        RuleFor(x => x.BlastingEngineerEmployeeNumber).NotEmpty().MaximumLength(50)
            .Matches(ValidationPatterns.EmployeeNumber).WithMessage("Employee number can only contain letters, numbers, and hyphens.");
    }
}

public class KpiItemScoreEntryValidator : AbstractValidator<KpiItemScoreEntry>
{
    public KpiItemScoreEntryValidator()
    {
        RuleFor(x => x.ItemId).NotEmpty();
        RuleFor(x => x.Score).InclusiveBetween(1, 3);
        RuleFor(x => x.Comment).MaximumLength(1000);
    }
}

public class SubmitCheckpointScoresRequestValidator : AbstractValidator<SubmitCheckpointScoresRequest>
{
    public SubmitCheckpointScoresRequestValidator()
    {
        RuleFor(x => x.CheckpointNumber).InclusiveBetween(1, 4);
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).SetValidator(new KpiItemScoreEntryValidator());
    }
}

public class SetItemFlagsRequestValidator : AbstractValidator<SetItemFlagsRequest>
{
    public SetItemFlagsRequestValidator()
    {
        RuleFor(x => x.Items).NotEmpty();
        RuleForEach(x => x.Items).ChildRules(item => item.RuleFor(x => x.ItemId).NotEmpty());
    }
}
