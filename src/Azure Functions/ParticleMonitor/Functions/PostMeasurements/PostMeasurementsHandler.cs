using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Logging;

namespace ParticleMonitor.Functions.PostMeasurements;

public interface IPostMeasurementsHandler
{
    Task<PostMeasurementsResponse> HandleAsync(PostMeasurementsRequest postMeasurementsRequest);
}

public class PostMeasurementsHandler(TableClient tableClient, TimeProvider timeProvider, ILogger<PostMeasurementsHandler> logger) : IPostMeasurementsHandler
{
    public async Task<PostMeasurementsResponse> HandleAsync(PostMeasurementsRequest postMeasurementsRequest)
    {
        var measurement = postMeasurementsRequest.ToMeasurement(DateWithoutMilliseconds(), Guid.NewGuid());

        Response? response;
        try
        {
            response = await tableClient.AddEntityAsync(measurement);
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "Measurement {Measurement} could not be stored.", measurement);
            throw;
        }

        if (response == null)
        {
            logger.LogError("Measurement {Measurement} could not be stored. AddEntityAsync returned null.", measurement);
            throw new RequestFailedException($"Measurement {measurement} could not be stored. AddEntityAsync returned null.");
        }
        else if (response.IsError)
        {
            logger.LogError("Measurement {Measurement} could not be stored. Status code: {StatusCode}, Reason: {ReasonPhrase}", measurement, response.Status, response.ReasonPhrase);
            throw new RequestFailedException($"Measurement {measurement} could not be stored. Status code: {response.Status}, Reason: {response.ReasonPhrase}");
        }

        return PostMeasurementsResponse.CreateFromMeasurement(measurement);
    }

    private DateTimeOffset DateWithoutMilliseconds()
    {
        DateTimeOffset dateTime = timeProvider.GetUtcNow();

        return new DateTimeOffset(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Offset);
    }
}
