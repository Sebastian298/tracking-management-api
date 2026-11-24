namespace tracking_management_api.Options.SafetyIncidents;

public sealed class SafetyIncidentsOptions
{
    public const string SectionName = "SafetyIncidents";

    public string FilePath { get; set; } = "data/safety_incidents.csv";

    public int ProcessingDelayMs { get; set; } = 8000;
}
