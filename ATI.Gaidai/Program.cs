using System;
using System.Net;
using System.Threading.Tasks;
using ATI.Services.Common.Behaviors;
using ATI.Services.Common.Initializers;
using ATI.Services.Consul;
using Microsoft.AspNetCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using NLog.Web;

namespace ATI.Gaidai
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var webHost = new HostBuilder()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder
                        .UseStartup<Startup>()
                        .UseKestrel((context, configuration) =>
                        {
                            configuration.AllowSynchronousIO = true;
                            configuration.Listen(IPAddress.Any, ConfigurationManager.GetApplicationPort());
                        })                        
                        .ConfigureLogging(logging =>
                        {
                            logging.ClearProviders();
                            logging.SetMinimumLevel(LogLevel.Warning);
                            logging.AddConsole();
                        })
                        .UseNLog();
                })
                .Build();

            using (var scope = webHost.Services.CreateScope())
            {
                var serviceProvider = scope.ServiceProvider;

                await serviceProvider.GetRequiredService<StartupInitializer>().InitializeAsync();

                var scenarioConfigValidator = serviceProvider.GetRequiredService<GaidaiConfigValidator>();
                var validateResult = scenarioConfigValidator.ValidateScenarioConfiguration();
                if (!validateResult.Success)
                {
                    Console.WriteLine(validateResult.DumpAllErrors());
                }
            }


            await webHost.RunAsync();
        }
    }
}
