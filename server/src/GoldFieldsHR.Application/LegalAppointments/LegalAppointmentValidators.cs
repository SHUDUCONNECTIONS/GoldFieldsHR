using FluentValidation;

namespace GoldFieldsHR.Application.LegalAppointments;

public class SubmitLegalAppointmentRequestValidator : AbstractValidator<SubmitLegalAppointmentRequest>
{
    public SubmitLegalAppointmentRequestValidator()
    {
        RuleFor(x => x.AppointmentType).IsInEnum();
        RuleFor(x => x.AppointedBy).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(4000);
        RuleFor(x => x.ValidTo)
            .GreaterThanOrEqualTo(x => x.ValidFrom)
            .WithMessage("Valid-to date must be on or after the valid-from date.");
    }
}

public class ReviewLegalAppointmentRequestValidator : AbstractValidator<ReviewLegalAppointmentRequest>
{
    public ReviewLegalAppointmentRequestValidator()
    {
        RuleFor(x => x.RejectionReason).MaximumLength(1000);
    }
}

public class RevokeLegalAppointmentRequestValidator : AbstractValidator<RevokeLegalAppointmentRequest>
{
    public RevokeLegalAppointmentRequestValidator()
    {
        RuleFor(x => x.RevokedNotes).MaximumLength(2000);
    }
}
