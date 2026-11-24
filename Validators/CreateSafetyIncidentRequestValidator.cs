using FluentValidation;
using tracking_management_api.Dtos;

namespace tracking_management_api.Validators;

public sealed class CreateSafetyIncidentRequestValidator : AbstractValidator<CreateSafetyIncidentRequest>
{
    public static readonly IReadOnlyList<string> AllowedIncidentTypes =
    [
        "Accident",
        "Customer Complaint",
        "DOT Violation",
        "Equipment Damage",
        "Moving Violation"
    ];

    public CreateSafetyIncidentRequestValidator()
    {
        RuleFor(request => request.TripId)
            .NotEmpty()
            .Matches(@"^TRIP\d{8}$")
            .WithMessage("TripId must match the pattern TRIP followed by 8 digits.");

        RuleFor(request => request.TruckId)
            .NotEmpty()
            .Matches(@"^TRK\d{5}$")
            .WithMessage("TruckId must match the pattern TRK followed by 5 digits.");

        RuleFor(request => request.DriverId)
            .NotEmpty()
            .Matches(@"^DRV\d{5}$")
            .WithMessage("DriverId must match the pattern DRV followed by 5 digits.");

        RuleFor(request => request.IncidentDate)
            .Must(date => date <= DateTime.UtcNow.AddHours(12))
            .WithMessage("Incident date cannot be in the future.");

        RuleFor(request => request.IncidentType)
            .NotEmpty()
            .Must(type => AllowedIncidentTypes.Contains(type))
            .WithMessage(
                "IncidentType must be one of: Accident, Customer Complaint, DOT Violation, Equipment Damage, Moving Violation.");

        RuleFor(request => request.LocationCity)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.LocationState)
            .NotEmpty()
            .Matches(@"^[A-Z]{2}$")
            .WithMessage("LocationState must be a two-letter uppercase code.");

        RuleFor(request => request.VehicleDamageCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.CargoDamageCost)
            .GreaterThanOrEqualTo(0);

        RuleFor(request => request.Description)
            .NotEmpty()
            .MaximumLength(500);
    }
}
