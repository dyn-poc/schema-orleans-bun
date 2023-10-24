namespace schema.Server;

using System.Runtime.InteropServices;
using Json.Schema;
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
            .UseConsoleLifetime();

    private static void ConfigureServices(HostBuilderContext context, IServiceCollection services)
    {
        // services.AddSingleton<IStorage<JsonSchema>, SchemaStorage>();

    }

    private static void ConfigureSiloBuilder(
        Microsoft.Extensions.Hosting.HostBuilderContext context,
        ISiloBuilder siloBuilder) =>
        siloBuilder
            .ConfigureServices(
                (context, services) =>
                {
                    services.Configure<ApplicationOptions>(context.Configuration);
                    services.Configure<ClusterOptions>(
                        context.Configuration.GetSection(nameof(ApplicationOptions.Cluster)));
                    services.Configure<StorageOptions>(
                        context.Configuration.GetSection(nameof(ApplicationOptions.Storage)));
                })
            .UseSiloUnobservedExceptionsHandler()
            .UseLocalhostClustering(EndpointOptions.DEFAULT_SILO_PORT, EndpointOptions.DEFAULT_GATEWAY_PORT)
            .ConfigureApplicationParts(parts => parts.AddApplicationPart(typeof(HelloGrain).Assembly).WithReferences())
            .UseInMemoryReminderService()
            .UseInMemoryLeaseProvider()
            .UseTransactions(withStatisticsReporter: true)
            .AddMemoryGrainStorageAsDefault()
            .AddMemoryGrainStorage("PubSubStore")
             .AddMemoryGrainStorage("schema_store")

            .AddSimpleMessageStreamProvider(StreamProviderName.Default)
            .UseIf(
                RuntimeInformation.IsOSPlatform(OSPlatform.Linux),
                x => x.UseLinuxEnvironmentStatistics())
            .UseIf(
                RuntimeInformation.IsOSPlatform(OSPlatform.Windows),
                x => x.UsePerfCounterEnvironmentStatistics())
            .UseDashboard();

    private static void ConfigureWebHostBuilder(IWebHostBuilder webHostBuilder) =>
        webHostBuilder
            .UseKestrel(
                (builderContext, options) =>
                {
                    options.AddServerHeader = false;
                    options.Configure(
                        builderContext.Configuration.GetSection(nameof(ApplicationOptions.Kestrel)),
                        reloadOnChange: false);
                })
            .UseStartup<Startup>();


    private static void ConfigureJsonSerializerSettings(JsonSerializerSettings jsonSerializerSettings)
    {
        jsonSerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
        jsonSerializerSettings.DateParseHandling = DateParseHandling.DateTimeOffset;
    }

    private static StorageOptions GetStorageOptions(IConfiguration configuration) =>
        configuration.GetSection(nameof(ApplicationOptions.Storage)).Get<StorageOptions>();
}
