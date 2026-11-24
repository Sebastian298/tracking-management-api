using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;
using Microsoft.Extensions.Options;
using tracking_management_api.Models;
using tracking_management_api.Options.SafetyIncidents;

namespace tracking_management_api.Repositories.SafetyIncidents;

public sealed class CsvSafetyIncidentRepository : ISafetyIncidentRepository, IDisposable
{
    private const string IncidentIdPrefix = "INC";
    private const string HeaderRow =
        "incident_id,trip_id,truck_id,driver_id,incident_date,incident_type,location_city,location_state,at_fault_flag,injury_flag,vehicle_damage_cost,cargo_damage_cost,claim_amount,preventable_flag,description";

    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<CsvSafetyIncidentRepository> _logger;
    private readonly SafetyIncidentsOptions _options;
    private readonly SemaphoreSlim _writeLock = new(1, 1);

    public CsvSafetyIncidentRepository(
        IWebHostEnvironment environment,
        IOptions<SafetyIncidentsOptions> options,
        ILogger<CsvSafetyIncidentRepository> logger)
    {
        ArgumentNullException.ThrowIfNull(environment);
        ArgumentNullException.ThrowIfNull(options);
        ArgumentNullException.ThrowIfNull(logger);

        _environment = environment;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<SafetyIncident> AddAsync(
        SafetyIncident incident,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(incident);

        await _writeLock.WaitAsync(cancellationToken);
        try
        {
            var filePath = ResolveFilePath();
            await EnsureFileExistsAsync(filePath, cancellationToken);

            var nextId = await ReadNextIncidentIdAsync(filePath, cancellationToken);
            var stored = incident with { IncidentId = nextId };

            await AppendIncidentAsync(filePath, stored, cancellationToken);

            _logger.LogInformation(
                "Appended safety incident {IncidentId} to {FilePath}.",
                stored.IncidentId,
                filePath);

            return stored;
        }
        finally
        {
            _writeLock.Release();
        }
    }

    public void Dispose()
    {
        _writeLock.Dispose();
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

    private static async Task EnsureFileExistsAsync(string filePath, CancellationToken cancellationToken)
    {
        var directory = Path.GetDirectoryName(filePath);
        if (!string.IsNullOrEmpty(directory))
        {
            Directory.CreateDirectory(directory);
        }

        if (File.Exists(filePath))
        {
            return;
        }

        await File.WriteAllTextAsync(
            filePath,
            HeaderRow + "\n",
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            cancellationToken);
    }

    private static async Task<string> ReadNextIncidentIdAsync(string filePath, CancellationToken cancellationToken)
    {
        var lastId = await ReadLastIncidentIdAsync(filePath, cancellationToken);
        if (string.IsNullOrWhiteSpace(lastId) || !lastId.StartsWith(IncidentIdPrefix, StringComparison.Ordinal))
        {
            return $"{IncidentIdPrefix}00000001";
        }

        var numericPart = lastId[IncidentIdPrefix.Length..];
        if (!int.TryParse(numericPart, NumberStyles.None, CultureInfo.InvariantCulture, out var number))
        {
            return $"{IncidentIdPrefix}00000001";
        }

        return string.Create(CultureInfo.InvariantCulture, $"{IncidentIdPrefix}{number + 1:D8}");
    }

    private static async Task<string?> ReadLastIncidentIdAsync(string filePath, CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.Read,
            bufferSize: 4096,
            FileOptions.Asynchronous | FileOptions.SequentialScan);

        using var reader = new StreamReader(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false));
        string? lastDataLine = null;
        var isHeader = true;

        while (await reader.ReadLineAsync(cancellationToken) is { } line)
        {
            if (isHeader)
            {
                isHeader = false;
                continue;
            }

            if (!string.IsNullOrWhiteSpace(line))
            {
                lastDataLine = line;
            }
        }

        if (lastDataLine is null)
        {
            return null;
        }

        var separatorIndex = lastDataLine.IndexOf(',');
        return separatorIndex < 0 ? lastDataLine : lastDataLine[..separatorIndex];
    }

    private static async Task AppendIncidentAsync(
        string filePath,
        SafetyIncident incident,
        CancellationToken cancellationToken)
    {
        await using var stream = new FileStream(
            filePath,
            FileMode.Open,
            FileAccess.ReadWrite,
            FileShare.Read,
            bufferSize: 4096,
            FileOptions.Asynchronous);

        await EnsureTrailingNewlineAsync(stream, cancellationToken);

        var configuration = new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            HasHeaderRecord = false,
            NewLine = "\n"
        };

        await using var writer = new StreamWriter(
            stream,
            new UTF8Encoding(encoderShouldEmitUTF8Identifier: false),
            leaveOpen: false);
        await using var csv = new CsvWriter(writer, configuration);
        csv.Context.RegisterClassMap<SafetyIncidentCsvMap>();
        csv.WriteRecord(incident);
        await csv.NextRecordAsync();
        await csv.FlushAsync();
    }

    private static async Task EnsureTrailingNewlineAsync(FileStream stream, CancellationToken cancellationToken)
    {
        if (stream.Length == 0)
        {
            return;
        }

        stream.Seek(-1, SeekOrigin.End);
        var lastByte = stream.ReadByte();
        stream.Seek(0, SeekOrigin.End);

        if (lastByte != '\n')
        {
            await stream.WriteAsync("\n"u8.ToArray(), cancellationToken);
        }
    }
}
