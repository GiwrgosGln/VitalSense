using VitalSense.Application.DTOs;
using FluentValidation;

namespace VitalSense.Application.Validators;

public class CreateAppointmentRequestValidator : AbstractValidator<CreateAppointmentRequest>
{
    public CreateAppointmentRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required.")
            .MaximumLength(200).WithMessage("Title cannot exceed 200 characters.");

        RuleFor(x => x.Start)
            .NotEmpty().WithMessage("Start time is required.");

        RuleFor(x => x.End)
            .NotEmpty().WithMessage("End time is required.")
            .GreaterThan(x => x.Start).WithMessage("End must be after Start.");

        RuleFor(x => x.AllDay)
            .NotNull();

        RuleFor(x => x.ClientId)
            .NotEmpty().WithMessage("ClientId is required.");
    }
}
