using ParticleMonitor.Entities;

namespace ParticleMonitor.Functions.PostMeasurements;

public record PostMeasurementsResponse(int DeviceId, DateTimeOffset DateTime, int Pm10, int Pm25, int Pm100)
{
    public static PostMeasurementsResponse CreateFromMeasurement(Measurement measurement)
    {
        return new PostMeasurementsResponse(
            measurement.DeviceId,
            measurement.DateTime,
            measurement.Pm10,
            measurement.Pm25,
            measurement.Pm100
        );
    }
}
