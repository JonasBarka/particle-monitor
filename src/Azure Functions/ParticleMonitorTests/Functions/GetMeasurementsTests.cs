using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ParticleMonitor.Entities;
using ParticleMonitor.Functions;

namespace ParticleMonitorTests.Functions;

public class GetMeasurementsTests
{
    private readonly TableClient _tableClient;
    private readonly ILogger<GetMeasurements> _logger;
    private readonly GetMeasurements _getMeasurements;

    public GetMeasurementsTests()
    {
        _tableClient = Substitute.For<TableClient>();
        _logger = Substitute.For<ILogger<GetMeasurements>>();
        _getMeasurements = new GetMeasurements(_tableClient, _logger);
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenPartitionKeyIsNullOrEmpty()
    {
        // Arrange
        var request = Substitute.For<HttpRequestData>(Substitute.For<FunctionContext>());

        // Act
        var result = await _getMeasurements.Run(request, "");

        // Assert
        _tableClient.DidNotReceiveWithAnyArgs().QueryAsync<Measurement>();
        _logger.AssertRecieved(2, LogLevel.Information);
        _logger.AssertRecieved(2);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("PartitionKey query parameter is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task Run_ReturnsServerError_WhenQueryAsyncThrows()
    {
        // Arrange
        _tableClient.QueryAsync<Measurement>(Arg.Any<string>()).Throws<Exception>();
        var request = Substitute.For<HttpRequestData>(Substitute.For<FunctionContext>());

        // Act
        var result = await _getMeasurements.Run(request, "testPartitionKey");

        // Assert
        _tableClient.ReceivedWithAnyArgs(1).QueryAsync<Measurement>();
        _logger.AssertRecieved(1, LogLevel.Information);
        _logger.AssertRecieved(1, LogLevel.Error);
        _logger.AssertRecieved(2);

        var serverErrorResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(500, serverErrorResult.StatusCode);
    }

    [Fact]
    public async Task Run_ReturnsExpectedMeasurements_WhenPartitionKeyIsValid()
    {
        // Arrange
        var measurements = new List<Measurement>
        {
            new() { PartitionKey = "testPartitionKey1", RowKey = "1", DeviceId = 1, Pm10 = 2, Pm25 = 3, Pm100 = 4 },
            new() { PartitionKey = "testPartitionKey2", RowKey = "2", DeviceId = 5, Pm10 = 6, Pm25 = 7, Pm100 = 8 }
        };

        _tableClient
            .QueryAsync<Measurement>(Arg.Any<string>())
            .Returns(new MockAsyncPageable<Measurement>(measurements));

        var request = Substitute.For<HttpRequestData>(Substitute.For<FunctionContext>());

        // Act
        var result = await _getMeasurements.Run(request, "testPartitionKey");

        // Assert
        _tableClient.ReceivedWithAnyArgs(1).QueryAsync<Measurement>();
        _logger.AssertRecieved(2, LogLevel.Information);
        _logger.AssertRecieved(2);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<List<MeasurementsResponse>>(okResult.Value);
        Assert.Equal(2, response.Count);
        Assert.Equal(new("testPartitionKey1", "1", 1, 2, 3, 4), response[0]);
        Assert.Equal(new("testPartitionKey2", "2", 5, 6, 7, 8), response[1]);
    }
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
