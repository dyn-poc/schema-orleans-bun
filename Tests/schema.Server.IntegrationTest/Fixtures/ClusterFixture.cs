namespace schema.Server.IntegrationTest.Fixtures;

using System.Threading.Tasks;
using Orleans.TestingHost;
using Xunit;
using Xunit.Abstractions;

public class ClusterFixture : IAsyncLifetime
{
    public ClusterFixture(ITestOutputHelper testOutputHelper)
    {
        this.TestOutputHelper = testOutputHelper;

        this.Cluster = new TestClusterBuilder()
            .AddClientBuilderConfigurator<TestClientBuilderConfigurator>()
            .AddSiloBuilderConfigurator<TestSiloConfigurator>()
            .Build();
    }

    public TestCluster Cluster { get; }

    public ITestOutputHelper TestOutputHelper { get; }

    public Task DisposeAsync() => this.Cluster.DisposeAsync().AsTask();

    public Task InitializeAsync() => this.Cluster.DeployAsync();
}