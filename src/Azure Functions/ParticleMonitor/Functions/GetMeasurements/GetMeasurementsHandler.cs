using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using ParticleMonitor.Entities;

namespace ParticleMonitor.Functions.GetMeasurements;

public interface IGetMeasurementsHandler
{
    Task<List<GetMeasurementsResponse>> HandleAsync(string deviceId, string dateUtc);
}

public class GetMeasurementsHandler(TableClient tableClient, ILogger<GetMeasurementsHandler> logger) : IGetMeasurementsHandler
{
    public async Task<List<GetMeasurementsResponse>> HandleAsync(string deviceId, string dateUtc)
    {
        var partitionKey = deviceId + "_" + dateUtc;

        var getMeasurementsResponses = new List<GetMeasurementsResponse>();
        try
        {
            var queryResult = tableClient.QueryAsync<Measurement>(filter: $"PartitionKey eq '{partitionKey}'");

            await foreach (var entity in queryResult)
            {
                getMeasurementsResponses.Add(GetMeasurementsResponse.CreateFromMeasurement(entity));
            }
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Measurements for partition key {partitionKey} could not be retrieved from table.", partitionKey);
            throw;
        }
        
        return getMeasurementsResponses;
    }
}
