using FluentValidation;

namespace GoldFieldsHR.Application.Policies;

public class CreatePolicyRequestValidator : AbstractValidator<CreatePolicyRequest>
{
    public CreatePolicyRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Content).NotEmpty().MaximumLength(20000);
    }
}

public class AcknowledgePolicyRequestValidator : AbstractValidator<AcknowledgePolicyRequest>
{
    public AcknowledgePolicyRequestValidator()
    {
        RuleFor(x => x.SignaturePngBase64).MaximumLength(2_000_000);
    }
}
