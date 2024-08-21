﻿using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using System.Text.Json;

namespace ParticleMonitor.Functions;

public class PostMeasurements(TableClient tableClient, TimeProvider timeProvider, JsonSerializerOptions JsonSerializerOptions, ILogger<PostMeasurements> logger)
{
    const string _method = "post";
    const string _route = "measurements";

    [Function(nameof(PostMeasurements))]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, _method, Route = _route)] HttpRequest req)
    {
        MeasurementsRequest? measurementRequest;
        try
        {
            measurementRequest = await JsonSerializer.DeserializeAsync<MeasurementsRequest>(req.Body, JsonSerializerOptions);
        }
        catch (JsonException ex)
        {
            logger.LogWarning(ex, "Received {Method} request to {Route} endpoint with an invalid body.", _method, _route);
            return new BadRequestObjectResult("Invalid request body.");
        }

        // Testing has not been able to produce this result regardless of input.
        if (measurementRequest == null)
        {
            logger.LogWarning("Unexpected null result when deserialising body for {Method} request to {Route} endpoint.", _method, _route);
            return new BadRequestObjectResult("Invalid request body.");
        }

        logger.LogInformation("Received {Method} request to {Route} endpoint, with body {Body}.", _method, _route, measurementRequest);

        var measurement = measurementRequest.ToMeasurement(DateWithoutMilliseconds(), Guid.NewGuid());

        try
        {
            await tableClient.AddEntityAsync(measurement);   
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "Measurement {Measurement} could not be stored.", measurement);
            return new ObjectResult("An error ocurred while trying to store the measurement.")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }

        var measurementResponse = MeasurementsResponse.CreateFromMeasurement(measurement);

        return new OkObjectResult(measurementResponse);
    }

    private DateTimeOffset DateWithoutMilliseconds()
    {
        DateTimeOffset dateTime = timeProvider.GetUtcNow();

        return new DateTimeOffset(dateTime.Year, dateTime.Month, dateTime.Day, dateTime.Hour, dateTime.Minute, dateTime.Second, dateTime.Offset);
    }
}
