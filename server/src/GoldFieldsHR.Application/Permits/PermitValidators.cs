using FluentValidation;

namespace GoldFieldsHR.Application.Permits;

public class SubmitPermitRequestValidator : AbstractValidator<SubmitPermitRequest>
{
    public SubmitPermitRequestValidator()
    {
        RuleFor(x => x.PermitType).IsInEnum();
        RuleFor(x => x.Location).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ValidTo)
            .GreaterThanOrEqualTo(x => x.ValidFrom)
            .WithMessage("Valid-to date must be on or after the valid-from date.");
    }
}

public class ReviewPermitRequestValidator : AbstractValidator<ReviewPermitRequest>
{
    public ReviewPermitRequestValidator()
    {
        RuleFor(x => x.RejectionReason).MaximumLength(1000);
    }
}

public class ClosePermitRequestValidator : AbstractValidator<ClosePermitRequest>
{
    public ClosePermitRequestValidator()
    {
        RuleFor(x => x.ClosedNotes).MaximumLength(2000);
    }
}
