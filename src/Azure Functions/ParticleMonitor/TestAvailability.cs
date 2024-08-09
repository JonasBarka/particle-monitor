using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace ParticleMonitorFunctions;

public class TestAvailability(ILogger<TestAvailability> logger)
{
    [Function("testavailability")]
    public IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequest req)
    {
        logger.LogInformation("TestAvailiability HTTP trigger function processed a request.");
        return new OkObjectResult("Particle Monitor server is available.");
    }
}
