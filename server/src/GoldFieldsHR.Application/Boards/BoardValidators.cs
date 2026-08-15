using FluentValidation;

namespace GoldFieldsHR.Application.Boards;

public class CreateBoardRequestValidator : AbstractValidator<CreateBoardRequest>
{
    public CreateBoardRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(1000);
        RuleFor(x => x.InitialMemberEmployeeIds).NotNull();
    }
}

public class UpdateBoardRequestValidator : AbstractValidator<UpdateBoardRequest>
{
    public UpdateBoardRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).MaximumLength(1000);
    }
}

public class AddBoardMemberRequestValidator : AbstractValidator<AddBoardMemberRequest>
{
    public AddBoardMemberRequestValidator()
    {
        RuleFor(x => x.EmployeeId).NotEmpty();
    }
}
