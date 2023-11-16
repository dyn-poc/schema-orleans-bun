namespace schema.Abstractions.Grains;

using System.Text.Json;
using Json.Schema;
using Orleans;

public interface IGuestSchemaGrain: IGrainWithStringKey
{
    ValueTask<JsonSchema> SetSchemaAsync(GuestSchema schema);
    ValueTask<JsonDocument> GetSchemaAsync();
    ValueTask<JsonDocument> GetBundledSchema();
}

public record GuestSchema(JsonSchema JsonSchema)
{
    public JsonSchema JsonSchema { get; init; } = JsonSchema;
    public GuestSchema() : this(new JsonSchemaBuilder().Build())
    {
    }
}
