using System.Reflection;
using System.Text.Json.Nodes;
using Json.Schema;
using Orleans;

public class SchemaRegistryService: IHostedService
{
    private readonly HttpClient httpClient;
    private readonly ILogger<SchemaRegistryService> logger;
    private IClusterClient clusterClient;
    private  Task<IClusterClient> clusterClientTask;

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
    public async  Task StartAsync(CancellationToken cancellationToken)
    {
         this.clusterClientTask = StartRetryAsync(cancellationToken);
         this.clusterClientTask.ContinueWith(clusterClient => this.clusterClient = clusterClient.Result);
    }

    public async Task<IClusterClient> StartRetryAsync(CancellationToken cancellationToken)
    {
        while (cancellationToken is not {IsCancellationRequested: true})
        {
            try
            {
                return await SchemaClusterClient.ConnectAsync();
            }
            catch (Exception e)
            {
                this.logger.LogError(e, "Failed to connect to cluster");
                await Task.Delay(500, cancellationToken);

            }
        }
         throw new TaskCanceledException("Failed to connect to cluster");
    }



    /// <inheritdoc />
    public Task StopAsync(CancellationToken cancellationToken) => this.clusterClient.AbortAsync();
}
