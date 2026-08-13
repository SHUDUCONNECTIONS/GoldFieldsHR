using FluentValidation;
using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Employees;

public class SetEmployeeManagerRequestValidator : AbstractValidator<SetEmployeeManagerRequest>
{
    public SetEmployeeManagerRequestValidator()
    {
        // ManagerEmployeeNumber is intentionally optional — null/empty means "clear the manager".
        RuleFor(x => x.ManagerEmployeeNumber).MaximumLength(50)
            .Matches(ValidationPatterns.EmployeeNumber).When(x => !string.IsNullOrEmpty(x.ManagerEmployeeNumber))
            .WithMessage("Employee number can only contain letters, numbers, and hyphens.");
    }
}

public class SetEmployeeRoleRequestValidator : AbstractValidator<SetEmployeeRoleRequest>
{
    public SetEmployeeRoleRequestValidator()
    {
        RuleFor(x => x.Role).IsInEnum();
    }
}
