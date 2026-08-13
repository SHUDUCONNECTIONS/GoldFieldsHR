using FluentValidation;
using GoldFieldsHR.Application.Common;

namespace GoldFieldsHR.Application.Auth;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty().MinimumLength(8).MaximumLength(128);
        RuleFor(x => x.FirstName).NotEmpty().MaximumLength(100)
            .Matches(ValidationPatterns.PersonName).WithMessage("First name can only contain letters.");
        RuleFor(x => x.LastName).NotEmpty().MaximumLength(100)
            .Matches(ValidationPatterns.PersonName).WithMessage("Last name can only contain letters.");
        RuleFor(x => x.EmployeeNumber).NotEmpty().MaximumLength(50)
            .Matches(ValidationPatterns.EmployeeNumber).WithMessage("Employee number can only contain letters, numbers, and hyphens.");
        RuleFor(x => x.JobTitle).NotEmpty().MaximumLength(150);
        RuleFor(x => x.SiteId).NotEqual(Guid.Empty);
        RuleFor(x => x.ManagerEmployeeNumber).MaximumLength(50)
            .Matches(ValidationPatterns.EmployeeNumber).When(x => !string.IsNullOrEmpty(x.ManagerEmployeeNumber))
            .WithMessage("Manager's employee number can only contain letters, numbers, and hyphens.");
        RuleFor(x => x.RequestedRole).IsInEnum().When(x => x.RequestedRole.HasValue);
    }
}

public class LoginRequestValidator : AbstractValidator<LoginRequest>
{
    public LoginRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Password).NotEmpty();
    }
}

public class ForgotPasswordRequestValidator : AbstractValidator<ForgotPasswordRequest>
{
    public ForgotPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
    }
}

public class ResetPasswordRequestValidator : AbstractValidator<ResetPasswordRequest>
{
    public ResetPasswordRequestValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress().MaximumLength(256);
        RuleFor(x => x.Token).NotEmpty();
        RuleFor(x => x.NewPassword).NotEmpty().MinimumLength(8).MaximumLength(128);
    }
}

public class RefreshTokenRequestValidator : AbstractValidator<RefreshTokenRequest>
{
    public RefreshTokenRequestValidator()
    {
        RuleFor(x => x.RefreshToken).NotEmpty();
    }
}
