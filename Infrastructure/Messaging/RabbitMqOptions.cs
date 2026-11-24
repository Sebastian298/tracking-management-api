namespace tracking_management_api.Infrastructure.Messaging;

public sealed class RabbitMqOptions
{
    public const string SectionName = "RabbitMq";

    public const string SetInSecretPlaceholder = "<Set_In_Secret>";

    public string HostName { get; set; } = "localhost";

    public int Port { get; set; } = 5672;

    public string UserName { get; set; } = SetInSecretPlaceholder;

    public string Password { get; set; } = SetInSecretPlaceholder;

    public string QueueName { get; set; } = "safety-incidents.registered";

    public static bool IsSecretConfigured(string? value)
    {
        return !string.IsNullOrWhiteSpace(value)
            && !string.Equals(value, SetInSecretPlaceholder, StringComparison.Ordinal);
    }
}
