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
        // var registry = await this
        //     .GrainFactory.GetGrain<ISchemaRegistryGrain>(this.GetPrimaryKeyString())
        //     .GetRegistryAsync()
        //     .ConfigureAwait(true);

        // this.options = registry.ToEvaluationOptions();
        this.options.EvaluateAs = SpecVersion.Draft202012;
         var schema = await this.GrainFactory.GetGrain<ISchemaGrain>(this.GetPrimaryKeyString()).GetSchemaAsync();
        // this.bundledSchema =  schema.Deserialize<JsonSchema>().Bundle(this.options);
        this.bundledSchema = await this
            .GrainFactory.GetGrain<ISchemaRegistryGrain>(this.GetPrimaryKeyString())
            .BundleSchemaAsync(schema.Deserialize<JsonSchema>()!)
            .ConfigureAwait(true);
        // this.options.SchemaRegistry.Register(new Uri(this.GetPrimaryKeyString()), this.bundledSchema);

    }

    public ValueTask<JsonDocument> GetSchemaAsync() => ValueTask.FromResult(this.bundledSchema.ToJsonDocument());

    //bundle input schema with the bundled schema
    public ValueTask<ImmutableSchema> BundleSchemaAsync(ImmutableSchema schema) =>
        ValueTask.FromResult(this.BundleSchema(schema));

    private  ImmutableSchema BundleSchema(JsonSchema schema)=> schema.Bundle(this.options);




}
