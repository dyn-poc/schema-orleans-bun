using System.Text.Json.Nodes;
using Json.Schema;
using Orleans;

public class SchemaRegistryService: IHostedService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<SchemaRegistryService> logger;
    private IClusterClient clusterClient;

    public SchemaRegistryService(HttpClient httpClient, ILogger<SchemaRegistryService> logger)
    {
        this.httpClient = httpClient;
        this.logger = logger;
        SchemaRegistry.Global.Fetch = this.DownloadSchema;
    }

    private IBaseDocument? DownloadSchema(Uri uri)
    {
        this.logger.LogWarning("Hit Registry {uri}", uri);
        var myStream = this.httpClient.Send(new HttpRequestMessage(HttpMethod.Get, uri)).Content.ReadAsStream();

        return JsonSchema.FromText(JsonNode.Parse(myStream)!.ToJsonString()!);
    }

    public IClusterClient ClusterClient => this.clusterClient;



    /// <inheritdoc />
    public async Task StartAsync(CancellationToken cancellationToken) => this.clusterClient = await SchemaClusterClient.ConnectAsync();



    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => this.clusterClient.AbortAsync();
}
