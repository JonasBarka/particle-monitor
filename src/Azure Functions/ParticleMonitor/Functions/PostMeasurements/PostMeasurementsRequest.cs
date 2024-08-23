using ParticleMonitor.Entities;

namespace ParticleMonitor.Functions.PostMeasurements;

public record PostMeasurementsRequest(int DeviceId, int Pm10, int Pm25, int Pm100)
{
    public Measurement ToMeasurement(DateTimeOffset dateTime, Guid rowKey)
    {
        return new Measurement
        {
            PartitionKey = DeviceId + "_" + dateTime.ToString(Constants.DateFormat),
            RowKey = rowKey.ToString(),
            DateTime = dateTime,
            DeviceId = DeviceId,
            Pm10 = Pm10,
            Pm25 = Pm25,
            Pm100 = Pm100
        };
    }
}
