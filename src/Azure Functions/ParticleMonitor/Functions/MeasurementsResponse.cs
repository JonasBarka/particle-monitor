using ParticleMonitor.Entities;

namespace ParticleMonitor.Functions;

public record MeasurementsResponse(int DeviceId, DateTimeOffset DateTime, int Pm10, int Pm25, int Pm100)
{
    public static MeasurementsResponse CreateFromMeasurement(Measurement measurement)
    {
        return new MeasurementsResponse(
            measurement.DeviceId,
            measurement.DateTime,
            measurement.Pm10,
            measurement.Pm25,
            measurement.Pm100
        );
    }
}
