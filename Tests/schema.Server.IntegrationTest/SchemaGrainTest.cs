namespace schema.Server.IntegrationTest;

using System;
using System.Threading.Tasks;
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
        var grain = this.Cluster.GrainFactory.GetGrain<ISchemaGrain>(new SchemaKey("apikey", "guest"));

        var schema = new JsonSchemaBuilder()
            .Type(SchemaValueType.String)
            .Build();

        var result = await grain.SetSchemaAsync(schema).ConfigureAwait(false);

        Assert.Equal(schema, result);


    }

}
