namespace schema.Grains;

using System.Text.Json;
using Abstractions.Grains;
using Json.More;
using Json.Schema;
using Orleans;
using Orleans.Runtime;

public class AuthSchemaGrain : Grain, IAuthSchemaGrain
{
    private EvaluationOptions options = new();
    private string siteSchema;
    private ISchemaRegistryGrain siteSchemaGrain;
    private JsonSchema bundledSchema;
    private ISchemaRegistryGrain registryGrain;

    private IPersistentState<ImmutableSchema> State { get; }

    public AuthSchemaGrain([PersistentState("schema", "schema_store")] IPersistentState<ImmutableSchema> state) =>
        this.State = state;

    /// <inheritdoc />
    public override async Task OnActivateAsync()
    {
        this.siteSchema = new Uri(this.GetPrimaryKeyString()).GetParentUri().ToString().TrimEnd('/');
        this.registryGrain = this.GrainFactory.GetGrain<ISchemaRegistryGrain>(this.siteSchema);

        if (this.State.State.Schema.Keywords is { Count: 0 } or null)
        {
            this.State.State = new ImmutableSchema(new JsonSchemaBuilder()
                .Id(this.GetPrimaryKeyString())
                .OneOf(
                    new JsonSchemaBuilder().Ref($"{this.siteSchema}/guest"),
                    new JsonSchemaBuilder().Ref($"{this.siteSchema}/fido")
                )
                .Build());
            await this.State.WriteStateAsync().ConfigureAwait(true);
        }

        var registry = await this.registryGrain.GetRegistryAsync();
        await this.RegisterAuthSchemas(registry);
        // await this.RegisterSiteSchema(registry);
        this.options = new() { EvaluateAs = SpecVersion.Draft202012 };
        registry.RegisterAll(this.options.SchemaRegistry);
        this.bundledSchema = this.State.State.Schema.Bundle(this.options);
    }

    private async Task RegisterSiteSchema(SiteRegistry registry) =>
        registry[this.siteSchema] = (await this.GrainFactory.GetGrain<ISchemaGrain>(this.siteSchema).GetSchemaAsync())
            .Deserialize<JsonSchema>()!;

    private async Task RegisterAuthSchemas(SiteRegistry registry)
    {
        registry["guest"] = (await Guest()).Deserialize<JsonSchema>()!;
        registry["fido"] = (await Fido()).Deserialize<JsonSchema>()!;


        async Task<JsonDocument> Fido()
        {
            return await this.GrainFactory.GetGrain<IGuestSchemaGrain>($"{this.siteSchema}/fido").GetSchemaAsync()
                .ConfigureAwait(true);
        }

        async Task<JsonDocument> Guest()
        {
            return await this.GrainFactory.GetGrain<IGuestSchemaGrain>($"{this.siteSchema}/guest")
                .GetSchemaAsync().ConfigureAwait(true);
        }
    }


    public async ValueTask<ImmutableSchema> SetSchemaAsync(ImmutableSchema schema)
    {
        this.State.State = schema;
        await this.State.WriteStateAsync().ConfigureAwait(true);

        return this.State.State.Schema.Bundle(this.options);
    }

    public ValueTask<JsonDocument> GetSchemaAsync() =>
        ValueTask.FromResult(this.State.State.Schema.ToJsonDocument());

    public async ValueTask<JsonDocument> GetBundledSchema() =>
        this.bundledSchema.ToJsonDocument();
}
