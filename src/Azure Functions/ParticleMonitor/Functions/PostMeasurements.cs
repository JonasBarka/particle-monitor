using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using FromBodyAttribute = Microsoft.Azure.Functions.Worker.Http.FromBodyAttribute;

namespace ParticleMonitor.Functions;

public class PostMeasurements(TableClient tableClient, TimeProvider timeProvider, ILogger<PostMeasurements> logger)
{
    const string _method = "post";
    const string _route = "measurements";

    [Function(nameof(PostMeasurements))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, _method, Route = _route)] HttpRequest req,
        [FromBody] MeasurementsRequest measurementRequest)
    {
        logger.LogInformation("Received {Method} request to {Route} endpoint, with body {Body}.", _method, _route, measurementRequest);
        
        try
        {
            var measurement = measurementRequest.ToMeasurement(timeProvider.GetUtcNow());

            await tableClient.AddEntityAsync(measurement);

            var measurementResponse = MeasurementsResponse.CreateFromMeasurement(measurement);

            return new OkObjectResult(measurementResponse);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error during {Method} request to {Route} endpoint, with body {Body}.", _method, _route, measurementRequest);
            return new StatusCodeResult(500);
        }
    }
}
