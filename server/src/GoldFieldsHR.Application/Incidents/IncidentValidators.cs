using FluentValidation;

namespace GoldFieldsHR.Application.Incidents;

public class SubmitIncidentReportRequestValidator : AbstractValidator<SubmitIncidentReportRequest>
{
    public SubmitIncidentReportRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.Severity).IsInEnum();
        RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
        RuleFor(x => x.OccurredAtUtc)
            .NotEqual(default(DateTime))
            .LessThanOrEqualTo(_ => DateTime.UtcNow.AddMinutes(5))
            .WithMessage("Occurred at cannot be in the future.");
    }
}

public class UpdateIncidentStatusRequestValidator : AbstractValidator<UpdateIncidentStatusRequest>
{
    public UpdateIncidentStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.ReviewNotes).MaximumLength(4000);
    }
}
