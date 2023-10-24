namespace schema.Grains;

using Orleans;
using Orleans.Concurrency;
using schema.Abstractions.Grains.HealthChecks;

[StatelessWorker(1)]
public class LocalHealthCheckGrain : Grain, ILocalHealthCheckGrain
{
    public ValueTask CheckAsync() => ValueTask.CompletedTask;
}