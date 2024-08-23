using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using ParticleMonitor.Entities;
using System.Net;

namespace ParticleMonitor.Functions.GetMeasurements;

public class GetMeasurements(TableClient tableClient, ILogger<GetMeasurements> logger)
{
    const string _method = "get";
    const string _route = "measurements";

    [Function(nameof(GetMeasurements))]
    [OpenApiOperation(operationId: "GetMeasurements", tags: ["Measurements"], Summary = "Get measurements",
        Description = "Retrieves measurements for a given device and date.")]
    [OpenApiParameter(name: "deviceId", In = ParameterLocation.Query, Required = true, Type = typeof(int),
        Description = "Device ID for the monitor, for which to retrive measurements.")]
    [OpenApiParameter(name: "dateUTC", In = ParameterLocation.Query, Required = true, Type = typeof(string),
        Description = "UTC date in format yyyy-MM-dd, for which to retrive measurements.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(List<GetMeasurementsResponse>),
        Description = "OK response message containing a JSON result with a collection of measurements for the specified device and date.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json", bodyType: typeof(string),
        Description = "Bad request response message with a descriptions of the problem with the request.")]
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

        var partitionKey = deviceId + "_" + dateUTC;

        var getMeasurementsResponses = new List<GetMeasurementsResponse>();
        try
        {
            var queryResult = tableClient.QueryAsync<Measurement>(filter: $"PartitionKey eq '{partitionKey}'");

            await foreach (var entity in queryResult)
            {
                getMeasurementsResponses.Add(GetMeasurementsResponse.CreateFromMeasurement(entity));
            }
        }
        catch (RequestFailedException ex)
        {
            logger.LogError(ex, "Measurements for partition key {partitionKey} could not be retrieved.", partitionKey);
            return new ObjectResult("An error occurred while trying to store retrive the measurements.")
            {
                StatusCode = StatusCodes.Status500InternalServerError
            };
        }
        logger.LogInformation("Returning result for partition key {PartitionKey}.", partitionKey);
        return new OkObjectResult(getMeasurementsResponses);
    }
}
