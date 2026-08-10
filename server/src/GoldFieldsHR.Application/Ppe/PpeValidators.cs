using FluentValidation;

namespace GoldFieldsHR.Application.Ppe;

public class SubmitPpeRequestValidator : AbstractValidator<SubmitPpeRequest>
{
    public SubmitPpeRequestValidator()
    {
        RuleFor(x => x.ItemType).IsInEnum();
        RuleFor(x => x.Size).MaximumLength(50);
        RuleFor(x => x.Quantity).GreaterThan(0);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
    }
}

public class ReviewPpeRequestValidator : AbstractValidator<ReviewPpeRequest>
{
    public ReviewPpeRequestValidator()
    {
        RuleFor(x => x.RejectionReason).MaximumLength(1000);
    }
}
