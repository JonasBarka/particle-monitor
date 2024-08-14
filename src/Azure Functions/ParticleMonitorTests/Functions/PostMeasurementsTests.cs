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
    [Fact]
    public async Task Run_ReturnsOk_WhenMeasurementIsSuccessfullyInserted()
    {
        // Arrange
        var tableClient = Substitute.For<TableClient>();
        
        var timeProvider = new FakeTimeProvider();

        var logger = Substitute.For<ILogger<PostMeasurements>>();
        
        var postMeasurements = new PostMeasurements(tableClient, timeProvider, logger);

        var httpRequest = Substitute.For<HttpRequest>();
        var measurementRequest = new MeasurementsRequest(1, 2, 3, 4);

        // Act
        var result = await postMeasurements.Run(httpRequest, measurementRequest);

        // Assert
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
        var tableClient = Substitute.For<TableClient>();
        tableClient.AddEntityAsync(Arg.Any<Measurement>()).ThrowsAsync<Exception>();

        var timeProvider = new FakeTimeProvider();
        var logger = Substitute.For<ILogger<PostMeasurements>>();
        var postMeasurements = new PostMeasurements(tableClient, timeProvider, logger);

        var request = Substitute.For<HttpRequest>();
        var measurementRequest = new MeasurementsRequest(1, 2, 3, 4);

        // Act
        var result = await postMeasurements.Run(request, measurementRequest);

        // Assert
        var serverErrorResult = Assert.IsType<StatusCodeResult>(result);
        Assert.Equal(500, serverErrorResult.StatusCode);
    }
}
