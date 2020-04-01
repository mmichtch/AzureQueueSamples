using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

using Serilog;

namespace ConsoleQueueProcessor.Core
{
    class Program
    {
        private static ILogger<Program> _logger;
        private static IServiceProvider ServiceProvider;
        public static IConfiguration Configuration { get; set; }


        static async Task<int> Main(string[] args)
        {
            Configuration = BuildConfig();
            ServiceProvider = BuildServiceProvider(Configuration);
            _logger = ServiceProvider.GetService<ILogger<Program>>();

            _logger.LogInformation($"Main started.");

            await Process();

            _logger.LogInformation("Main Completed");

            return 0;
        }

        private static async Task Process()
        {
            using var scope = ServiceProvider.CreateScope();
            var processor = scope.ServiceProvider.GetService<IQueueProcessor>();
            await processor.ProcessAsync();
        }

        private static IConfiguration BuildConfig()
        {
            var cmdArgs = Environment.GetCommandLineArgs();
            string exePath = cmdArgs[0];
            string appRoot = Path.GetDirectoryName(exePath);

            var builder = new ConfigurationBuilder()
                .SetBasePath(appRoot)
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true);


            var switchMapping = new Dictionary<string, string> {
                { "-Log", "Serilog:MinimumLevel:Override:ConsoleQueueProcessor.Core"},
                { "-L", "Serilog:MinimumLevel:Override:ConsoleQueueProcessor.Core"},
            };

            // First parameter (if not key/value pair) is always environment
            if (cmdArgs.Length > 1 && !cmdArgs[1].Contains("="))
                builder.AddJsonFile($"appsettings.{cmdArgs[1]}.json", optional: true, reloadOnChange: true);

            builder
                .AddUserSecrets<Program>()
                .AddCommandLine(cmdArgs, switchMapping)
                .AddEnvironmentVariables();


            return builder.Build();
        }

        private static IServiceProvider BuildServiceProvider(IConfiguration configuration)
        {
            var logger = new LoggerConfiguration()
                .ReadFrom.Configuration(configuration, "Serilog")
                .CreateLogger();

            var services = new ServiceCollection();
            services.AddLogging(configure => {
                configure.AddSerilog(logger, true);
            });


            services.AddScoped<IQueueProcessor, QueueProcessor>();

            services.AddSingleton(serviceProvider => DocumentMessageQueue.CreateQueue(Configuration.GetConnectionString("AzureWebJobsStorage"), Configuration.GetValue<string>("DocumentQueue")));

            return services.BuildServiceProvider();
        }

    }
}
