using FluentValidation;

namespace GoldFieldsHR.Application.Employees;

public class SetEmployeeManagerRequestValidator : AbstractValidator<SetEmployeeManagerRequest>
{
    public SetEmployeeManagerRequestValidator()
    {
        // ManagerEmployeeNumber is intentionally optional — null/empty means "clear the manager".
        RuleFor(x => x.ManagerEmployeeNumber).MaximumLength(50);
    }
}

public class SetEmployeeRoleRequestValidator : AbstractValidator<SetEmployeeRoleRequest>
{
    public SetEmployeeRoleRequestValidator()
    {
        RuleFor(x => x.Role).IsInEnum();
    }
}
