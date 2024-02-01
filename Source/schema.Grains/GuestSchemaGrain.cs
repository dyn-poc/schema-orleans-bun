namespace schema.Grains;

using System.Text.Json;
using Abstractions.Grains;
using Json.More;
using Json.Schema;
using Orleans;
using Orleans.Runtime;

public class GuestSchemaGrain : Grain, IGuestSchemaGrain
{
    private EvaluationOptions options = new();
    private string siteSchema;
    private ISchemaRegistryGrain siteSchemaGrain;
    private JsonSchema bundledSchema;

    private IPersistentState<GuestSchema> State { get; }

    public GuestSchemaGrain([PersistentState("schema", "schema_store")] IPersistentState<GuestSchema> state) =>
        this.State = state;

    /// <inheritdoc />
    public override async Task OnActivateAsync()
    {
        this.siteSchema = new Uri(this.GetPrimaryKeyString()).GetParentUri().ToString().TrimEnd('/');
        this.siteSchemaGrain = this.GrainFactory.GetGrain<ISchemaRegistryGrain>(this.siteSchema);

        if (this.State.State.JsonSchema.Keywords is { Count: 0 } or null || this.State.State.JsonSchema.BaseUri.GetComponents(UriComponents.Path, UriFormat.Unescaped).Contains("bundled"))
        {
            this.State.State = new GuestSchema(new JsonSchemaBuilder()
                .Id($"{this.siteSchema}/guest")
                .Properties(
                    new Dictionary<string, JsonSchema>()
                    {
                        ["auth"] = new JsonSchemaBuilder().Const("guest"),
                        ["user_info"] = new JsonSchemaBuilder()
                            .Properties(new Dictionary<string, JsonSchema>()
                            {
                                ["preferences"] =
                                    //ref to original preferences schema
                                    new JsonSchemaBuilder().Ref("preferences").Anchor("preferences")
                                        //specify what properties are allowed
                                        .Properties(new Dictionary<string, JsonSchema>() { ["terms"] = true })
                                        //don't allow any other properties
                                        .AdditionalProperties(false),
                                ["subscriptions"] =
                                    new JsonSchemaBuilder().Ref("subscriptions").Anchor("subscriptions")
                                        .Properties(new Dictionary<string, JsonSchema>() { ["newsletter"] = true })
                                        .AdditionalProperties(false),
                                ["data"] =
                                    new JsonSchemaBuilder().Ref("data")
                                        .Properties(new Dictionary<string, JsonSchema>() { ["zip"] = true })
                                        .AdditionalProperties(false),
                                ["profile"] = new JsonSchemaBuilder().Ref("profile")
                                    .Properties(new Dictionary<string, JsonSchema>()
                                    {
                                        ["email"] = true, ["firstName"] = true
                                    }).AdditionalProperties(false)
                            })
                    })
                .Build());
            await this.State
                .WriteStateAsync()
                .ConfigureAwait(true);
        }
    }


    public async ValueTask<JsonSchema> SetSchemaAsync(GuestSchema schema)
    {
        this.State.State = schema;
        await this.State.WriteStateAsync().ConfigureAwait(true);

        return this.State.State.JsonSchema.Bundle(this.options);
    }

    public ValueTask<JsonDocument> GetSchemaAsync() =>
        ValueTask.FromResult(this.State.State.JsonSchema.ToJsonDocument());

    public async ValueTask<JsonDocument> GetBundledSchema() =>
        (await this
            .siteSchemaGrain
            .BundleSchemaAsync(this.State.State.JsonSchema)
            .ConfigureAwait(true)).Schema.ToJsonDocument();
}
