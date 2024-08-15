using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using ParticleMonitor.Entities;
using ParticleMonitor.Functions;

namespace ParticleMonitorTests.Functions;

public class PostMeasurementsTests
{
    private readonly TableClient _tableClient;
    private readonly FakeTimeProvider _timeProvider;
    private readonly ILogger<PostMeasurements> _logger;
    private readonly PostMeasurements _postMeasurements;

    public PostMeasurementsTests()
    {
        _tableClient = Substitute.For<TableClient>();
        _timeProvider = new FakeTimeProvider();
        _logger = Substitute.For<ILogger<PostMeasurements>>();
        _postMeasurements = new PostMeasurements(_tableClient, _timeProvider, _logger);
    }

    [Fact]
    public async Task Run_ReturnsOk_WhenMeasurementIsSuccessfullyInserted()
    {
        // Arrange
        var httpRequest = Substitute.For<HttpRequest>();
        var measurementRequest = new MeasurementsRequest(1, 2, 3, 4);

        // Act
        var result = await _postMeasurements.Run(httpRequest, measurementRequest);

        // Assert
        await _tableClient.Received(1).AddEntityAsync(Arg.Any<Measurement>());
        _logger.AssertRecieved(1, LogLevel.Information);
        _logger.AssertRecieved(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<MeasurementsResponse>(okResult.Value);
        Assert.Equal(measurementRequest.DeviceId, response.DeviceId);
        Assert.Equal(measurementRequest.Pm10, response.Pm10);
        Assert.Equal(measurementRequest.Pm25, response.Pm25);
        Assert.Equal(measurementRequest.Pm100, response.Pm100);
    }

    [Fact]
    public async Task Run_ReturnsServerError_WhenAddEntityAsyncThrows()
    {
        // Arrange
        _tableClient.AddEntityAsync(Arg.Any<Measurement>()).ThrowsAsync<Exception>();
        var httpRequest = Substitute.For<HttpRequest>();
        var measurementRequest = new MeasurementsRequest(1, 2, 3, 4);

        // Act
        var result = await _postMeasurements.Run(httpRequest, measurementRequest);

        // Assert
        await _tableClient.Received(1).AddEntityAsync(Arg.Any<Measurement>());
        _logger.AssertRecieved(1, LogLevel.Information);
        _logger.AssertRecieved(1, LogLevel.Error);
        _logger.AssertRecieved(2);

        var serverErrorResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(500, serverErrorResult.StatusCode);
    }
}
