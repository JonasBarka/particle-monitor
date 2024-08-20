using ParticleMonitor.Functions;

namespace ParticleMonitorTests.Functions;

public class MeasurementsRequestTests
{
    [Fact]
    public void ToMeasurement_CorrectlyConvertsToMeasurement()
    {
        // Arrange
        var request = new MeasurementsRequest(1, 10, 25, 100);
        var dateTime = new DateTimeOffset(2001, 1, 1, 1, 1, 1, 0, TimeSpan.Zero);
        var guid = Guid.Parse("cc3f632b-d3f5-41c5-810a-b4a71672fc2f");

        // Act
        var measurement = request.ToMeasurement(dateTime, guid);

        // Assert
        Assert.Equal("1_2001-01-01", measurement.PartitionKey);
        Assert.Equal(guid.ToString(), measurement.RowKey);
        Assert.Equal(dateTime, measurement.DateTime);
        Assert.Equal(1, measurement.DeviceId);
        Assert.Equal(10, measurement.Pm10);
        Assert.Equal(25, measurement.Pm25);
        Assert.Equal(100, measurement.Pm100);
    }
}
