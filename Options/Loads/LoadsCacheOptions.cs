namespace tracking_management_api.Options.Loads;

public sealed class LoadsCacheOptions
{
    public const string SectionName = "LoadsCache";
    public const string AllLoadsCacheKey = "loads:all";

    public int AbsoluteExpirationSeconds { get; set; } = 60;
}
