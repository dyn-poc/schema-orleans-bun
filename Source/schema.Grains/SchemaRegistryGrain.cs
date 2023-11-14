namespace schema.Grains;

using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.Schema;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using schema.Abstractions.Grains;
using JsonSerializer = System.Text.Json.JsonSerializer;

[StatelessWorker]
public class SchemaRegistryRegistryGrain : Grain, ISchemaRegistryGrain
{
     private readonly HttpClient httpClient = new();
     private readonly ILogger<SchemaRegistryRegistryGrain> logger;
    private SiteRegistry siteRegistry;
    public SchemaRegistryRegistryGrain(ILogger<SchemaRegistryRegistryGrain> logger)
    {
        this.logger = logger;
    }

    /// <inheritdoc />
    public override async Task OnActivateAsync()
    {
        await base.OnActivateAsync().ConfigureAwait(true);
        var uri = new Uri(this.GetPrimaryKeyString());

        var accountSchema =
            await this.GetAccountsSchemaAsync(uri.Segments.Last().TrimEnd('/')).ConfigureAwait(true);
        this.siteRegistry = new(uri)
        {
            ["profile"] = SchemaConvert.Convert(accountSchema!.Profile.Fields, "profile"),
            ["data"] = SchemaConvert.Convert(accountSchema!.Data.Fields, "data"),
            ["preferences"] = SchemaConvert.Convert(accountSchema!.Preferences.Fields, "preferences"),
            ["subscriptions"] = SchemaConvert.Convert(accountSchema!.Subscriptions.Fields, "subscriptions")
        };
    }


    private async Task<AccountsSchema?> GetAccountsSchemaAsync(string apiKey)
    {
        var accountSchema=await this.GetAccountsSchemaAsync(apiKey, $"https://accounts.gigya.com/accounts.getSchema?apiKey={apiKey}").ConfigureAwait(false);

        if (accountSchema?.Location is not null and var location)
        {
            accountSchema = await this.GetAccountsSchemaAsync(apiKey, new Uri($"{location}?apiKey={apiKey}").ToString())
                .ConfigureAwait(false);
        }


        return accountSchema;
    }

    private async Task<AccountsSchema?> GetAccountsSchemaAsync(string apiKey, string location)
    {

        this.logger.LogWarning("get accounts schema for api {api} {uri}", apiKey, location);

        var accountSchema = await this.httpClient.GetFromJsonAsync<AccountsSchema>(
            location, new JsonSerializerOptions()
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                IgnoreReadOnlyProperties = true,
                IncludeFields = true
            }).ConfigureAwait(false);

        this.logger.LogInformation("get schema result {schema}",
            JsonSerializer.Serialize(accountSchema,
                new JsonSerializerOptions()
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                    PropertyNameCaseInsensitive = true,
                    AllowTrailingCommas = true,
                    WriteIndented = true,
                    IncludeFields = true
                }));
        return accountSchema;
    }

    public Task<JsonSchema> GetSchemaAsync(string type) =>  Task.FromResult(this.siteRegistry[type].Bundle());

    public Task<SiteRegistry> GetRegistryAsync() => Task.FromResult(this.siteRegistry);

}



public class AccountsSchema
{

    [JsonPropertyName("profileSchema")]
    public Schema Profile { get; set; }

    [JsonPropertyName("dataSchema")]
    public Schema Data { get; set; }

    [JsonPropertyName("subscriptionsSchema")]
    public Schema Subscriptions { get; set; }

    [JsonPropertyName("preferencesSchema")]
    public Schema Preferences { get; set; }

    [JsonExtensionData]

    public JsonObject Meta { get; set; }

    [JsonPropertyName("apiDomain")]
    public string ApiDomain { get; set; }

    [JsonPropertyName("statusCode")]
    public int statusCode { get; set; }

    [JsonPropertyName("location")]
    public string Location { get; set; }


}




