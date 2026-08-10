using FluentValidation;

namespace GoldFieldsHR.Application.Safety;

public class SubmitPreShiftCheckRequestValidator : AbstractValidator<SubmitPreShiftCheckRequest>
{
    public SubmitPreShiftCheckRequestValidator()
    {
        RuleFor(x => x.HazardNotes).MaximumLength(2000);
    }
}
