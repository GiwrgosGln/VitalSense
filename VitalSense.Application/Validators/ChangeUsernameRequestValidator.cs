using FluentValidation;
using VitalSense.Application.DTOs;

namespace VitalSense.Application.Validators;

public class ChangeUsernameRequestValidator : AbstractValidator<ChangeUsernameRequest>
{
    public ChangeUsernameRequestValidator()
    {
        RuleFor(x => x.NewUsername)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(6).WithMessage("Username must be at least 6 characters long.")
            .Matches("^[a-zA-Z0-9_]+$").WithMessage("Username can only contain letters, numbers, and underscores.");
            
        RuleFor(x => x.CurrentPassword)
            .NotEmpty().WithMessage("Current password is required.");
    }
}
