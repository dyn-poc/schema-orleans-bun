namespace schema.Grains;

using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using Json.Schema;
using Orleans;
using Orleans.Runtime;
using schema.Abstractions.Grains;

public class SchemaGrain : Grain, ISchemaGrain
{
    private EvaluationOptions options = new();

    private IPersistentState<UserInfo> State { get; }

    public SchemaGrain([PersistentState("schema", "schema_store")] IPersistentState<UserInfo> state) => this.State = state;

    /// <inheritdoc />
    public override async Task OnActivateAsync()
    {
        var registry = await this
            .GrainFactory.GetGrain<IProfileSchemaGrain>(this.GetPrimaryKeyString())
            .GetRegistryAsync()
            .ConfigureAwait(true);

        this.options = registry.ToEvaluationOptions();
        if (State.State.JsonSchema.Keywords.Count == 0)
        {
            this.State.State = new UserInfo(new JsonSchemaBuilder()
                .Id(this.GetPrimaryKeyString())
                .Properties(new Dictionary<string, JsonSchema>()
                {
                    ["profile"] = new JsonSchemaBuilder().Ref("profile"),
                    ["data"] = new JsonSchemaBuilder().Ref("data"),
                    ["preferences"] = new JsonSchemaBuilder().Ref("preferences"),
                    ["subscriptions"] = new JsonSchemaBuilder().Ref("subscriptions"),
                    ["restricted"] =  JsonSchema.FromText(new JsonObject()
                    {
                        ["$ref"] = $"profile",
                        ["properties"] = new JsonObject() { ["age"] = true, ["name"] = true },
                        ["additionalProperties"] = false
                    }.ToJsonString())
                })
                .Build());
            await this.State.WriteStateAsync();
        }
    }

    public async ValueTask<JsonSchema> SetSchemaAsync(UserInfo schema)
    {
        this.State.State = schema;
        await this.State.WriteStateAsync().ConfigureAwait(true);

        return this.State.State.JsonSchema.Bundle(this.options);
    }

    ValueTask<JsonDocument> ISchemaGrain.GetSchemaAsync() => ValueTask.FromResult(this.State.State.JsonSchema.ToJsonDocument());
}






public class ContextSchema
{
    public JsonSchema Policy { get; set; }
    public JsonSchema Identity { get; set; }
    public JsonSchema UserInfo { get; set; }

    public JsonSchema Schema =>
        new JsonSchemaBuilder()
            .Properties(new Dictionary<string, JsonSchema>()
            {
                ["policy"] = this.Policy,
                ["identity"] = this.Identity,
                ["info"] = this.UserInfo
            });

}


// public record UserInfo(string Id, Dictionary<string, UserInfo.Section> Sections)
// {
//     public JsonSchemaBuilder JsonSchemaBuilder =>
//         new JsonSchemaBuilder()
//             .Id(this.Id)
//             .Properties(this.Sections.Select(e => (e.Key, e.Value.Apply(new JsonSchemaBuilder()).Build())).ToArray()
//             );
//
//     public record Section(string Ref, IReadOnlyDictionary<string, JsonSchema> Properties)
//     {
//         public JsonSchemaBuilder SchemaBuilder { get; init; } = new JsonSchemaBuilder().Ref(Ref).Properties(Properties)
//             .AdditionalProperties(false);
//
//         public static implicit operator JsonSchema(Section section) => section.SchemaBuilder;
//         public static implicit operator JsonSchemaBuilder(Section section) => section.SchemaBuilder;
//
//         public JsonSchemaBuilder Apply(JsonSchemaBuilder builder) =>
//             builder.Ref(this.Ref).Properties(this.Properties).AdditionalProperties(false);
//
//         // public (string, JsonSchema) Property => (this.Name, this.Apply(new JsonSchemaBuilder()).Build());
//     }
//     // public record Section(string Name, string Ref, IReadOnlyDictionary<string, JsonSchema> Properties)
//     // {
//     //     public JsonSchemaBuilder Apply(JsonSchemaBuilder builder) =>
//     //         builder.Ref(this.Ref).Properties(this.Properties).AdditionalProperties(false);
//     //
//     //     public (string, JsonSchema) Property => (this.Name, this.Apply(new JsonSchemaBuilder()).Build());
//     // }
// }


