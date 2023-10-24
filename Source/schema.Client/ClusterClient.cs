using Orleans;
using Orleans.Configuration;
using Orleans.Hosting;
using Orleans.Runtime;
using schema.Abstractions.Constants;
using schema.Abstractions.Grains;
using schema.Client;

public class SchemaClusterClient
{
    public static async Task<IClusterClient> ConnectAsync()
    {
        var clusterClient = CreateClientBuilder().Build();
        await clusterClient.Connect().ConfigureAwait(false);

        // Set a trace ID, so that requests can be identified.
        RequestContext.Set("TraceId", Guid.NewGuid());


        return clusterClient;
    }

    private static IClientBuilder CreateClientBuilder() =>
        new ClientBuilder()
            .UseLocalhostClustering(EndpointOptions.DEFAULT_GATEWAY_PORT)
            .Configure<ClusterOptions>(
                options =>
                {
                    options.ClusterId = Cluster.ClusterId;
                    options.ServiceId = Cluster.ServiceId;
                })
            .ConfigureApplicationParts(
                parts => parts
                    .AddApplicationPart(typeof(ISchemaGrain).Assembly)
                    .WithReferences())
            .AddSimpleMessageStreamProvider(StreamProviderName.Default);


}



