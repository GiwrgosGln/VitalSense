using FluentValidation;
using VitalSense.Application.DTOs;

namespace VitalSense.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Username)
            .NotEmpty().WithMessage("Username is required.")
            .MinimumLength(6).WithMessage("Username must be at least 6 characters long.")
            .MaximumLength(50).WithMessage("Username cannot exceed 50 characters.")
            .Matches("^[a-zA-Z0-9._-]+$").WithMessage("Username can only contain letters, numbers, dots, underscores, or hyphens.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.")
            .MaximumLength(255).WithMessage("Email cannot exceed 255 characters.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(8).WithMessage("Password must be at least 8 characters long.")
            .MaximumLength(100).WithMessage("Password cannot exceed 100 characters.")
            .Must(password => password.Any(char.IsUpper)).WithMessage("Password must contain at least one uppercase letter.")
            .Must(password => password.Any(char.IsLower)).WithMessage("Password must contain at least one lowercase letter.")
            .Must(password => password.Any(char.IsDigit)).WithMessage("Password must contain at least one digit.")
            .Must(password => password.Any(c => !char.IsLetterOrDigit(c))).WithMessage("Password must contain at least one special character.");

        RuleFor(x => x.ConfirmPassword)
            .NotEmpty().WithMessage("Confirm password is required.")
            .Equal(x => x.Password).WithMessage("The password and confirmation password do not match.");
    }
}