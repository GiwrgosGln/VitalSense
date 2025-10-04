using FluentValidation;
using VitalSense.Application.DTOs;

namespace VitalSense.Application.Validators;

public class CreateTaskRequestValidator : AbstractValidator<CreateTaskRequest>
{
    public CreateTaskRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MinimumLength(4).WithMessage("Title must be at least 4 characters long.")
            .MaximumLength(50).WithMessage("Title cannot exceed 50 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(50).WithMessage("Description cannot exceed 50 characters.");

        RuleFor(x => x.DueDate)
            .GreaterThan(DateTime.UtcNow).When(x => x.DueDate.HasValue)
            .WithMessage("Date of birth must be in the future.");
    }
}