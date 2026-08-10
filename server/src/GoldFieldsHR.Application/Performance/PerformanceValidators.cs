using FluentValidation;

namespace GoldFieldsHR.Application.Performance;

public class CreatePerformanceReviewRequestValidator : AbstractValidator<CreatePerformanceReviewRequest>
{
    public CreatePerformanceReviewRequestValidator()
    {
        RuleFor(x => x.EmployeeNumber).NotEmpty().MaximumLength(50);
        RuleFor(x => x.PeriodLabel).NotEmpty().MaximumLength(100);
        RuleFor(x => x.Score).InclusiveBetween(1, 5);
        RuleFor(x => x.Comments).MaximumLength(2000);
    }
}
