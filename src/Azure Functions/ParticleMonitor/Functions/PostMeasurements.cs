using Azure.Data.Tables;
using ParticleMonitor.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace ParticleMonitor.Functions;

public class PostMeasurements(TableClient tableClient, ILogger<PostMeasurements> logger)
{
    const string _method = "post";
    const string _route = "measurements";

    [Function(nameof(PostMeasurements))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, _method, Route = _route)] HttpRequest req,
        [FromBody] MeasurementsRequest measurementRequest)
    {
        logger.LogInformation("Received {Method} request to {Route} endpoint, with body {Body}.", _method, _route, measurementRequest);
        var measurement = measurementRequest.ToMeasurement();

        await tableClient.AddEntityAsync(measurement);

        var measurementResponse = MeasurementsResponse.CreateFromMeasurement(measurement);

        return new OkObjectResult(measurementResponse);
    }
}

public record MeasurementsRequest(int DeviceId, int Pm10, int Pm25, int Pm100)
{
    public Measurement ToMeasurement()
    {
        var dateTime = DateTime.UtcNow;

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
