//convert bellow to minimal API
//https://docs.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis?view=aspnetcore-6.0

using Boxed.AspNetCore;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.HttpLogging;
using Orleans.Configuration;
using schema.Abstractions.Grains;
using schema.Grains;
using schema.Server;
using schema.Server.Api;
using schema.Server.HealthChecks;
using schema.Server.Options;

var builder = WebApplication.CreateBuilder(args);

//add settings
builder.Configuration.AddJsonFile("appsettings.json");
// add environment settings
builder.Configuration.AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json");

builder.Services.Configure<ApplicationOptions>(builder.Configuration);
builder.Services.Configure<ClusterOptions>(
    builder.Configuration.GetSection(nameof(ApplicationOptions.Cluster)));
builder.Services.AddRouting(options => options.LowercaseUrls = true);
builder.Services.AddCors(options => options.AddDefaultPolicy(
        policy => policy
                .AllowAnyOrigin()
                .AllowAnyMethod()
                .AllowAnyHeader()));


AddOpenApi(builder);
AddHttpClients(builder.Services);
AddHealthChecks(builder.Services);
ConfigureLogging(builder);

builder.Host.ConfigureAppConfiguration(
    (hostingContext, configurationBuilder) =>
    {
        hostingContext.HostingEnvironment.ApplicationName = AssemblyInformation.Current.Product;
        configurationBuilder.AddCustomConfiguration(hostingContext.HostingEnvironment, args);
    });


builder.Host.UseOrleans(OrleansSetup.ConfigureSiloBuilder);
builder.Host.UseContentRoot(Directory.GetCurrentDirectory());
builder.Host.UseDefaultServiceProvider(
    (context, options) =>
    {
        var isDevelopment = context.HostingEnvironment.IsDevelopment();
        options.ValidateScopes = isDevelopment;
        options.ValidateOnBuild = isDevelopment;
    });

builder.WebHost.UseStaticWebAssets();

builder.WebHost.UseKestrel(
    (builderContext, options) =>
    {
        options.AddServerHeader = false;
        options.Configure(
            builderContext.Configuration.GetSection(nameof(ApplicationOptions.Kestrel)),
            reloadOnChange: false);

    });


var app = builder.Build();
app.AddSchemaApi();
MapHealth(app);
UseOpenApi(app);

app.UseStaticFiles();
app.UseRouting();
app.UseCors();


app.LogApplicationStarted();
Console.WriteLine(string.Join(Environment.NewLine,app.Urls));
await app.RunAsync().ConfigureAwait(false);

app.LogApplicationStopped();




void ConfigureLogging(WebApplicationBuilder builder)
{
    builder.Services.Configure<LoggerFilterOptions>(builder.Configuration.GetSection("Logging"));
    builder.Logging.AddConfiguration(builder.Configuration.GetSection("Logging"));
    builder.Services.AddLogging(logger =>
    {

        logger.AddConsole();
        logger.AddFilter((string catagory, LogLevel level) =>
        {
            if (
                catagory.Contains("Orleans")
                || catagory.Contains("GrainDirectory")
                || catagory.Contains("Messaging.") // || catagory.Contains("Microsoft.Hosting")
            )
            {
                return level >= LogLevel.Warning;
            }
            else
            {
                return level >= LogLevel.Information;
            }
        });
        // TypeNameHelper.GetTypeDisplayName(typeof (T), includeGenericParameters: false, nestedTypeDelimiter: '.')
    })
        .AddHttpLogging(http => http.LoggingFields = HttpLoggingFields.All);;
}

void AddHealthChecks(IServiceCollection serviceCollection) => serviceCollection.AddHealthChecks()
        .AddCheck<ClusterHealthCheck>(nameof(ClusterHealthCheck))
        .AddCheck<GrainHealthCheck>(nameof(GrainHealthCheck))
        .AddCheck<SiloHealthCheck>(nameof(SiloHealthCheck))
        .AddCheck<StorageHealthCheck>(nameof(StorageHealthCheck));

void AddHttpClients(IServiceCollection serviceCollection)
{
    serviceCollection
        .AddHttpClient()
        .AddHttpClient<MockSchemaGrain>();
    serviceCollection.AddHttpClient()
        .AddHttpClient<ISchemaRegistryGrain>();
    serviceCollection.AddHttpClient();
}

void MapHealth(IEndpointRouteBuilder endpointRouteBuilder)
{
    endpointRouteBuilder.MapHealthChecks("/status");
    endpointRouteBuilder.MapHealthChecks("/status/self", new HealthCheckOptions() { Predicate = _ => false });
    endpointRouteBuilder.MapSwagger();
}

