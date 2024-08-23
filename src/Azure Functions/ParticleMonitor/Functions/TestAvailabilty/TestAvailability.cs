using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Extensions.Logging;
using System.Net;

namespace ParticleMonitor.Functions.TestAvailabilty;

public class TestAvailability(ILogger<TestAvailability> logger)
{
    [Function("testavailability")]
    [OpenApiOperation(operationId: "TestAvailability", tags: ["Test availabilty"], Summary = "Test availability",
        Description = "Test if the server is ready to respond to requests.")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, "application/json", bodyType: typeof(string),
        Description = """Should respond with "Particle Monitor server is available." """)]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        logger.LogInformation("TestAvailiability HTTP trigger function processed a request.");
        return new OkObjectResult("Particle Monitor server is available.");
    }
}
