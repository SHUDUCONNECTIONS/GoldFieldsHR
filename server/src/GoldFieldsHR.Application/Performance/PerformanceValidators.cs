using FluentValidation;
using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Performance;

public class CreatePerformanceReviewRequestValidator : AbstractValidator<CreatePerformanceReviewRequest>
{
    public CreatePerformanceReviewRequestValidator()
    {
        RuleFor(x => x.EmployeeNumber).NotEmpty().MaximumLength(50)
            .Matches(ValidationPatterns.EmployeeNumber).WithMessage("Employee number can only contain letters, numbers, and hyphens.");
        RuleFor(x => x.PeriodLabel).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Score).InclusiveBetween(1, 5);
        RuleFor(x => x.Comments).MaximumLength(2000);
    }
}
