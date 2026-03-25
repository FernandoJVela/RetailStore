namespace RetailStore.SharedKernel.Domain;

public abstract class AggregateRoot : Entity
{
    // Version for optimistic concurrency
    public int Version { get; protected set; }

    protected void IncrementVersion() => Version++;
}
