using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Diagnostics.CodeAnalysis;

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
            services.AddSingleton(sp =>
            {
                string? connectionString = Environment.GetEnvironmentVariable("AzureWebJobsStorage");
                string tableName = "ParticleMonitor";
                return new TableClient(connectionString, tableName);
            });
            services.AddSingleton(TimeProvider.System);
        })
        .Build();

        await host.RunAsync();
    }
}