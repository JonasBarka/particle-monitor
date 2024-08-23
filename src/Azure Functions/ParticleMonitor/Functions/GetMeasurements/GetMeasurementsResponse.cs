using ParticleMonitor.Entities;

namespace ParticleMonitor.Functions.GetMeasurements;

public record GetMeasurementsResponse(int DeviceId, DateTimeOffset DateTime, int Pm10, int Pm25, int Pm100)
{
    public static GetMeasurementsResponse CreateFromMeasurement(Measurement measurement)
    {
        return new GetMeasurementsResponse(
            measurement.DeviceId,
            measurement.DateTime,
            measurement.Pm10,
            measurement.Pm25,
            measurement.Pm100
        );
    }
}
