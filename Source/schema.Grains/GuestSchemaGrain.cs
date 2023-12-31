namespace schema.Grains;

using System.Text.Json;
using Abstractions.Grains;
using Json.More;
using Json.Schema;
using Orleans;
using Orleans.Runtime;

public class GuestSchemaGrain:Grain, IGuestSchemaGrain
{
    private EvaluationOptions options = new();
    private string siteSchema;
    private ISchemaRegistryGrain siteSchemaGrain;

    private IPersistentState<GuestSchema> State { get; }

    public GuestSchemaGrain([PersistentState("schema", "schema_store")] IPersistentState<GuestSchema> state) =>
        this.State = state;

    /// <inheritdoc />
    public override async Task OnActivateAsync()
    {
        this.siteSchema = new Uri(this.GetPrimaryKeyString()).GetParentUri().ToString().TrimEnd('/');
        this.siteSchemaGrain =   this.GrainFactory.GetGrain<ISchemaRegistryGrain>(this.siteSchema);
        // var schema = await this
        //     .GrainFactory.GetGrain<IBundledSchemaGrain>(siteSchema)
        //     .GetSchemaAsync()
        //     .ConfigureAwait(true);

        // this.options.SchemaRegistry.Register(new Uri(siteSchema), schema.Deserialize<JsonSchema>()!);
        if (this.State.State.JsonSchema.Keywords is {Count:0 } or null)
        {
            this.State.State = new GuestSchema(new JsonSchemaBuilder()
                .Id(this.GetPrimaryKeyString())
                .Ref(this.siteSchema)
                .Properties(new Dictionary<string, JsonSchema>()
                {
                    // ["preferences"] = new JsonSchemaBuilder().Ref("preferences").Anchor("preferences").Properties(new Dictionary<string, JsonSchema>()
                    // {
                    //     ["terms"] = true
                    // }).AdditionalProperties(false),
                    // ["subscriptions"] = new JsonSchemaBuilder().Ref("subscriptions").Anchor("subscriptions").Properties(new Dictionary<string, JsonSchema>()
                    // {
                    //     ["newsletter"] = true
                    // }).AdditionalProperties(false),
                    ["data"] = new JsonSchemaBuilder().Ref("profile").Properties(new Dictionary<string, JsonSchema>()
                    {
                        ["zip"] = true
                    }).AdditionalProperties(false),
                    ["profile"] = new JsonSchemaBuilder().Ref("data").Properties(new Dictionary<string, JsonSchema>()
                    {
                        ["email"] = true,
                        ["firstName"] = true
                    }).AdditionalProperties(false)

                })
                .Build());
            await this.State.WriteStateAsync();
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

   public async ValueTask<JsonDocument> GetBundledSchema( ) =>
        (await this.siteSchemaGrain.BundleSchemaAsync(this.State.State.JsonSchema)).ToJsonDocument();


}



