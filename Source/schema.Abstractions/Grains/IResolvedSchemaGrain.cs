namespace schema.Abstractions;

using Json.Pointer;
using Json.Schema;
using Orleans;

public interface IResolvedSchemaGrain : IGrainWithStringKey
{
    Task<JsonSchema?> GetSchema(JsonPointer? pointer = null);
}
