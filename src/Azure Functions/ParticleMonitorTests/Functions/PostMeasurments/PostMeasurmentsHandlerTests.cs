using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using ParticleMonitor.Entities;
using ParticleMonitor.Functions.PostMeasurements;

namespace ParticleMonitorTests.Functions.PostMeasurments;

public class PostMeasurementsHandlerTests
{
    private readonly TableClient _mockTableClient;
    private readonly TimeProvider _mockTimeProvider;
    private readonly ILogger<PostMeasurementsHandler> _mockLogger;
    private readonly PostMeasurementsHandler _handler;

    public PostMeasurementsHandlerTests()
    {
        _mockTableClient = Substitute.For<TableClient>();
        _mockTimeProvider = Substitute.For<TimeProvider>();
        _mockLogger = Substitute.For<ILogger<PostMeasurementsHandler>>();
        _handler = new PostMeasurementsHandler(_mockTableClient, _mockTimeProvider, _mockLogger);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnResponse_WhenMeasurementIsStoredSuccessfully()
    {
        // Arrange
        var request = new PostMeasurementsRequest(1, 10, 25, 100);
        var dateTime = new DateTimeOffset(2023, 10, 1, 12, 0, 0, TimeSpan.Zero);
        _mockTimeProvider.GetUtcNow().Returns(dateTime);

        // Act
        var response = await _handler.HandleAsync(request);

        // Assert
        Assert.Equal(request.DeviceId, response.DeviceId);
        Assert.Equal(dateTime, response.DateTime);
        Assert.Equal(request.Pm10, response.Pm10);
        Assert.Equal(request.Pm25, response.Pm25);
        Assert.Equal(request.Pm100, response.Pm100);
        await _mockTableClient.Received(1).AddEntityAsync(Arg.Any<Measurement>());
    }

    [Fact]
    public async Task HandleAsync_ShouldLogErrorAndThrowException_WhenRequestFails()
    {
        // Arrange
        var request = new PostMeasurementsRequest(1, 10, 25, 100);
        var dateTime = new DateTimeOffset(2023, 10, 1, 12, 0, 0, TimeSpan.Zero);
        _mockTimeProvider.GetUtcNow().Returns(dateTime);
        _mockTableClient.AddEntityAsync(Arg.Any<Measurement>()).ThrowsAsync(new RequestFailedException("Error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RequestFailedException>(() => _handler.HandleAsync(request));

        _mockLogger.AssertRecieved(1, LogLevel.Error);
    }
}
