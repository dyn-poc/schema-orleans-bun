namespace schema.Abstractions.Grains;

using System.Text.Json;
using Json.Schema;
using Orleans;
using Orleans.Concurrency;

public interface IBundledSchemaGrain: IGrainWithStringKey
{
    ValueTask<JsonDocument> GetSchemaAsync();

}
