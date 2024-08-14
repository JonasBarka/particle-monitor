using ParticleMonitor.Entities;

namespace ParticleMonitor.Functions;

public record MeasurementsRequest(int DeviceId, int Pm10, int Pm25, int Pm100)
{
    public Measurement ToMeasurement(DateTimeOffset dateTime)
    {
        return new Measurement
        {
            PartitionKey = $"device{DeviceId}_{dateTime:yyyy-MM-dd}",
            RowKey = dateTime.ToString("HH:mm:ss.fff"),
            DeviceId = DeviceId,
            Pm10 = Pm10,
            Pm25 = Pm25,
            Pm100 = Pm100
        };
    }
}
