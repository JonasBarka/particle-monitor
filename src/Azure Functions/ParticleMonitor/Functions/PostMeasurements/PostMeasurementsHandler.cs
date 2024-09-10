using Azure.Data.Tables;
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using ParticleMonitor.Functions.GetMeasurements;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;

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

        try
        {
            await tableClient.AddEntityAsync(measurement);
        }
        catch (RequestFailedException ex)
        {
            //logger.LogError("Measurement {Measurement} could not be stored.", measurement);
            logger.LogError(ex, "Measurement {Measurement} could not be stored.", measurement);
            throw;
        }

        return PostMeasurementsResponse.CreateFromMeasurement(measurement);
    }

    private DateTimeOffset DateWithoutMilliseconds()
    {
        DateTimeOffset dateTime = timeProvider.GetUtcNow();

        return new DateTimeOffset(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Offset);
    }
}
