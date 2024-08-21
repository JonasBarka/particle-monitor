using Azure;
using Azure.Core;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using ParticleMonitor.Entities;
using ParticleMonitor.Functions;
using System.Text;
using System.Text.Json;

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
        _postMeasurements = new PostMeasurements(_tableClient, _timeProvider,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },_logger);
    }

    [Fact]
    public async Task Run_ReturnsBadRequest_WhenRequestBodyIsInvalid()
    {
        // Arrange
        var invalidJson = "Invalid JSON";
        var request = Substitute.For<HttpRequest>();
        request.Body.Returns(new MemoryStream(Encoding.UTF8.GetBytes(invalidJson)));

        // Act
        var result = await _postMeasurements.Run(request);

        // Assert
        await _tableClient.DidNotReceiveWithAnyArgs().AddEntityAsync(Arg.Any<Measurement>());
        _logger.AssertRecieved(1, LogLevel.Warning);
        _logger.AssertRecieved(1);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal("Invalid request body.", badRequestResult.Value);
    }

    [Fact]
    public async Task Run_ReturnsOk_WhenMeasurementIsSuccessfullyInserted()
    {
        // Arrange
        var validJson = """
        {
            "deviceId" : 1,
            "pm10" : 2,
            "pm25" : 3,
            "pm100" : 4
        }
        """;
        var request = Substitute.For<HttpRequest>();
        request.Body.Returns(new MemoryStream(Encoding.UTF8.GetBytes(validJson)));

        // Act
        var result = await _postMeasurements.Run(request);

        // Assert
        await _tableClient.Received(1).AddEntityAsync(Arg.Any<Measurement>());
        _logger.AssertRecieved(1, LogLevel.Information);
        _logger.AssertRecieved(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<MeasurementsResponse>(okResult.Value);
        Assert.Equal(1, response.DeviceId);
        Assert.Equal(2, response.Pm10);
        Assert.Equal(3, response.Pm25);
        Assert.Equal(4, response.Pm100);
    }

    [Fact]
    public async Task Run_ReturnsServerError_WhenAddEntityAsyncThrows()
    {
        // Arrange
        var validJson = """
        {
            "deviceId" : 1,
            "pm10" : 2,
            "pm25" : 3,
            "pm100" : 4
        }
        """;
        var request = Substitute.For<HttpRequest>();
        request.Body.Returns(new MemoryStream(Encoding.UTF8.GetBytes(validJson)));

        _tableClient.AddEntityAsync(Arg.Any<Measurement>()).ThrowsAsync(new RequestFailedException("Error"));

        // Act
        var result = await _postMeasurements.Run(request);

        // Assert
        await _tableClient.Received(1).AddEntityAsync(Arg.Any<Measurement>());
        _logger.AssertRecieved(1, LogLevel.Information);
        _logger.AssertRecieved(1, LogLevel.Error);
        _logger.AssertRecieved(2);

        var serverErrorResult = Assert.IsType<ObjectResult>(result);
        Assert.Equal(500, serverErrorResult.StatusCode);
        Assert.Equal("An error ocurred while trying to store the measurement.", serverErrorResult.Value);
    }
}
