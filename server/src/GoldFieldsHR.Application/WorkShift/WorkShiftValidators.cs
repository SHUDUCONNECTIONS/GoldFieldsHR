using FluentValidation;

namespace GoldFieldsHR.Application.WorkShift;

public class SubmitShiftChangeRequestValidator : AbstractValidator<SubmitShiftChangeRequest>
{
    public SubmitShiftChangeRequestValidator()
    {
        RuleFor(x => x.RequestedShiftType).IsInEnum();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.Comments).MaximumLength(1000);
    }
}

public class ReviewShiftChangeRequestValidator : AbstractValidator<ReviewShiftChangeRequest>
{
    public ReviewShiftChangeRequestValidator()
    {
        RuleFor(x => x.RejectionReason).MaximumLength(1000);
    }
}
