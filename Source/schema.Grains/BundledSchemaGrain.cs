namespace schema.Grains;

using System.Text.Json;
using Abstractions.Grains;
using Json.More;
using Json.Schema;
using Orleans;

public class BundledSchemaGrain: Grain, IBundledSchemaGrain
{
    private EvaluationOptions options = new();
    private JsonSchema bundledSchema;


    /// <inheritdoc />
    public override async Task OnActivateAsync()
    {
        var registry = await this
            .GrainFactory.GetGrain<IProfileSchemaGrain>(this.GetPrimaryKeyString())
            .GetRegistryAsync()
            .ConfigureAwait(true);

        this.options = registry.ToEvaluationOptions();

        var schema = await this.GrainFactory.GetGrain<ISchemaGrain>(this.GetPrimaryKeyString()).GetSchemaAsync();
        this.bundledSchema =  schema.Deserialize<JsonSchema>().Bundle(this.options);

    }

    public ValueTask<JsonDocument> GetSchemaAsync() => ValueTask.FromResult(this.bundledSchema.ToJsonDocument());



}
