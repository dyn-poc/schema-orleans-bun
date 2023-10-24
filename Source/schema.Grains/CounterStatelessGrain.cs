namespace schema.Grains;

using Orleans;
using Orleans.Concurrency;
using schema.Abstractions.Grains;

/// <summary>
/// An implementation of the 'Reduce' pattern (See https://github.com/OrleansContrib/DesignPatterns/blob/master/Reduce.md).
/// </summary>
/// <seealso cref="Grain" />
/// <seealso cref="ICounterStatelessGrain" />
[StatelessWorker]
public class CounterStatelessGrain : Grain, ICounterStatelessGrain
{

}
