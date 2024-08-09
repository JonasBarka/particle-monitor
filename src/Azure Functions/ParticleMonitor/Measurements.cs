using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace ParticleMonitorFunctions;

public class Measurements(ILogger<Measurements> logger)
{
    [Function("measurements")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
        [FromBody] MeasurementRequest measurementRequest)
    {
        logger.LogInformation("Measurement recieved: {measurementRequest}", measurementRequest.ToString());

        var measurementResponse = new MeasurementResponse(
            Guid.NewGuid(),
            DateTime.UtcNow,
            measurementRequest.DeviceId,
            measurementRequest.Pm10,
            measurementRequest.Pm25,
            measurementRequest.Pm100
        );

        return new OkObjectResult(measurementResponse);
    }
}

public record MeasurementRequest(int DeviceId, int Pm10, int Pm25, int Pm100);

public record MeasurementResponse(Guid Guid, DateTime DateTime, int DeviceId, int Pm10, int Pm25, int Pm100);