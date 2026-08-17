using FluentValidation;

namespace GoldFieldsHR.Application.WorkShift;

public class CreateScheduleDocumentRequestValidator : AbstractValidator<CreateScheduleDocumentRequest>
{
    public CreateScheduleDocumentRequestValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(200);
    }
}
