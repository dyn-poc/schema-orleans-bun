namespace schema.Grains;

using System.Text.Json.Serialization;
using Json.Schema;
using Json.Schema.Generation.Intents;
using Orleans;
using Orleans.Runtime;
using schema.Abstractions.Grains;

public class SchemaGrain : Grain, ISchemaGrain
{
    private IPersistentState<JsonSchema> State { get; }

    public SchemaGrain([PersistentState("schema", "schema_store")] IPersistentState<JsonSchema> state) => this.State = state;
    public async ValueTask<JsonSchema> SetSchemaAsync(JsonSchema schema)
    {
        this.State.State = schema;
        await this.State.WriteStateAsync().ConfigureAwait(true);
        return this.State.State;
    }



    ValueTask<JsonSchema> ISchemaGrain.GetSchemaAsync() => ValueTask.FromResult(this.State.State);
}


public class RegisterRequest
{
    [JsonPropertyName("user_info")]
    public UserInfo UserInfo { get; set; }
    public Identity Identity { get; set; }
    public Policy Policy { get; set; }
}

public record AccountSchema(string BaseSchema, IReadOnlyDictionary<string, JsonSchema> Properties)
{
    private JsonSchema Ref => new JsonSchemaBuilder().Ref(this.BaseSchema);
    private JsonSchema PropertiesSchema => new JsonSchemaBuilder().Properties(this.Properties);
    public JsonSchema Schema =>
        new JsonSchemaBuilder()
            .AllOf(new []{Ref, PropertiesSchema  });


    public static implicit operator JsonSchema(AccountSchema accountSchema) => accountSchema.Schema;

}


public class UserInfoSchema: SchemaOrPropertyList
{
    /// <inheritdoc />
    public UserInfoSchema(
        string baseSchema,
        IReadOnlyDictionary<string, JsonSchema> properties) : base(GetSchema(properties))
    {
    }

    private static JsonSchemaBuilder GetSchema(IReadOnlyDictionary<string, JsonSchema> properties) =>
        new JsonSchemaBuilder()
            .Properties(properties);

    /// <inheritdoc />
    public UserInfoSchema(IEnumerable<string> requirements) : base(requirements)
    {
    }
}

public class Policy
{
}

public class Identity
{
}

public class UserInfo
{
}

