namespace schema.Grains;

using System.Text.Json;
using Abstractions.Grains;
using Json.Schema;
using Orleans.Core;
using Orleans.Runtime;
using Orleans.Storage;

/// <summary>
/// NOT IN USED + Has bugs
/// </summary>
public class MixinJsonSchema
{
    public JsonDocument Schema { get; set; }
    public Dictionary<string, string> Properties { get; set; }
    public JsonSchema GetSchema() => JsonSchema.FromText(this.GetSchemaText());

    private string GetSchemaText()
    {
        var raw= this.Schema.RootElement.GetRawText();
        // replace the properties with the values from the dictionary

        return this.Properties
            .Aggregate(raw, (current, property) =>
                current.Replace($"${{{property.Key}}}", property.Value));
    }
}


/// <summary>
/// NOT IN USED + Has bugs
/// </summary>
public class SchemaStorage: IStorage<JsonSchema>
{
    private readonly string fullStateName;
    private readonly IStorage<MixinJsonSchema> storage;
    private readonly IGrainActivationContext context;
    private readonly IGrainStorage storageProvider;
    private readonly SchemaKey key;

    public SchemaStorage(IGrainActivationContext context, IGrainStorage storageProvider , IStorage<MixinJsonSchema> storage)
    {
        this.fullStateName = fullStateName;
        this.context = context;
        this.storageProvider = storageProvider;
        this.key = this.context.GrainIdentity.PrimaryKeyString;

    }

    public SchemaStorage(string fullStateName, IGrainActivationContext context, IGrainStorage storageProvider , IStorage<MixinJsonSchema> storage)
    {
        this.fullStateName = fullStateName;
        this.context = context;
        this.storageProvider = storageProvider;
        this.key = this.context.GrainIdentity.PrimaryKeyString;
    }


    public SchemaStorage(IStorage<MixinJsonSchema> storage) => this.storage = storage;

    /// <inheritdoc />
    public Task ClearStateAsync() => this.storage.ClearStateAsync();
    /// <inheritdoc />
    public Task WriteStateAsync() => this.storage.WriteStateAsync();

    /// <inheritdoc />
    public Task ReadStateAsync() => this.storage.ReadStateAsync();

    /// <inheritdoc />
    public string Etag => this.storage.Etag;

    /// <inheritdoc />
    public bool RecordExists => this.storage.RecordExists;

    /// <inheritdoc />
    public JsonSchema State
    {
        get => this.storage.State.GetSchema() ?? JsonSchema.Empty;
        set => this.storage.State = new MixinJsonSchema
        {
            Schema = JsonDocument.Parse(value.ToString()),
            Properties = new Dictionary<string, string>()
            {
                ["apiKey"] = this.key.Id,

            }
        };
    }
}
