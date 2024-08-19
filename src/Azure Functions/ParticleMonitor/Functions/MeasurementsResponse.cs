using ParticleMonitor.Entities;

namespace ParticleMonitor.Functions;

public record MeasurementsResponse(string DateUTC, string TimeUTC, int DeviceId, int Pm10, int Pm25, int Pm100)
{
    public static MeasurementsResponse CreateFromMeasurement(Measurement measurement)
    {
        return new MeasurementsResponse(
            measurement.Date,
            measurement.RowKey,
            measurement.DeviceId,
            measurement.Pm10,
            measurement.Pm25,
            measurement.Pm100
        );
    }
}
