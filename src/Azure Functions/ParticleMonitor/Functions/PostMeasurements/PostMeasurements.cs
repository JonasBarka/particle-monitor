using Azure;
using Azure.Data.Tables;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using System.Net;
using System.Text.Json;

namespace ParticleMonitor.Functions.PostMeasurements;

public class PostMeasurements(IPostMeasurementsHandler postMeasurementsHandler, JsonSerializerOptions JsonSerializerOptions, ILogger<PostMeasurements> logger)
{
    const string _method = "post";
    const string _route = "measurements";

    [Function(nameof(PostMeasurements))]
    [OpenApiOperation(operationId: "PostMeasurements", tags: ["Measurements"], Summary = "Stores a measurement.",
        Description = "Stores a measurement recently collected by a Particle Monitor device.")]
    [OpenApiRequestBody("application/json", typeof(PostMeasurementsRequest),
        Description = "JSON request body containing the id for the device, followed by detected PM (Particulate Matter) values for PM1.0, PM2.5 and PM10.0.", Required = true)]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(PostMeasurementsResponse),
        Description = "OK response message containing a JSON result with the supplied request and corresponding date and time in UTC.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, "application/json", bodyType: typeof(string),
        Description = "Bad request response message with a description of the problem with the request body.")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, _method, Route = _route)] HttpRequest req)
    {
        var deserializeResult = await DeserializeAsync(req.Body);

        if (deserializeResult.PostMeasurementsRequest == null)
        {
            return new BadRequestObjectResult(deserializeResult.ErrorMessage);
        }

        logger.LogInformation("Received {Method} request to {Route} endpoint, with body {Body}.", _method, _route, deserializeResult.PostMeasurementsRequest);

        var postMeasurementResponse = await postMeasurementsHandler.HandleAsync(deserializeResult.PostMeasurementsRequest);

        return new OkObjectResult(postMeasurementResponse);
    }

    private async Task<(PostMeasurementsRequest? PostMeasurementsRequest, string ErrorMessage)> DeserializeAsync(Stream json)
    {
        try
        {
            return (await JsonSerializer.DeserializeAsync<PostMeasurementsRequest>(json, JsonSerializerOptions), "");
        }
        catch (JsonException)
        {
            logger.LogWarning("Received {Method} request to {Route} endpoint with an invalid body.", _method, _route);
            return (null, "Invalid request body.");
        }
    }
}
