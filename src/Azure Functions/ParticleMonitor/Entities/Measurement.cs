using Azure;
using Azure.Data.Tables;
using System.Diagnostics.CodeAnalysis;

namespace ParticleMonitor.Entities;

[ExcludeFromCodeCoverage]
public record Measurement : ITableEntity
{
    // PartitionKey format "device{DeviceId}_{Date}".
    public required string PartitionKey { get; set; }

    // RowKey equals time UTC with format HH:mm:ss.
    public required string RowKey { get; set; }

    // Set by database.
    public DateTimeOffset? Timestamp { get; set; }

    // Set by database.
    public ETag ETag { get; set; }

    // Date format HH:mm:ss.
    public required string Date { get; set; }

    public int DeviceId { get; set; }

    public int Pm10 { get; set; }

    public int Pm25 { get; set; }

    public int Pm100 { get; set; }
}
