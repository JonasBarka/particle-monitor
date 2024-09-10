using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ParticleMonitor.Functions.GetMeasurements;
using ParticleMonitor.Functions.PostMeasurements;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json;

namespace ParticleMonitor;

[ExcludeFromCodeCoverage]
internal class Program
{
    private static async Task Main(string[] _)
    {
        var host = new HostBuilder()
            .ConfigureFunctionsWebApplication()
            .ConfigureServices(services =>
            {
                services.AddApplicationInsightsTelemetryWorkerService();
                services.ConfigureFunctionsApplicationInsights();
                
                services.AddSingleton<IGetMeasurementsHandler, GetMeasurementsHandler>();
                services.AddSingleton<IPostMeasurementsHandler, PostMeasurementsHandler>();
                services.AddSingleton(TimeProvider.System);

                services.AddSingleton(_ =>
                {
                    string? connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                    string tableName = "ParticleMonitor";
                    return new TableClient(connectionString, tableName);
                }); 

                // Other ways to globally set serialization options have failed.
                services.AddSingleton(new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });
                
            })
            .Build();

        await host.RunAsync();
    }
}
