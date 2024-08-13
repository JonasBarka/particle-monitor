using Azure.Data.Tables;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

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
    })
    .Build();

host.Run();
