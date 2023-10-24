using System.Text.Json.Nodes;
using Json.Schema;
using Orleans;
using schema.web.Controllers;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllersWithViews();
builder.Services.AddHttpClient<SchemaRegistryService>();
builder.Services.AddSingleton<SchemaRegistryService>();
builder.Services.AddHostedService<SchemaRegistryService>(services => services.GetRequiredService<SchemaRegistryService>());

builder.Services.AddLogging(logger=>logger.AddConsole());
var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

// if (app.Environment.IsDevelopment())
// {
//     app.UseSpa(spaBuilder =>
//     {
//         spaBuilder.Options.PackageManagerCommand = "yarn";
//         spaBuilder.Options.SourcePath = Path.GetFullPath("./RemixApp");
//
//         spaBuilder.UseReactDevelopmentServer("dev");
//     });
// }

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();


app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

app.AddSchemaApi();
app.Run();

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
