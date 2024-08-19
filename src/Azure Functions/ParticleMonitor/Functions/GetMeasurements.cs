using Azure.Data.Tables;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using ParticleMonitor.Entities;

namespace ParticleMonitor.Functions;

public class GetMeasurements(TableClient tableClient, ILogger<GetMeasurements> logger)
{
    const string _method = "get";
    const string _route = "measurements";

    [Function(nameof(GetMeasurements))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, _method, Route = _route)] HttpRequestData req,
        string deviceId,
        string dateUTC
        )
    {
        logger.LogInformation("Received {Method} request to {Route} endpoint with query parameters deviceId: {deviceId} and dateUTC: {dateUTC}.", _method, _route, deviceId, dateUTC);

        if (string.IsNullOrEmpty(deviceId))
        {
            logger.LogInformation("No deviceId in request.");
            return new BadRequestObjectResult("DeviceId query parameter is required.");
        }

        if (int.TryParse(deviceId, out _) == false)
        {
            logger.LogInformation("Invalid query parameter deviceId: {InvalidQueryParameter} in request. Must be parsable as integer.", deviceId);
            return new BadRequestObjectResult("DeviceId query parameter must be an integer.");
        }

        if (string.IsNullOrEmpty(dateUTC))
        {
            logger.LogInformation("No query parameter dateUTC in request.");
            return new BadRequestObjectResult("DateUTC query parameter is required.");
        }

        if (DateOnly.TryParseExact(dateUTC, Constants.DateFormat, out _) != true)
        {
            logger.LogInformation("Invalid dateUTC query parameter: {InvalidQueryParameter} in request. Must be parsable as a {DateFormat} DateOnly.", dateUTC, Constants.DateFormat);
            return new BadRequestObjectResult($"DateUTC query parameter must be a date in the format {Constants.DateFormat}.");
        }

        var partitionKey = $"device{deviceId}_{dateUTC}";

        try
        {
            var queryResult = tableClient.QueryAsync<Measurement>(filter: $"PartitionKey eq '{partitionKey}'");
            
            var measurementsResponses = new List<MeasurementsResponse>();
            await foreach (var entity in queryResult)
            {
                measurementsResponses.Add(MeasurementsResponse.CreateFromMeasurement(entity));
            }

            logger.LogInformation("Returning result for partition key {PartitionKey}.", partitionKey);
            return new OkObjectResult(measurementsResponses);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while querying entities for partition key {PartitionKey}.", partitionKey);
            return new StatusCodeResult(500);
        }
    }
}
