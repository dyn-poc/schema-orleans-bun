namespace schema.server.UnitTest;

using Xunit.Abstractions;

public class Fixture
{
    public Fixture(ITestOutputHelper testOutputHelper) => this.TestOutputHelper = testOutputHelper;

    public ITestOutputHelper TestOutputHelper { get; }
}
