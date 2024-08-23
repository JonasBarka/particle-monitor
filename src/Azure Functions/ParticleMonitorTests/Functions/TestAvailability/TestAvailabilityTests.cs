using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using PM = ParticleMonitor.Functions.TestAvailabilty;

namespace ParticleMonitorTests.Functions.TestAvailability;

public class TestAvailabilityTests
{
    [Fact]
    public void Run_ReturnsOkObjectResult_WithExpectedMessage()
    {
        // Arrange
        var logger = Substitute.For<ILogger<PM.TestAvailability>>();
        var testAvailability = new PM.TestAvailability(logger);
        var request = Substitute.For<HttpRequest>();

        // Act
        var result = testAvailability.Run(request);

        // Assert
        logger.AssertRecieved(1, LogLevel.Information);
        logger.AssertRecieved(1);

        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Particle Monitor server is available.", okResult.Value);
    }
}
