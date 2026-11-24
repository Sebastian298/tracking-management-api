using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using tracking_management_api.Options.Loads;
using tracking_management_api.Repositories.Loads;
using OptionsFactory = Microsoft.Extensions.Options.Options;

namespace tracking_management_api.Tests;

public sealed class CsvLoadRepositoryTests
{
    [Fact]
    public async Task GetAllAsync_ReadsEveryRowFromCsv()
    {
        using var directory = new TempDirectory();
        var csvPath = Path.Combine(directory.Path, "loads.csv");
        await File.WriteAllTextAsync(
            csvPath,
            """
            load_id,customer_id,route_id,load_date,load_type,weight_lbs,pieces,revenue,fuel_surcharge,accessorial_charges,load_status,booking_type
            LOAD00000001,CUST00183,RTE00019,2022-01-01,Dry Van,19178,13,3045.23,406.72,100,Completed,Spot
            LOAD00000002,CUST00076,RTE00058,2022-01-01,Dry Van,27761,22,1224.48,98.61,0,Completed,Dedicated
            """);

        var repository = CreateRepository(directory.Path, "loads.csv", maxItems: 10_000);

        var loads = await repository.GetAllAsync();

        Assert.Equal(2, loads.Count);
        Assert.Equal("LOAD00000001", loads[0].LoadId);
        Assert.Equal(new DateOnly(2022, 1, 1), loads[0].LoadDate);
        Assert.Equal(3045.23m, loads[0].Revenue);
        Assert.Equal("LOAD00000002", loads[1].LoadId);
        Assert.Equal("Dedicated", loads[1].BookingType);
    }

    [Fact]
    public async Task GetAllAsync_WhenFileIsMissing_ThrowsFileNotFoundException()
    {
        using var directory = new TempDirectory();
        var repository = CreateRepository(directory.Path, "missing.csv", maxItems: 10_000);

        await Assert.ThrowsAsync<FileNotFoundException>(() => repository.GetAllAsync());
    }

    [Fact]
    public async Task GetAllAsync_StopsAfterMaxItems()
    {
        using var directory = new TempDirectory();
        var csvPath = Path.Combine(directory.Path, "loads.csv");
        await File.WriteAllTextAsync(
            csvPath,
            """
            load_id,customer_id,route_id,load_date,load_type,weight_lbs,pieces,revenue,fuel_surcharge,accessorial_charges,load_status,booking_type
            LOAD00000001,CUST00183,RTE00019,2022-01-01,Dry Van,19178,13,3045.23,406.72,100,Completed,Spot
            LOAD00000002,CUST00076,RTE00058,2022-01-01,Dry Van,27761,22,1224.48,98.61,0,Completed,Dedicated
            LOAD00000003,CUST00027,RTE00048,2022-01-01,Refrigerated,35594,16,7171.12,792.88,0,Completed,Spot
            """);

        var repository = CreateRepository(directory.Path, "loads.csv", maxItems: 2);

        var loads = await repository.GetAllAsync();

        Assert.Equal(2, loads.Count);
        Assert.Equal("LOAD00000001", loads[0].LoadId);
        Assert.Equal("LOAD00000002", loads[1].LoadId);
    }

    private static CsvLoadRepository CreateRepository(string contentRootPath, string relativeFilePath, int maxItems)
    {
        var environment = Substitute.For<IWebHostEnvironment>();
        environment.ContentRootPath.Returns(contentRootPath);
        var options = OptionsFactory.Create(new LoadsDataOptions
        {
            FilePath = relativeFilePath,
            MaxItems = maxItems
        });
        return new CsvLoadRepository(environment, options, NullLogger<CsvLoadRepository>.Instance);
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
