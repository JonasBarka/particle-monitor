using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Time.Testing;
using ParticleMonitor.Entities;
using ParticleMonitor.Functions.PostMeasurements;
using System.Text;
using System.Text.Json;

namespace ParticleMonitorTests.Functions.PostMeasurments;

public class PostMeasurementsTests
{
    private readonly IPostMeasurementsHandler _postMeasurementsHandler;
    private readonly ILogger<PostMeasurements> _logger;
    private readonly PostMeasurements _postMeasurements;

    public PostMeasurementsTests()
    {
        _postMeasurementsHandler = Substitute.For<IPostMeasurementsHandler>();
        _postMeasurementsHandler
            .HandleAsync(Arg.Any<PostMeasurementsRequest>())
            .Returns(new PostMeasurementsResponse(1, DateTimeOffset.UtcNow, 2, 3, 4));

        _logger = Substitute.For<ILogger<PostMeasurements>>();
        _postMeasurements = new PostMeasurements(
            _postMeasurementsHandler,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true },
            _logger);
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
        await _postMeasurementsHandler.DidNotReceiveWithAnyArgs().HandleAsync(Arg.Any<PostMeasurementsRequest>());
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
        await _postMeasurementsHandler.Received(1).HandleAsync(Arg.Any<PostMeasurementsRequest>());
        _logger.AssertRecieved(1, LogLevel.Information);
        _logger.AssertRecieved(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<PostMeasurementsResponse>(okResult.Value);
        Assert.Equal(1, response.DeviceId);
        Assert.Equal(2, response.Pm10);
        Assert.Equal(3, response.Pm25);
        Assert.Equal(4, response.Pm100);
    }

    //[Fact]
    //public async Task Run_ReturnsServerError_WhenAddEntityAsyncThrows()
    //{
    //    // Arrange
    //    var validJson = """
    //    {
    //        "deviceId" : 1,
    //        "pm10" : 2,
    //        "pm25" : 3,
    //        "pm100" : 4
    //    }
    //    """;
    //    var request = Substitute.For<HttpRequest>();
    //    request.Body.Returns(new MemoryStream(Encoding.UTF8.GetBytes(validJson)));

    //    _tableClient.AddEntityAsync(Arg.Any<Measurement>()).ThrowsAsync(new RequestFailedException("Error"));

    //    // Act
    //    var result = await _postMeasurements.Run(request);

    //    // Assert
    //    await _tableClient.Received(1).AddEntityAsync(Arg.Any<Measurement>());
    //    _logger.AssertRecieved(1, LogLevel.Information);
    //    _logger.AssertRecieved(1, LogLevel.Error);
    //    _logger.AssertRecieved(2);

    //    var serverErrorResult = Assert.IsType<ObjectResult>(result);
    //    Assert.Equal(500, serverErrorResult.StatusCode);
    //    Assert.Equal("An error occurred while trying to store the measurement.", serverErrorResult.Value);
    //}
}
