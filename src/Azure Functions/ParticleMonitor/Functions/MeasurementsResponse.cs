using ParticleMonitor.Entities;

namespace ParticleMonitor.Functions;

public record MeasurementsResponse(string PartitionKey, string RowKey, int DeviceId, int Pm10, int Pm25, int Pm100)
{
    public static MeasurementsResponse CreateFromMeasurement(Measurement measurement)
    {
        return new MeasurementsResponse(
            measurement.PartitionKey,
            measurement.RowKey,
            measurement.DeviceId,
            measurement.Pm10,
            measurement.Pm25,
            measurement.Pm100
        );
    }
}
