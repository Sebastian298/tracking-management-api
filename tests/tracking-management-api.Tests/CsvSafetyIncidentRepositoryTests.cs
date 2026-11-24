using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using NSubstitute;
using tracking_management_api.Options.SafetyIncidents;
using tracking_management_api.Repositories.SafetyIncidents;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace tracking_management_api.Tests;

public sealed class CsvSafetyIncidentRepositoryTests
{
    private const string Header =
        "incident_id,trip_id,truck_id,driver_id,incident_date,incident_type,location_city,location_state,at_fault_flag,injury_flag,vehicle_damage_cost,cargo_damage_cost,claim_amount,preventable_flag,description";

    [Fact]
    public async Task AddAsync_AppendsRowAndAssignsNextIncidentId()
    {
        using var directory = new TempDirectory();
        var csvPath = Path.Combine(directory.Path, "safety_incidents.csv");
        await File.WriteAllTextAsync(
            csvPath,
            $"""
            {Header}
            INC00000170,TRIP00036297,TRK00013,DRV00020,2023-04-12 04:00:00,Moving Violation,Memphis,NY,False,False,0.0,0.0,0.0,False,Severe incident involving other driver
            """);

        using var repository = CreateRepository(directory.Path, "safety_incidents.csv");
        var incident = SafetyIncidentFixtures.CreateIncident(string.Empty);

        var stored = await repository.AddAsync(incident);

        Assert.Equal("INC00000171", stored.IncidentId);
        var lines = await File.ReadAllLinesAsync(csvPath);
        Assert.Equal(Header, lines[0]);
        Assert.Equal(3, lines.Length);
        Assert.StartsWith("INC00000171,", lines[2]);
        Assert.Contains("Moving Violation", lines[2]);
    }

    [Fact]
    public async Task AddAsync_WhenFileHasOnlyHeader_StartsAtFirstId()
    {
        using var directory = new TempDirectory();
        var csvPath = Path.Combine(directory.Path, "safety_incidents.csv");
        await File.WriteAllTextAsync(csvPath, Header + "\n");

        using var repository = CreateRepository(directory.Path, "safety_incidents.csv");

        var stored = await repository.AddAsync(SafetyIncidentFixtures.CreateIncident(string.Empty));

        Assert.Equal("INC00000001", stored.IncidentId);
    }

    [Fact]
    public async Task AddAsync_DoesNotRewriteHeader()
    {
        using var directory = new TempDirectory();
        var csvPath = Path.Combine(directory.Path, "safety_incidents.csv");
        await File.WriteAllTextAsync(csvPath, Header + "\n");

        using var repository = CreateRepository(directory.Path, "safety_incidents.csv");
        await repository.AddAsync(SafetyIncidentFixtures.CreateIncident(string.Empty));
        await repository.AddAsync(SafetyIncidentFixtures.CreateIncident(string.Empty));

        var lines = await File.ReadAllLinesAsync(csvPath);
        Assert.Equal(Header, lines[0]);
        Assert.Single(lines, line => line == Header);
        Assert.Equal(3, lines.Length);
    }

    [Fact]
    public async Task AddAsync_WhenCalledInParallel_AssignsUniqueIds()
    {
        using var directory = new TempDirectory();
        var csvPath = Path.Combine(directory.Path, "safety_incidents.csv");
        await File.WriteAllTextAsync(csvPath, Header + "\n");

        using var repository = CreateRepository(directory.Path, "safety_incidents.csv");
        var tasks = Enumerable.Range(0, 20)
            .Select(_ => repository.AddAsync(SafetyIncidentFixtures.CreateIncident(string.Empty)));

        var stored = await Task.WhenAll(tasks);
        var ids = stored.Select(incident => incident.IncidentId).ToHashSet();

        Assert.Equal(20, ids.Count);
        Assert.Contains("INC00000001", ids);
        Assert.Contains("INC00000020", ids);
    }

    private static CsvSafetyIncidentRepository CreateRepository(string contentRootPath, string relativeFilePath)
    {
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.ContentRootPath.Returns(contentRootPath);
        var options = OptionsFactory.Create(new SafetyIncidentsOptions
        {
            FilePath = relativeFilePath,
            ProcessingDelayMs = 1
        });
        return new CsvSafetyIncidentRepository(
            environment,
            options,
            NullLogger<CsvSafetyIncidentRepository>.Instance);
    }

    private sealed class TempDirectory : IDisposable
    {
        public TempDirectory()
        {
            Path = System.IO.Path.Combine(System.IO.Path.GetTempPath(), Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(Path);
        }

        public string Path { get; }

        public void Dispose()
        {
            if (Directory.Exists(Path))
            {
                Directory.Delete(Path, recursive: true);
            }
        }
    }
}
