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
        await _handler.DidNotReceiveWithAnyArgs().HandleAsync(default!, default!);
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
        await _handler.DidNotReceiveWithAnyArgs().HandleAsync(default!, default!);
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
        await _handler.DidNotReceiveWithAnyArgs().HandleAsync(default!, default!);
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
        await _handler.DidNotReceiveWithAnyArgs().HandleAsync(default!, default!);
        _logger.AssertRecieved(2, LogLevel.Information);
        _logger.AssertRecieved(2);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("DateUTC query parameter must be a date in the format yyyy-MM-dd.", badRequestResult.Value);
    }

    [Fact]
    public async Task Run_Throws_WhenQueryAsyncThrows()
    {
        // Arrange
        _handler.HandleAsync("1", "2000-01-01").ThrowsAsync(new Exception());

        // Act
        await Assert.ThrowsAnyAsync<Exception>(() => _getMeasurements.Run(_request, "1", "2000-01-01"));

        // Assert
        await _handler.ReceivedWithAnyArgs(1).HandleAsync(default!, default!);
        _logger.AssertRecieved(1, LogLevel.Information);
        _logger.AssertRecieved(1);
    }

    [Fact]
    public async Task Run_ReturnsExpectedMeasurements_IdAndDateAreValid()
    {
        // Arrange

        var dateTime1 = new DateTimeOffset(2001, 1, 1, 1, 1, 1, TimeSpan.Zero);
        var dateTime2 = new DateTimeOffset(2001, 1, 1, 2, 2, 2, TimeSpan.Zero);
        var measurement1 = new GetMeasurementsResponse(1, dateTime1, 2, 3, 4);
        var measurement2 = new GetMeasurementsResponse(1, dateTime2, 6, 7, 8);

        var measurements = new List<GetMeasurementsResponse>
        {
            measurement1,
            measurement2,
        };

        _handler
            .HandleAsync("1", "2000-01-01")
            .Returns(measurements);

        // Act
        var result = await _getMeasurements.Run(_request, "1", "2000-01-01");

        // Assert
        await _handler.ReceivedWithAnyArgs(1).HandleAsync(default!, default!);
        _logger.AssertRecieved(2, LogLevel.Information);
        _logger.AssertRecieved(2);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<List<GetMeasurementsResponse>>(okResult.Value);
        Assert.Equal(2, response.Count);
        Assert.Equal(measurement1, response[0]);
        Assert.Equal(measurement2, response[1]);
    }
}
