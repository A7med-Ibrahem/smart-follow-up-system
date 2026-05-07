using FluentValidation;
using SmartFollowUp.API.DTOs;

namespace SmartFollowUp.API.Validators
{
    public class CreateCaseRequestValidator : AbstractValidator<CreateCaseRequestDto>
    {
        public CreateCaseRequestValidator()
        {
            RuleFor(x => x.PatientName)
                .NotEmpty().WithMessage("Patient name is required")
                .MinimumLength(3).WithMessage("Patient name must be at least 3 characters");

            RuleFor(x => x.PatientEmail)
                .NotEmpty().WithMessage("Patient email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.PatientPhone)
                .NotEmpty().WithMessage("Patient phone is required");

            RuleFor(x => x.OperationType)
                .NotEmpty().WithMessage("Operation type is required");

            RuleFor(x => x.OperationDate)
                .NotEmpty().WithMessage("Operation date is required")
                .LessThanOrEqualTo(DateTime.UtcNow.AddDays(1))
                .WithMessage("Operation date cannot be in the future");
        }
    }
}