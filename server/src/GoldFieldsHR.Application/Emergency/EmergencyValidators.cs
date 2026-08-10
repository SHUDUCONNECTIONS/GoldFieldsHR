using FluentValidation;

namespace GoldFieldsHR.Application.Emergency;

public class TriggerEmergencyAlertRequestValidator : AbstractValidator<TriggerEmergencyAlertRequest>
{
    public TriggerEmergencyAlertRequestValidator()
    {
        RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Message).MaximumLength(2000);
    }
}

public class ResolveEmergencyAlertRequestValidator : AbstractValidator<ResolveEmergencyAlertRequest>
{
    public ResolveEmergencyAlertRequestValidator()
    {
        RuleFor(x => x.ResolutionNotes).MaximumLength(2000);
    }
}
