namespace tracking_management_api.Infrastructure.Messaging;

public sealed class SafetyIncidentPublishException : Exception
{
    public SafetyIncidentPublishException(string message, Exception innerException)
        : base(message, innerException)
    {
    }
}
