using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

using Serilog;

using System;
using System.Threading.Tasks;

namespace HostedQueueProcessor.Core
{
    class Program
    {
        static void Main(/*string[] args*/)
        {
            string ASPNETCORE_ENVIRONMENT = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT");

            var builder = new HostBuilder();
            builder
                .UseEnvironment(ASPNETCORE_ENVIRONMENT)
                .ConfigureWebJobs((context, builder) =>
                {
                    builder.AddAzureStorageCoreServices();
                    builder.AddAzureStorage();
                })
                .ConfigureAppConfiguration((context, config) =>
                {
                    // Host builder framework adds appsettings.json and evironment variables automatically
                    // it does not support appsettings.<environment>.json by default

                    // For development evironment only add secrets file                    
                    if (context.HostingEnvironment.IsDevelopment())
                    {
                        config.AddUserSecrets<Program>();
                    }
                })
                // to add application insight:
                // https://github.com/serilog/serilog-sinks-applicationinsights
                .UseSerilog((context, loggerConfiguration) =>
                {
                    loggerConfiguration
                        .ReadFrom.Configuration(context.Configuration)
                        .Enrich.FromLogContext();
                })
                .ConfigureServices((context, services) =>
                {
                    services.AddSingleton<INameResolver, ConfigValuesResolver>();
                    services.AddSingleton<QueueCollection>();
                })
                ;

            var host = builder.Build();

            host.Run();
        }
    }
}
