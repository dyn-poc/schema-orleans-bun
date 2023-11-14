namespace schema.Server;

using System.Runtime.InteropServices;
using Abstractions.Constants;
using Abstractions.Grains;
using Grains;
using Options;
using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using Orleans.Runtime.Development;
using Orleans.Statistics;

public static class OrleansSetup
{
    public static void ConfigureSiloBuilder(
        ISiloBuilder siloBuilder) =>
        siloBuilder
            .ConfigureServices(
                (builderContext, services) =>
                {
                    services.Configure<ApplicationOptions>(builderContext.Configuration);
                    services.Configure<ClusterOptions>(
                        builderContext.Configuration.GetSection(nameof(ApplicationOptions.Cluster)));
                    services.Configure<StorageOptions>(
                        builderContext.Configuration.GetSection(nameof(ApplicationOptions.Storage)));
                    services.AddHttpClient()
                        .AddHttpClient<ISchemaRegistryGrain>();
                    services.AddHttpClient();
                    services.AddLogging(logger =>
                    {
                        logger.AddConsole();
                        logger.AddFilter((string catagory, LogLevel level) =>
                        {
                            if (
                                catagory.Contains(".Orleans")
                                || catagory.Contains(".Microsoft.Hosting"))
                            {
                                return level >= LogLevel.Warning;
                            }
                            else
                            {
                                return level >= LogLevel.Information;
                            }
                        });
                        // TypeNameHelper.GetTypeDisplayName(typeof (T), includeGenericParameters: false, nestedTypeDelimiter: '.')
                    });
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


}
