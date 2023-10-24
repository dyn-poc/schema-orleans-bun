namespace schema.Grains;

using System.Text.Json;
using System.Text.Json.Nodes;
using Abstractions;
using Json.More;
using Json.Pointer;
using Json.Schema;
using Microsoft.Extensions.Logging;
using Orleans;
using Orleans.Runtime;

public class ResolvedSchemaGrain : Grain, IResolvedSchemaGrain
{
    private readonly IPersistentState<JsonNodeBaseDocument?> state;
    private readonly EvaluationOptions options = new();
    private readonly ILogger<ResolvedSchemaGrain> logger;
    private readonly HttpClient httpClient;
    private JsonNodeBaseDocument root;
    private JsonSchema bundled;

    public ResolvedSchemaGrain(ILogger<ResolvedSchemaGrain> logger, HttpClient httpClient)
    {
        this.logger = logger;
        this.httpClient = httpClient;
        this.options = new EvaluationOptions()
        {
            SchemaRegistry =
            {
                // Fetch = this.DownloadSchema
            }
        };
    }


    /// <inheritdoc />
    public override Task OnActivateAsync()
    {
        // this.options.SchemaRegistry.Fetch= this.DownloadSchema;

        this.root =
            new JsonNodeBaseDocument(GetSchema(this.GetPrimaryKeyString()), new Uri(this.GetPrimaryKeyString()));
        this.options.EvaluateAs = SpecVersion.Draft202012;
        this.options.SchemaRegistry.Register(GetProfileSchema(this.GetPrimaryKeyString()).Deserialize<JsonSchema>()!);
        this.options.SchemaRegistry.Register(GetSchema(this.GetPrimaryKeyString()).Deserialize<JsonSchema>()!);
        this.bundled =
            this.options.SchemaRegistry.Get(new Uri(this.GetPrimaryKeyString()))!.FindSubschema(JsonPointer.Parse("#"),
                this.options)!.Bundle(this.options);
        return Task.CompletedTask;
    }

    public async Task<JsonSchema?> GetSchema(JsonPointer? pointer = null)
    {
        var schema = this.root;
        pointer ??= JsonPointer.Parse("#");

        return this.bundled;
        // return Task.FromResult(JsonSchema.FromText(GetSchema(this.GetPrimaryKeyString())).Bundle());
    }

    static JsonObject GetSchema(string baseUri) =>
        new()
        {
            ["$id"] = baseUri,
            ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
            ["type"] = "object",
            ["properties"] = new JsonObject()
            {
                ["profile"] = new JsonObject() { ["$ref"] = $"#/$defs/profile" },
                ["data"] = new JsonObject() { ["$ref"] = $"#/$defs/profile" }
                // ["name"] = new JsonObject() { ["$ref"] = $"profile/name" }
            },
            ["$defs"] = new JsonObject() { ["profile"] = new JsonObject() { ["$ref"] = $"{baseUri}/profile" } }
        };

    public static JsonObject GetProfileSchema(string baseUri) => new()
    {
        ["$schema"] = "https://json-schema.org/draft/2020-12/schema",
        ["$id"] = $"{baseUri}/profile",
        // ["apiKey"] =apiKey,
        ["type"] = "object",
        ["properties"] = new JsonObject
        {
            ["name"] = new JsonObject
            {
                ["$anchor"] = $"name",
                ["type"] = "string",
                ["description"] = "Name of the person",
                ["minLength"] = 2,
                ["maxLength"] = 10
            },
            ["age"] = new JsonObject
            {
                ["$anchor"] = "age", ["type"] = "integer", ["minimum"] = 18, ["maximum"] = 99
            }
        },
        ["required"] = new JsonArray { "name", "age" }
    };


    /*
        private IBaseDocument? DownloadSchema(Uri uri)
        {
            this.logger.LogWarning("Hit Registry {uri}", uri);
            var myStream = this.httpClient.Send(new HttpRequestMessage(HttpMethod.Get, uri)).Content.ReadAsStream();
            return JsonSchema.FromText(JsonNode.Parse(myStream)!.ToJsonString()!);
        }
    */
}
