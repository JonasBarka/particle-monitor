using Azure;
using Azure.Data.Tables;
using System.Diagnostics.CodeAnalysis;

namespace ParticleMonitor.Entities;

[ExcludeFromCodeCoverage]
public record Measurement : ITableEntity
{
    // PartitionKey format "device{DeviceId}_{yyyy-MM-dd}".
    public required string PartitionKey { get; set; }

    // UUID v4.
    public required string RowKey { get; set; }

    // Set by database and such can have different date than PartitionKey and DateTime properties.
    public DateTimeOffset? Timestamp { get; set; }

    // Set by database.
    public ETag ETag { get; set; }

    public int DeviceId { get; set; }

    public DateTimeOffset DateTime { get; set; }

    public int Pm10 { get; set; }

    public int Pm25 { get; set; }

    public int Pm100 { get; set; }
}
