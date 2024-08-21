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
    private readonly HttpRequestData _request;

    public GetMeasurementsTests()
    {
        _tableClient = Substitute.For<TableClient>();
        _logger = Substitute.For<ILogger<GetMeasurements>>();
        _getMeasurements = new GetMeasurements(_tableClient, _logger);
        _request = Substitute.For<HttpRequestData>(Substitute.For<FunctionContext>());
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenDeviceIdIsEmpty()
    {
        // Arrange

        // Act
        var result = await _getMeasurements.Run(_request, "", "2000-01-01");

        // Assert
        _tableClient.DidNotReceiveWithAnyArgs().QueryAsync<Measurement>();
        _logger.AssertRecieved(2, LogLevel.Information);
        _logger.AssertRecieved(2);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("DeviceId query parameter is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenDeviceIdIsNotInteger()
    {
        // Arrange

        // Act
        var result = await _getMeasurements.Run(_request, "one", "2000-01-01");

        // Assert
        _tableClient.DidNotReceiveWithAnyArgs().QueryAsync<Measurement>();
        _logger.AssertRecieved(2, LogLevel.Information);
        _logger.AssertRecieved(2);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("DeviceId query parameter must be an integer.", badRequestResult.Value);
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenDateUtcIsEmpty()
    {
        // Arrange

        // Act
        var result = await _getMeasurements.Run(_request, "1", "");

        // Assert
        _tableClient.DidNotReceiveWithAnyArgs().QueryAsync<Measurement>();
        _logger.AssertRecieved(2, LogLevel.Information);
        _logger.AssertRecieved(2);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("DateUTC query parameter is required.", badRequestResult.Value);
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenDateUtcIsNotWellFormedDate()
    {
        // Arrange

        // Act
        var result = await _getMeasurements.Run(_request, "1", "00-01-01");

        // Assert
        _tableClient.DidNotReceiveWithAnyArgs().QueryAsync<Measurement>();
        _logger.AssertRecieved(2, LogLevel.Information);
        _logger.AssertRecieved(2);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("DateUTC query parameter must be a date in the format yyyy-MM-dd.", badRequestResult.Value);
    }

    [Fact]
    public async Task Run_ReturnsServerError_WhenQueryAsyncThrows()
    {
        // Arrange
        _tableClient.QueryAsync<Measurement>(Arg.Any<string>()).Throws(new RequestFailedException("Error"));

        // Act
        var result = await _getMeasurements.Run(_request, "1", "2000-01-01");

        // Assert
        _tableClient.ReceivedWithAnyArgs(1).QueryAsync<Measurement>();
        _logger.AssertRecieved(1, LogLevel.Information);
        _logger.AssertRecieved(1, LogLevel.Error);
        _logger.AssertRecieved(2);

        var serverErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, serverErrorResult.StatusCode);
        Assert.Equal("An error occurred while trying to store retrive the measurements.", serverErrorResult.Value);
    }

    [Fact]
    public async Task Run_ReturnsExpectedMeasurements_WhenPartitionKeyIsValid()
    {
        // Arrange

        var DateTime1 = new DateTimeOffset(2001, 1, 1, 1, 1, 1, TimeSpan.Zero);
        var DateTime2 = new DateTimeOffset(2001, 1, 1, 2, 2, 2, TimeSpan.Zero);
        var measurements = new List<Measurement>
        {
            new() { PartitionKey = "1_2001-01-01", RowKey = "2c58023b-e8ce-4dd9-b61f-cefa6fbec95d", DeviceId = 1, DateTime = DateTime1, Pm10 = 2, Pm25 = 3, Pm100 = 4 },
            new() { PartitionKey = "1_2001-01-01", RowKey = "85d5568a-580f-40a3-af29-577a2105812b", DeviceId = 1, DateTime = DateTime2, Pm10 = 6, Pm25 = 7, Pm100 = 8 }
        };

        _tableClient
            .QueryAsync<Measurement>(Arg.Any<string>())
            .Returns(new MockAsyncPageable<Measurement>(measurements));

        // Act
        var result = await _getMeasurements.Run(_request, "1", "2000-01-01");

        // Assert
        _tableClient.ReceivedWithAnyArgs(1).QueryAsync<Measurement>();
        _logger.AssertRecieved(2, LogLevel.Information);
        _logger.AssertRecieved(2);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<List<MeasurementsResponse>>(okResult.Value);
        Assert.Equal(2, response.Count);
        Assert.Equal(new(1, DateTime1, 2, 3, 4), response[0]);
        Assert.Equal(new(1, DateTime2, 6, 7, 8), response[1]);
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
