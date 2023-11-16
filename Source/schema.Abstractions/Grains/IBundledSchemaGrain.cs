namespace schema.Abstractions.Grains;

using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using Json.Schema;
using Orleans;
using Orleans.Concurrency;

public interface IBundledSchemaGrain: IGrainWithStringKey
{
    ValueTask<JsonDocument> GetSchemaAsync();

    ValueTask<ImmutableSchema> BundleSchemaAsync(ImmutableSchema schema);
}

[Immutable]
public record ImmutableSchema(JsonSchema Schema)
{
    public ImmutableSchema() : this( JsonSchema.Empty)
    {
    }

// cast operators from JSONSchema And to JSon Schema
    public static implicit operator JsonSchema(ImmutableSchema schema) => schema.Schema;
    public static implicit operator ImmutableSchema(JsonSchema schema) => new(schema);
    public static implicit operator ImmutableSchema(JsonNode node) => new(JsonSchema.FromText(node.ToJsonString()));


}
