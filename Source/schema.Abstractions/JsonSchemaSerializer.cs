namespace schema.Abstractions;

using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using Json.More;
using Json.Schema;
using Orleans.CodeGeneration;
using Orleans.Serialization;

[Orleans.CodeGeneration.SerializerAttribute(typeof(JsonSchema))]
[Orleans.CodeGeneration.SerializerAttribute(typeof(JsonSchemaBuilder))]
internal class JsonSchemaSerializer
{
    [CopierMethod]
    public static object DeepCopier(
        object original, ICopyContext context)
    {
        var input = (JsonSchema)original;
        var result = input;

        // Record 'result' as a copy of 'input'. Doing this
        // immediately after construction allows for data
        // structures that have cyclic references or duplicate
        // references. For example, imagine that 'input.BestFriend'
        // is set to 'input'. In that case, failing to record
        // the copy before trying to copy the 'BestFriend' field
        // would result in infinite recursion.
        // context.RecordCopy(original, result);
        //
        // foreach (var keyword in input.Keywords ?? Enumerable.Empty<IJsonSchemaKeyword>())
        // {
        //     result.Add(keyword);
        // }

        return result;
    }

    [SerializerMethod]
    public static void Serializer(
        object untypedInput, ISerializationContext context, Type expected)
    {
        var input = (JsonSchema)untypedInput;

        context.SerializeInner( JsonSerializer.SerializeToNode(input, new JsonSerializerOptions()
        {
            UnknownTypeHandling = JsonUnknownTypeHandling.JsonNode
        }), typeof(JsonNode));
    }

    [DeserializerMethod]
    public static object Deserializer(
        Type expected, IDeserializationContext context)
    {
        var result = new JsonSchemaBuilder();

        // Record 'result' immediately after constructing it.
        // As with the deep copier, this
        // allows for cyclic references and de-duplication.
        context.RecordObject(result);
         if (context.DeserializeInner(typeof(JsonNode)) is JsonNode jsonDocument)
        {
            var keywords = JsonSchema.FromText(jsonDocument.ToJsonString()).Keywords;

            foreach (var keyword in keywords ?? Enumerable.Empty<IJsonSchemaKeyword>())
            {
                result.Add(keyword);
            }
        }


        return result.Build();
    }
}
