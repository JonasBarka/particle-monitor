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

    private readonly DateTimeOffset _dateTime;
    private readonly PostMeasurementsRequest _request;

    public PostMeasurementsHandlerTests()
    {
        _mockTableClient = Substitute.For<TableClient>();

        _dateTime = new DateTimeOffset(2000, 1, 1, 0, 0, 0, TimeSpan.Zero);
        _mockTimeProvider = Substitute.For<TimeProvider>();
        _mockTimeProvider
            .GetUtcNow()
            .Returns(_dateTime);

        _mockLogger = Substitute.For<ILogger<PostMeasurementsHandler>>();
        _handler = new PostMeasurementsHandler(_mockTableClient, _mockTimeProvider, _mockLogger);
        
        _request = new PostMeasurementsRequest(1, 2, 3, 4);
    }

    [Fact]
    public async Task HandleAsync_ShouldReturnResponse_WhenMeasurementIsStoredSuccessfully()
    {
        // Arrange

        // Act
        var response = await _handler.HandleAsync(_request);

        // Assert

        // Default response from AddEntityAsync is valid, so we need to check the call instead of the return value.
        await _mockTableClient.Received(1).AddEntityAsync(Arg.Is<Measurement>(x=>
            x.DateTime == _dateTime 
            && x.Pm10 == 2
            && x.Pm25 == 3
            && x.Pm100 == 4));

        await _mockTableClient.Received(1).AddEntityAsync(Arg.Any<Measurement>());

        Assert.Equal(_request.DeviceId, response.DeviceId);
        Assert.Equal(_dateTime, response.DateTime);
        Assert.Equal(_request.Pm10, response.Pm10);
        Assert.Equal(_request.Pm25, response.Pm25);
        Assert.Equal(_request.Pm100, response.Pm100); 
    }

    [Fact]
    public async Task HandleAsync_ShouldLogErrorAndThrowException_WhenAddThrows()
    {
        // Arrange
        _mockTableClient
            .AddEntityAsync(Arg.Any<Measurement>())
            .ThrowsAsync(new RequestFailedException("Error"));

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RequestFailedException>(() => _handler.HandleAsync(_request));

        // Assert
        _mockLogger.AssertRecieved(1, LogLevel.Error);
        _mockLogger.AssertRecieved(1);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenAddReturnsNull()
    {
        // Arrange
        _mockTableClient
            .AddEntityAsync(Arg.Any<Measurement>())
            .ReturnsNull();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RequestFailedException>(() => _handler.HandleAsync(_request));

        // Assert
        _mockLogger.AssertRecieved(1, LogLevel.Error);
        _mockLogger.AssertRecieved(1);
    }

    [Fact]
    public async Task HandleAsync_ShouldThrow_WhenAddReturnsFailed()
    {
        // Arrange
        var _mockResponse = Substitute.For<Response>();
        _mockResponse
            .IsError
            .Returns(true);

        _mockTableClient.AddEntityAsync(Arg.Any<Measurement>()).Returns(_mockResponse);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<RequestFailedException>(() => _handler.HandleAsync(_request));

        // Assert
        _mockLogger.AssertRecieved(1, LogLevel.Error);
        _mockLogger.AssertRecieved(1);
    }
}
