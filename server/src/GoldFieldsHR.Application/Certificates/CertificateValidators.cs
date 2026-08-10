using FluentValidation;

namespace GoldFieldsHR.Application.Certificates;

public class IssueCertificateRequestValidator : AbstractValidator<IssueCertificateRequest>
{
    public IssueCertificateRequestValidator()
    {
        RuleFor(x => x.EmployeeNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Notes).MaximumLength(2000);
        RuleFor(x => x.ExpiryDate)
            .GreaterThanOrEqualTo(x => x.IssuedDate)
            .WithMessage("Expiry date must be on or after the issued date.");
    }
}
