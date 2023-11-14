namespace schema.Abstractions.Grains;

using System.Text.Json;
using Json.Schema;
using Orleans;
using Orleans.Concurrency;

public interface ISchemaGrain : IGrainWithStringKey
{
   ValueTask<JsonSchema> SetSchemaAsync(UserInfo schema);
    ValueTask<JsonDocument> GetSchemaAsync();

}

public record UserInfo(JsonSchema JsonSchema)
{
    public JsonSchema JsonSchema { get; init; } = JsonSchema;
    public UserInfo() : this(new JsonSchemaBuilder().Build())
    {
    }
}


public readonly record struct SchemaKey(string Base, string Id, string Type)
{

    /// Serialize Key
    public override string ToString() => $"{this.Base}|{this.Id}|{this.Type}";

    public static implicit operator SchemaKey (string keyString) => FromString(keyString);
    public static implicit operator string (SchemaKey key) => key.ToString();

    /// Parse key from string representation
    public static SchemaKey FromString(string keyString)
    {
        var props = keyString.Split(new[] { '|' }, StringSplitOptions.RemoveEmptyEntries);
        return new SchemaKey(props[0], props[1], props[2]);
    }
}

public interface ISchemaRegistryGrain:IGrainWithStringKey
{
    Task<JsonSchema> GetSchemaAsync(string type);
    Task<SiteRegistry> GetRegistryAsync();


}


[Immutable]
[Serializable()]
public record SiteRegistry(Uri Base, Dictionary<string, JsonSchema>? Schemas = null)
{
    public Dictionary<string, JsonSchema> Schemas { get; init; } = Schemas ?? new();

    public void RegisterAll(SchemaRegistry registry)
    {
        foreach (var schema in this.Schemas)
        {
            registry.Register(new Uri(this.Base,schema.Key), schema.Value);
        }
    }

    public EvaluationOptions ToEvaluationOptions()
    {
        var options = new EvaluationOptions();
        this.RegisterAll(options.SchemaRegistry);
        return options;
    }

    public JsonSchema this[string key]
    {
        get => this.Schemas[key];
        set => this.Schemas[key] = value;
    }

}
