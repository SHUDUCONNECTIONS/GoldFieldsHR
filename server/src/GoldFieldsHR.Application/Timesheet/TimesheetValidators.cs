using FluentValidation;

namespace GoldFieldsHR.Application.Timesheet;

public class SubmitTimesheetCorrectionRequestValidator : AbstractValidator<SubmitTimesheetCorrectionRequest>
{
    public SubmitTimesheetCorrectionRequestValidator()
    {
        RuleFor(x => x.TimesheetEntryId).NotEqual(Guid.Empty);
        RuleFor(x => x.Reason).NotEmpty().MaximumLength(1000);
        RuleFor(x => x.RequestedClockOutUtc)
            .GreaterThan(x => x.RequestedClockInUtc!.Value)
            .When(x => x.RequestedClockInUtc.HasValue && x.RequestedClockOutUtc.HasValue)
            .WithMessage("Clock-out time must be after clock-in time.");
    }
}

public class ReviewTimesheetCorrectionRequestValidator : AbstractValidator<ReviewTimesheetCorrectionRequest>
{
    public ReviewTimesheetCorrectionRequestValidator()
    {
        RuleFor(x => x.RejectionReason).MaximumLength(1000);
    }
}
