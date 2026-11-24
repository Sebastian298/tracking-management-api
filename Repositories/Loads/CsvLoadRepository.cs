using System.Globalization;
using CsvHelper;
using Microsoft.Extensions.Options;
using tracking_management_api.Models;
using tracking_management_api.Options.Loads;

namespace tracking_management_api.Repositories.Loads;

public sealed class CsvLoadRepository : ILoadRepository
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<CsvLoadRepository> _logger;
    private readonly LoadsDataOptions _options;

    public CsvLoadRepository(
        IWebHostEnvironment environment,
        IOptions<LoadsDataOptions> options,
        ILogger<CsvLoadRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _environment = environment;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<IReadOnlyList<Load>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var filePath = ResolveFilePath();
        if (!File.Exists(filePath))
        {
            throw new FileNotFoundException($"Loads data file was not found at '{filePath}'.", filePath);
        }

        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        using var reader = new StreamReader(stream);
        using var csv = new CsvReader(reader, CultureInfo.InvariantCulture);
        csv.Context.RegisterClassMap<LoadCsvMap>();

        var maxItems = Math.Max(0, _options.MaxItems);
        _logger.LogInformation(
            "Reading loads from CSV {FilePath} (max {MaxItems}). This path is not IMemoryCache.",
            filePath,
            maxItems);

        var loads = new List<Load>(maxItems);
        await foreach (var load in csv.GetRecordsAsync<Load>(cancellationToken))
        {
            loads.Add(load);
            if (loads.Count >= maxItems)
            {
                break;
            }
        }

        return loads;
    }

    private string ResolveFilePath()
    {
        var configuredPath = _options.FilePath;
        if (Path.IsPathRooted(configuredPath))
        {
            return configuredPath;
        }

        return Path.GetFullPath(Path.Combine(_environment.ContentRootPath, configuredPath));
    }
}
