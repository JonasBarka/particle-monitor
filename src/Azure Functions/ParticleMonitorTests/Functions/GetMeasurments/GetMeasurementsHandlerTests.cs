using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using ParticleMonitor.Entities;
using ParticleMonitor.Functions.GetMeasurements;

namespace ParticleMonitorTests.Functions.GetMeasurments;

public class GetMeasurementsHandlerTests
{
    private readonly TableClient _tableClient;
    private readonly ILogger<GetMeasurementsHandler> _logger;
    private readonly GetMeasurementsHandler _handler;

    public GetMeasurementsHandlerTests()
    {
        _tableClient = Substitute.For<TableClient>();
        _logger = Substitute.For<ILogger<GetMeasurementsHandler>>();
        _handler = new GetMeasurementsHandler(_tableClient, _logger);
    }

    [Fact]
    public async Task HandleAsync_ReturnsMeasurements_WhenDataExists()
    {
        // Arrange
        var measurements = new List<Measurement>
        {
            new()
            {
                PartitionKey = "1_2023-10-01",
                RowKey = Guid.NewGuid().ToString(),
                DeviceId = 1,
                DateTime = DateTimeOffset.UtcNow,
                Pm10 = 1,
                Pm25 = 2,
                Pm100 = 3
            },
            new()
            {
                PartitionKey = "1_2023-10-01",
                RowKey = Guid.NewGuid().ToString(),
                DeviceId = 1,
                DateTime = DateTimeOffset.UtcNow,
                Pm10 = 4,
                Pm25 = 5,
                Pm100 = 6
            }
        };

        _tableClient
            .QueryAsync<Measurement>(Arg.Any<string>())
            .Returns(new MockAsyncPageable<Measurement>(measurements));

        // Act
        var result = await _handler.HandleAsync("1", "2023-10-01");

        // Assert
        Assert.Equal(2, result.Count);

        Assert.Equal(measurements[0].DeviceId, result[0].DeviceId);
        Assert.Equal(measurements[0].DateTime, result[0].DateTime);
        Assert.Equal(measurements[0].Pm10, result[0].Pm10);
        Assert.Equal(measurements[0].Pm25, result[0].Pm25);
        Assert.Equal(measurements[0].Pm100, result[0].Pm100);

        Assert.Equal(measurements[1].DeviceId, result[1].DeviceId);
        Assert.Equal(measurements[1].DateTime, result[1].DateTime);
        Assert.Equal(measurements[1].Pm10, result[1].Pm10);
        Assert.Equal(measurements[1].Pm25, result[1].Pm25);
        Assert.Equal(measurements[1].Pm100, result[1].Pm100);
    }

    [Fact]
    public async Task HandleAsync_ReturnsEmpty_WhenDataDonNotExist()
    {
        // Arrange
        var measurements = new List<Measurement>();

        _tableClient
            .QueryAsync<Measurement>(Arg.Any<string>())
            .Returns(new MockAsyncPageable<Measurement>(measurements));

        // Act
        var result = await _handler.HandleAsync("1", "2023-10-01");

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task HandleAsync_Throws_WhenQueryAsyncThrows()
    {
        // Arrange
        var deviceId = "device123";
        var dateUtc = "2023-10-01";
        var partitionKey = $"{deviceId}_{dateUtc}";

        _tableClient
            .QueryAsync<Measurement>(Arg.Any<string>())
            .Throws(new Exception("Test exception"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<Exception>(() => _handler.HandleAsync(deviceId, dateUtc));
        Assert.Equal("Test exception", exception.Message);
        _logger.AssertRecieved(1, LogLevel.Error);
        _logger.AssertRecieved(1);
    }

    // Manual mock needed to fulfill notnull constraint on T, which otherwise causes a warning.
    public class MockAsyncPageable<T>(IEnumerable<T> items) : AsyncPageable<T> where T : notnull
    {
        public override async IAsyncEnumerable<Page<T>> AsPages(string? continuationToken = null, int? pageSizeHint = null)
        {
            var page = Page<T>.FromValues(items.ToList(), null, Substitute.For<Response>());
            yield return await Task.FromResult(page);
        }

        public override async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
        {
            foreach (var item in items)
            {
                yield return await Task.FromResult(item);
            }
        }
    }
}
