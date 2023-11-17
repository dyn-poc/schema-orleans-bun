namespace schema.Abstractions.Grains;

using System.Text.Json;
using Orleans;

public interface IAuthSchemaGrain:IGrainWithStringKey
{
    ValueTask<ImmutableSchema> SetSchemaAsync(ImmutableSchema schema);
    ValueTask<JsonDocument> GetSchemaAsync();
    ValueTask<JsonDocument> GetBundledSchema();
}
