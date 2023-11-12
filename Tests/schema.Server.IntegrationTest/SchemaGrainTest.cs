namespace schema.Server.IntegrationTest;

using System;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Json.More;
using Json.Schema;
using schema.Abstractions.Grains;
using schema.Server.IntegrationTest.Fixtures;
using Xunit;
using Xunit.Abstractions;

public class SchemaGrainTest : ClusterFixture
{
    public SchemaGrainTest(ITestOutputHelper testOutputHelper)
        : base(testOutputHelper)
    {
    }

    [Fact]
    public async Task SetSchema_ShouldReturnUpdatedSchema()
    {
        var grain = this.Cluster.GrainFactory.GetGrain<ISchemaGrain>("http://localhost:5503/schema/4_DxFqHMTOAJNe9VmFvyO3Uw");

        var result = await grain.SetSchemaAsync(new UserInfo(JsonSchema.FromText(GetSchema("http://localhost:5503/schema/4_DxFqHMTOAJNe9VmFvyO3Uw").ToJsonString()) )).ConfigureAwait(false);


        this.TestOutputHelper.WriteLine(result.ToJsonDocument().RootElement.ToJsonString());
        // Assert.Equal(schema, result);
        static JsonObject GetSchema(string baseUri) =>
            new()
            {
                ["$id"] = baseUri,
                ["type"] = "object",
                ["properties"] = new JsonObject()
                {
                    ["profile"] =
                        new JsonObject()
                        {
                            ["$ref"] = $"profile",
                            ["properties"] = new JsonObject() { ["age"] = true, ["name"] = true },
                            ["additionalProperties"] = false
                        },
                    ["data"] = new JsonObject() { ["$ref"] = $"data" }
                    // ["name"] = new JsonObject() { ["$ref"] = $"profile/name" }
                }
            };

    }

}
