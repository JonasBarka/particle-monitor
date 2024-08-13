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
        string partitionKey
        )
    {
        logger.LogInformation("Received {Method} request to {Route} endpoint.", _method, _route);

        if (string.IsNullOrEmpty(partitionKey))
        {
            logger.LogInformation("No partition key in request.");
            return new BadRequestObjectResult("PartitionKey query parameter is required.");
        }

        try
        {
            var queryResult = tableClient.QueryAsync<Measurement>(filter: $"PartitionKey eq '{partitionKey}'");
            
            var entities = new List<Measurement>();
            await foreach (var entity in queryResult)
            {
                entities.Add(entity);
            }

            logger.LogInformation("Returning result for partition key {PartitionKey}.", partitionKey);
            return new OkObjectResult(entities);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error occurred while querying entities for {PartitionKey}.", partitionKey);
            return new StatusCodeResult(500);
        }
    }
}
