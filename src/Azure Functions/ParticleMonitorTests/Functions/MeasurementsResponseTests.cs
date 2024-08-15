using ParticleMonitor.Entities;
using ParticleMonitor.Functions;

namespace ParticleMonitorTests.Functions;

public class MeasurementsResponseTests
{
    [Fact]
    public void CreateFromMeasurement_CorrectlyConvertsToMeasurementsResponse()
    {
        // Arrange
        var measurement = new Measurement
        {
            PartitionKey = "device1_2023-10-05",
            RowKey = "14:30:00.123",
            DeviceId = 1,
            Pm10 = 10,
            Pm25 = 25,
            Pm100 = 100
        };

        // Act
        var response = MeasurementsResponse.CreateFromMeasurement(measurement);

        // Assert
        Assert.Equal("device1_2023-10-05", response.PartitionKey);
        Assert.Equal("14:30:00.123", response.RowKey);
        Assert.Equal(1, response.DeviceId);
        Assert.Equal(10, response.Pm10);
        Assert.Equal(25, response.Pm25);
        Assert.Equal(100, response.Pm100);
    }
}
