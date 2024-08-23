using ParticleMonitor.Entities;
using ParticleMonitor.Functions.PostMeasurements;

namespace ParticleMonitorTests.Functions.PostMeasurments;

public class PostMeasurementsResponseTests
{
    [Fact]
    public void CreateFromMeasurement_CorrectlyConvertsToMeasurementsResponse()
    {
        var dateTimeOffset = new DateTimeOffset(2000, 1, 1, 14, 30, 0, 123, TimeSpan.Zero);

        // Arrange
        var measurement = new Measurement
        {
            PartitionKey = "1_2000-01-01",
            RowKey = "a99373c3-21d6-44f2-a93c-2cda1f68d790",
            DeviceId = 1,
            DateTime = dateTimeOffset,
            Pm10 = 10,
            Pm25 = 25,
            Pm100 = 100
        };

        // Act
        var response = PostMeasurementsResponse.CreateFromMeasurement(measurement);

        // Assert
        Assert.Equal(dateTimeOffset, response.DateTime);
        Assert.Equal(1, response.DeviceId);
        Assert.Equal(10, response.Pm10);
        Assert.Equal(25, response.Pm25);
        Assert.Equal(100, response.Pm100);
    }
}
