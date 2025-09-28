using FluentValidation;
using VitalSense.Application.DTOs;

namespace VitalSense.Application.Validators;

public class CreateClientRequestValidator : AbstractValidator<CreateClientRequest>
{
    public CreateClientRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .MinimumLength(4).WithMessage("First Name must be at least 4 characters long.")
            .MaximumLength(50).WithMessage("First Name cannot exceed 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .MinimumLength(4).WithMessage("Last Name must be at least 4 characters long.")
            .MaximumLength(50).WithMessage("Last Name cannot exceed 50 characters.");

        RuleFor(x => x.Email)
            .EmailAddress()
            .When(x => !string.IsNullOrWhiteSpace(x.Email))
            .WithMessage("Invalid email address.");

        RuleFor(x => x.Phone)
            .MaximumLength(50);

        RuleFor(x => x.DateOfBirth)
            .LessThan(DateTime.UtcNow).When(x => x.DateOfBirth.HasValue)
            .WithMessage("Date of birth must be in the past.");

        RuleFor(x => x.Gender)
            .MaximumLength(20);

        RuleFor(x => x.Notes)
            .MaximumLength(2000);
    }
}