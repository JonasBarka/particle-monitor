using Azure.Data.Tables;
using Azure;

namespace ParticleMonitor.Entities;

public class Measurement : ITableEntity
{
    // PartitionKey format deviceX_yyyy-MM-dd
    public required string PartitionKey { get; set; }

    // RowKey format HH:mm:ss.fff
    public required string RowKey { get; set; }

    // Set by database
    public DateTimeOffset? Timestamp { get; set; }

    // Set by database
    public ETag ETag { get; set; }

    public int DeviceId { get; set; }

    public int Pm10 { get; set; }

    public int Pm25 { get; set; }

    public int Pm100 { get; set; }
}
