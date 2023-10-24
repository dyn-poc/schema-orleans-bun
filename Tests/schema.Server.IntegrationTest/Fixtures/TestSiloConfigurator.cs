namespace schema.Server.IntegrationTest.Fixtures;

using Orleans.Hosting;
using Orleans.TestingHost;
using schema.Abstractions.Constants;

public class TestSiloConfigurator : ISiloConfigurator
{
    public void Configure(ISiloBuilder siloBuilder) =>
        siloBuilder
            .AddMemoryGrainStorageAsDefault()
            .AddMemoryGrainStorage("PubSubStore")
            .AddMemoryGrainStorage("schema_store")
            .AddSimpleMessageStreamProvider(StreamProviderName.Default);
}
