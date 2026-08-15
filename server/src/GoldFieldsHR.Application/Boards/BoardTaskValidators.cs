using FluentValidation;

namespace GoldFieldsHR.Application.Boards;

public class CreateBoardTaskRequestValidator : AbstractValidator<CreateBoardTaskRequest>
{
    public CreateBoardTaskRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public class UpdateBoardTaskRequestValidator : AbstractValidator<UpdateBoardTaskRequest>
{
    public UpdateBoardTaskRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(2000);
    }
}

public class ChangeTaskStatusRequestValidator : AbstractValidator<ChangeTaskStatusRequest>
{
    public ChangeTaskStatusRequestValidator()
    {
        RuleFor(x => x.Status).IsInEnum();
    }
}
