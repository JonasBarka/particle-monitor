using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParticleMonitor.Functions;

namespace ParticleMonitorTests.Functions;

public class TestAvailabilityTests
{
    [Fact]
    public void Run_ReturnsOkObjectResult_WithExpectedMessage()
    {
        // Arrange
        var logger = Substitute.For<ILogger<TestAvailability>>();
        var testAvailability = new TestAvailability(logger);

        var request = Substitute.For<HttpRequest>();

        // Act
        var result = testAvailability.Run(request);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("Particle Monitor server is available.", okResult.Value);
    }
}
