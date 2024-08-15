using ParticleMonitor.Functions;

namespace ParticleMonitorTests.Functions;

public class MeasurementsRequestTests
{
    [Fact]
    public void ToMeasurement_CorrectlyConvertsToMeasurement()
    {
        // Arrange
        var request = new MeasurementsRequest(1, 10, 25, 100);
        var dateTime = new DateTimeOffset(2023, 10, 5, 14, 30, 0, 123, TimeSpan.Zero);

        // Act
        var measurement = request.ToMeasurement(dateTime);

        // Assert
        Assert.Equal("device1_2023-10-05", measurement.PartitionKey);
        Assert.Equal("14:30:00.123", measurement.RowKey);
        Assert.Equal(1, measurement.DeviceId);
        Assert.Equal(10, measurement.Pm10);
        Assert.Equal(25, measurement.Pm25);
        Assert.Equal(100, measurement.Pm100);
    }
}
