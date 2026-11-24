namespace tracking_management_api.Options.Loads;

public sealed class LoadsDataOptions
{
    public const string SectionName = "LoadsData";

    public string FilePath { get; set; } = "data/loads.csv";

    public int MaxItems { get; set; } = 10_000;
}
