namespace schema.Grains;

using System.Text.Json;
using System.Text.Json.Nodes;
using Json.More;
using Json.Schema;
using Orleans;
using Orleans.Runtime;
using schema.Abstractions.Grains;

public class UserInfoSchemaGrain : Grain, ISchemaGrain
{
    private EvaluationOptions options = new();

    private IPersistentState<UserInfo> State { get; }

    public UserInfoSchemaGrain([PersistentState("schema", "schema_store")] IPersistentState<UserInfo> state) =>
        this.State = state;

    /// <inheritdoc />
    public override async Task OnActivateAsync()
    {
        if (this.State.State.JsonSchema.Keywords is {Count:0 } or null)
        {
            this.State.State = new UserInfo(new JsonSchemaBuilder()
                .Id(this.GetPrimaryKeyString())
                .Properties(new Dictionary<string, JsonSchema>()
                {
                    ["profile"] = new JsonSchemaBuilder().Ref("profile").Anchor("profile"),
                    ["data"] = new JsonSchemaBuilder().Ref("data").Anchor("data"),
                    ["preferences"] = new JsonSchemaBuilder().Ref("preferences").Anchor("preferences"),
                    ["subscriptions"] = new JsonSchemaBuilder().Ref("subscriptions").Anchor("subscriptions"),


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

    ValueTask<JsonDocument> ISchemaGrain.GetSchemaAsync() =>
        ValueTask.FromResult(this.State.State.JsonSchema.ToJsonDocument());
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
