using System.Net.Http.Headers;
using System.Threading;
using ATI.Gaidai.Entities;
using ATI.Gaidai.Helpers;
using ATI.Services.Common.Behaviors;
using ATI.Services.Common.Extensions;
using ATI.Services.Common.Logging;
using ATI.Services.Common.Metrics;
using ATI.Services.Consul;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NLog;
using Winton.Extensions.Configuration.Consul;

namespace ATI.Gaidai
{
    public class Startup
    {
        public Startup(IWebHostEnvironment env)
        {
            TokenSource = new CancellationTokenSource();
            LogHelper.ConfigureNlog(env);
            Logger = LogManager.GetCurrentClassLogger();

            ConfigurationManager.ConfigurationRoot = Configuration = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath)
                .AddJsonFile("appsettings.json", true, true)
                .AddJsonFile($"appsettings.{env.EnvironmentName}.json", true, true)
                .AddJsonFile("ScenarioSettings/scenariossettings.json", true, true)
                .AddJsonFile($"ScenarioSettings/scenariossettings.{env.EnvironmentName}.json", true, true)
                .AddJsonFile("ScenarioSettings/methodssettings.json", true, true)
                .AddJsonFile($"ScenarioSettings/methodssettings.{env.EnvironmentName}.json", true, true)
                .AddJsonFile("ScenarioSettings/servicessettings.json", true, true)
                .AddJsonFile($"ScenarioSettings/servicessettings.{env.EnvironmentName}.json", true, true)
                .AddConsul("gaidai/scenariossettings", TokenSource.Token, ConfigureConsulOptions)
                .AddConsul("gaidai/methodssettings", TokenSource.Token, ConfigureConsulOptions)
                .AddConsul("gaidai/servicessettings", TokenSource.Token, ConfigureConsulOptions)
                .Build();
        }
        private ILogger Logger { get; }
        private CancellationTokenSource TokenSource { get; }
        private IConfigurationRoot Configuration { get; }
        private void ConfigureConsulOptions(IConsulConfigurationSource options)
        {
            options.Optional = true;
            options.ReloadOnChange = true;
            options.OnLoadException = exceptionContext =>
            {
                exceptionContext.Ignore = true;
                Logger.Error(exceptionContext.Exception);
            };
        }
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services
                .AddControllers(options =>
                {
                    options.AllowEmptyInputInBodyModelBinding = true;
                    options.SuppressInputFormatterBuffering = true;
                    options.SuppressOutputFormatterBuffering = true;
                })
                .AddNewtonsoftJson(options =>
                {
                    options.SerializerSettings.ContractResolver = new DefaultContractResolver
                    {
                        NamingStrategy = new SnakeCaseNamingStrategy
                        {
                            ProcessDictionaryKeys = true,
                            OverrideSpecifiedNames = true
                        }
                    };
                    options.SerializerSettings.NullValueHandling = NullValueHandling.Ignore;
                    options.SerializerSettings.DateFormatString = "dd.MM.yyyy";
                });

            services.AddSingleton<ParametersHelper>();
            services.AddSingleton<ScenarioHelper>();
            services.AddSingleton<HeadersHelper>();
            services.AddSingleton<HttpRequestHelper>();
            services.AddSingleton<CookiesHelper>();

            services.AddSingleton<GaidaiConfigValidator>();

            services.Configure<ScenarioOptions>(Configuration);
            services.AddConsul();
            services.AddInitializers();

            services.AddHttpClient<ScenarioHelper>(client =>
                client.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json")));


        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseExceptionHandler(new ExceptionHandlerOptions
            {
                ExceptionHandler = async context =>
                {
                    var response = context.Response;
                    response.ContentType = "application/json; charset=utf-8";
                    await response.WriteAsync(JsonConvert.SerializeObject(ApiMessages.InternalServerError));
                }
            });

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapConsulDeregistration();
                endpoints.MapMetricsCollection();
            });

        }
    }
}