namespace schema.Abstractions.Grains;

using Orleans;

public interface IHelloGrain : IGrainWithGuidKey
{
    ValueTask<string> SayHelloAsync(string name);
}