void AddOpenApi(WebApplicationBuilder webApplicationBuilder)
{
    webApplicationBuilder.Services.AddEndpointsApiExplorer();
    webApplicationBuilder.Services
        .AddSwaggerGen(c =>
            c.SwaggerDoc("v1", new()
            {
                Title = webApplicationBuilder.Environment.ApplicationName,
                Version = "v1"
            }));
}

void UseOpenApi(WebApplication webApplication)
{
    webApplication.UseSwagger();
    webApplication.UseSwaggerUI(c => c.SwaggerEndpoint("v1/swagger.json", "My API V1"));
}


/*
 *builder.WebHost.ConfigureAppConfiguration(
   (hostingContext, configurationBuilder) =>
   {
   hostingContext.HostingEnvironment.ApplicationName = AssemblyInformation.Current.Product;
   configurationBuilder.AddCustomConfiguration(hostingContext.HostingEnvironment, args);
   });
 * builder.WebHost.UseContentRoot(Directory.GetCurrentDirectory());
   builder.WebHost.UseDefaultServiceProvider(
   (context, options) =>
   {
   var isDevelopment = context.HostingEnvironment.IsDevelopment();
   options.ValidateScopes = isDevelopment;
   options.ValidateOnBuild = isDevelopment;
   });

 */
/*
using System.Runtime.InteropServices;
using Abstractions.Grains;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Orleans;
using Orleans.Configuration;
using Orleans.Core;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Runtime.Development;
using Orleans.Statistics;
using schema.Abstractions.Constants;
using schema.Grains;
using schema.Server.Options;
using HostBuilderContext = Microsoft.Extensions.Hosting.HostBuilderContext;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        IHost? host = null;

        try
        {
            host = CreateHostBuilder(args).Build();
            host.LogApplicationStarted();
            await host.RunAsync().ConfigureAwait(false);
            host.LogApplicationStopped();

            return 0;
        }
#pragma warning disable CA1031 // Do not catch general exception types
        catch (Exception exception)
#pragma warning restore CA1031 // Do not catch general exception types
        {
            host!.LogApplicationTerminatedUnexpectedly(exception);

            return 1;
        }
    }

    private static IHostBuilder CreateHostBuilder(string[] args) =>
        new HostBuilder()
            .UseContentRoot(Directory.GetCurrentDirectory())
            .ConfigureHostConfiguration(
                configurationBuilder => configurationBuilder.AddCustomBootstrapConfiguration(args))
            .ConfigureAppConfiguration(
                (hostingContext, configurationBuilder) =>
                {
                    hostingContext.HostingEnvironment.ApplicationName = AssemblyInformation.Current.Product;
                    configurationBuilder.AddCustomConfiguration(hostingContext.HostingEnvironment, args);
                })
            .UseDefaultServiceProvider(
                (context, options) =>
                {
                    var isDevelopment = context.HostingEnvironment.IsDevelopment();
                    options.ValidateScopes = isDevelopment;
                    options.ValidateOnBuild = isDevelopment;
                })
            .UseOrleans(ConfigureSiloBuilder)
            .ConfigureServices(ConfigureServices)
            .ConfigureWebHost(ConfigureWebHostBuilder)
            .UseConsoleLifetime()
        ;

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // services.AddSingleton<IStorage<JsonSchema>, SchemaStorage>();

    }


    private static void ConfigureWebHostBuilder(IWebHostBuilder webHostBuilder) =>
        webHostBuilder
            .UseStaticWebAssets()

            .UseKestrel(
                (builderContext, options) =>
                {
                     options.AddServerHeader = false;
                    options.Configure(
                        builderContext.Configuration.GetSection(nameof(ApplicationOptions.Kestrel)),
                        reloadOnChange: false);
                    // options.ConfigureEndpointDefaults(
                    //     listenOptions =>
                    //     {
                    //         listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                    //         listenOptions.UseConnectionLogging();
                    //
                    //     });



                })
            .UseStartup<Startup>()
            ;


    private static void ConfigureJsonSerializerSettings(JsonSerializerSettings jsonSerializerSettings)
    {
        jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        jsonSerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
    }

    private static StorageOptions GetStorageOptions(IConfiguration configuration) =>
        configuration.GetSection(nameof(ApplicationOptions.Storage)).Get<StorageOptions>();
}
*/
