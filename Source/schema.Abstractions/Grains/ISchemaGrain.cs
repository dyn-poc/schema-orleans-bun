namespace schema.Abstractions.Grains;

using Json.Schema;
using Orleans;

public interface ISchemaGrain : IGrainWithStringKey
{
    ValueTask<JsonSchema> SetSchemaAsync(JsonSchema schema);
    ValueTask<JsonSchema> GetSchemaAsync();
}


public record struct SchemaKey(string Id, string Type)
{

    /// Serialize Key
    public override readonly string ToString() => $"{this.Id}|{this.Type}";

    public static implicit operator SchemaKey (string keyString) => FromString(keyString);
    public static implicit operator string (SchemaKey key) => key.ToString();

    /// Parse key from string representation
    public static SchemaKey FromString(string keyString)
    {
        var props = keyString.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        return new SchemaKey(props[0], props[1]);
    }

}


