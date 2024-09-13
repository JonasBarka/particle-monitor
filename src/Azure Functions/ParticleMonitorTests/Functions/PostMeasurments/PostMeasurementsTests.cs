using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
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
        var dateTimeOffset = DateTimeOffset.UtcNow;
        _postMeasurementsHandler
            .HandleAsync(Arg.Is<PostMeasurementsRequest>(x => x == new PostMeasurementsRequest(1, 2, 3, 4)))
            .Returns(new PostMeasurementsResponse(1, dateTimeOffset, 2, 3, 4));

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
        Assert.Equal(new PostMeasurementsResponse(1, dateTimeOffset, 2, 3, 4), response);
    }
}
