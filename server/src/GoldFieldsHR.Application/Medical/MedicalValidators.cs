using FluentValidation;
using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Medical;

public class RecordMedicalExaminationRequestValidator : AbstractValidator<RecordMedicalExaminationRequest>
{
    public RecordMedicalExaminationRequestValidator()
    {
        RuleFor(x => x.EmployeeNumber).NotEmpty().MaximumLength(50)
            .Matches(ValidationPatterns.EmployeeNumber).WithMessage("Employee number can only contain letters, numbers, and hyphens.");
        RuleFor(x => x.Status).IsInEnum();
        RuleFor(x => x.Restrictions).MaximumLength(2000);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.ExpiryDate)
            .GreaterThanOrEqualTo(x => x.ExamDate)
            .WithMessage("Expiry date must be on or after the exam date.");
    }
}
