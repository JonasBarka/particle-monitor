using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ParticleMonitor.Functions.GetMeasurements;

namespace ParticleMonitorTests.Functions.GetMeasurments;

public class GetMeasurementsTests
{
    private readonly IGetMeasurementsHandler _handler;
    private readonly ILogger<GetMeasurements> _logger;
    private readonly GetMeasurements _getMeasurements;
    private readonly HttpRequestData _request;

    public GetMeasurementsTests()
    {
        _handler = Substitute.For<IGetMeasurementsHandler>();
        _logger = Substitute.For<ILogger<GetMeasurements>>();
        _getMeasurements = new GetMeasurements(_handler, _logger);
        _request = Substitute.For<HttpRequestData>(Substitute.For<FunctionContext>());
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenDeviceIdIsEmpty()
    {
        // Arrange

        // Act
        var result = await _getMeasurements.Run(_request, "", "2000-01-01");

        // Assert
        await _handler.DidNotReceive().HandleAsync(Arg.Any<string>(), Arg.Any<string>());
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
        await _handler.DidNotReceive().HandleAsync(Arg.Any<string>(), Arg.Any<string>());
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
        await _handler.DidNotReceive().HandleAsync(Arg.Any<string>(), Arg.Any<string>());
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
        await _handler.DidNotReceive().HandleAsync(Arg.Any<string>(), Arg.Any<string>());
        _logger.AssertRecieved(2, LogLevel.Information);
        _logger.AssertRecieved(2);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("DateUTC query parameter must be a date in the format yyyy-MM-dd.", badRequestResult.Value);
    }

    [Fact]
    public async Task Run_Throws_WhenQueryAsyncThrows()
    {
        // Arrange
        _handler.HandleAsync(Arg.Any<string>(), Arg.Any<string>()).ThrowsAsync(new Exception());

        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _getMeasurements.Run(_request, "1", "2000-01-01"));

        // Assert
        await _handler.Received(1).HandleAsync(Arg.Any<string>(), Arg.Any<string>());
        _logger.AssertRecieved(1, LogLevel.Information);
        _logger.AssertRecieved(1);
    }

    [Fact]
    public async Task Run_ReturnsExpectedMeasurements_WhenPartitionKeyIsValid()
    {
        // Arrange

        var DateTime1 = new DateTimeOffset(2001, 1, 1, 1, 1, 1, TimeSpan.Zero);
        var DateTime2 = new DateTimeOffset(2001, 1, 1, 2, 2, 2, TimeSpan.Zero);
        var measurements = new List<GetMeasurementsResponse>
        {
            new(1, DateTime1, 2, 3, 4),
            new(1, DateTime2, 6, 7, 8),
        };

        _handler
            .HandleAsync(Arg.Any<string>(), Arg.Any<string>())
            .Returns(measurements);

        // Act
        var result = await _getMeasurements.Run(_request, "1", "2000-01-01");

        // Assert
        await _handler.Received(1).HandleAsync(Arg.Any<string>(), Arg.Any<string>());
        _logger.AssertRecieved(2, LogLevel.Information);
        _logger.AssertRecieved(2);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<List<GetMeasurementsResponse>>(okResult.Value);
        Assert.Equal(2, response.Count);
        Assert.Equal(new(1, DateTime1, 2, 3, 4), response[0]);
        Assert.Equal(new(1, DateTime2, 6, 7, 8), response[1]);
    }
}

// Manual mock needed to fulfill notnull constraint on T, which otherwise causes a warning.
//public class MockAsyncPageable<T>(IEnumerable<T> items) : AsyncPageable<T> where T : notnull
//{
//    public override async IAsyncEnumerable<Page<T>> AsPages(string? continuationToken = null, int? pageSizeHint = null)
//    {
//        var page = Page<T>.FromValues(items.ToList(), null, Substitute.For<Response>());
//        yield return await Task.FromResult(page);
//    }

//    public override async IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default)
//    {
//        foreach (var item in items)
//        {
//            yield return await Task.FromResult(item);
//        }
//    }
//}
