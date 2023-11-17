namespace schema.Grains;

using System.Text.Json;
using Abstractions.Grains;
using Json.More;
using Json.Schema;
using Orleans;
using Orleans.Concurrency;

[StatelessWorker, Reentrant]
public class BundledSchemaGrain: Grain, IBundledSchemaGrain
{
    private EvaluationOptions options = new();
    private JsonSchema bundledSchema;


    /// <inheritdoc />
    public override async Task OnActivateAsync()
    {
        this.options.EvaluateAs = SpecVersion.Draft202012;
         var schema = await this.GrainFactory.GetGrain<ISchemaGrain>(this.GetPrimaryKeyString()).GetSchemaAsync();

        this.bundledSchema = await this
            .GrainFactory.GetGrain<ISchemaRegistryGrain>(this.GetPrimaryKeyString())
            .BundleSchemaAsync(schema.Deserialize<JsonSchema>()!)
            .ConfigureAwait(true);

    }

    public ValueTask<JsonDocument> GetSchemaAsync() => ValueTask.FromResult(this.bundledSchema.ToJsonDocument());

    //bundle input schema with the bundled schema
    public ValueTask<ImmutableSchema> BundleSchemaAsync(ImmutableSchema schema) =>
        ValueTask.FromResult(this.BundleSchema(schema));

    private  ImmutableSchema BundleSchema(JsonSchema schema)=> schema.Bundle(this.options);




}
