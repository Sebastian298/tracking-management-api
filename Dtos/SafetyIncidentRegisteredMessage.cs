namespace tracking_management_api.Dtos;

public sealed record SafetyIncidentRegisteredMessage(
    Guid MessageId,
    DateTime OccurredAtUtc,
    SafetyIncidentDto Incident);
