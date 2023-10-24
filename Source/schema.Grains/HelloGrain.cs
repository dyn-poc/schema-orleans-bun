namespace schema.Grains;

using Orleans;
using schema.Abstractions.Constants;
using schema.Abstractions.Grains;

public class HelloGrain : Grain, IHelloGrain
{
    public async ValueTask<string> SayHelloAsync(string name)
    {
        await this.PublishSaidHelloAsync(name).ConfigureAwait(true);

        return $"Hello {name}!";
    }


    private Task PublishSaidHelloAsync(string name)
    {
        var streamProvider = this.GetStreamProvider(StreamProviderName.Default);
        var stream = streamProvider.GetStream<string>(Guid.Empty, StreamName.SaidHello);
        return stream.OnNextAsync(name);
    }
}
