using FluentValidation;

namespace GoldFieldsHR.Application.Leave;

public class SubmitLeaveRequestValidator : AbstractValidator<SubmitLeaveRequest>
{
    public SubmitLeaveRequestValidator()
    {
        RuleFor(x => x.LeaveType).IsInEnum();
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.EndDate)
            .GreaterThanOrEqualTo(x => x.StartDate)
            .WithMessage("End date must be on or after the start date.");
    }
}

public class ReviewLeaveRequestValidator : AbstractValidator<ReviewLeaveRequest>
{
    public ReviewLeaveRequestValidator()
    {
        RuleFor(x => x.RejectionReason).MaximumLength(1000);
    }
}
