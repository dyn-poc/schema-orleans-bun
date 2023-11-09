namespace schema.Grains;

using System.Buffers;
using System.Buffers.Text;
using System.Net.Http.Json;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Schema;
using Json.Schema.Generation;
using Json.Schema.Serialization;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Concurrency;
using Orleans.Hosting;
using schema.Abstractions.Grains;
using Grains;
using JsonSerializer = System.Text.Json.JsonSerializer;

[StatelessWorker]
public class ProfileSchemaGrain : Grain, IProfileSchemaGrain
{
     private readonly HttpClient httpClient = new();
    private EvaluationOptions evaluationOptions;
    private readonly ILogger<ProfileSchemaGrain> logger;
    private SiteRegistry SiteRegistry;
    public ProfileSchemaGrain(ILogger<ProfileSchemaGrain> logger)
    {
        this.logger = logger;
    }

    /// <inheritdoc />
    public override async Task OnActivateAsync()
    {
        await base.OnActivateAsync().ConfigureAwait(true);
         var uri = new Uri(this.GetPrimaryKeyString());

        var accountSchema = await this.GetAccountsSchema(uri.Segments.Skip(2).First().TrimEnd('/'));
        this.evaluationOptions = new EvaluationOptions();

        this.SiteRegistry = new (new Uri(this.GetPrimaryKeyString()));
        this.SiteRegistry[ "profile"] = SchemaConvert.Convert(accountSchema!.Profile.Fields, "profile");
        this.SiteRegistry[ "data"] = SchemaConvert.Convert(accountSchema!.Data.Fields, "data");
        this.SiteRegistry[ "preferences"] = SchemaConvert.Convert(accountSchema!.Preferences.Fields, "data");
        this.SiteRegistry[ "subscriptions"] = SchemaConvert.Convert(accountSchema!.Subscriptions.Fields, "data");


    }

    private async Task<AccountsSchema?> GetAccountsSchema(string apiKey)
    {
        var path = "accounts.getSchema";

        var accountSchema=await this.GetAccountsSchemaAsync(apiKey, $"https://accounts.gigya.com/accounts.getSchema?apiKey={apiKey}").ConfigureAwait(false);

        if (accountSchema?.Location is not null and var location)
        {
            accountSchema = await this.GetAccountsSchemaAsync(apiKey,       new Uri($"{location}?apiKey={apiKey}" ).ToString()).ConfigureAwait(false);
                ;
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

    public Task<JsonSchema> GetSchemaAsync(string type) =>  Task.FromResult(this.SiteRegistry[type].Bundle());

    public Task<SiteRegistry> GetRegistryAsync() => Task.FromResult(this.SiteRegistry);

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




