using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Domain;
using EventSourced.Domain.Snapshosts;

namespace EventSourced.Persistence
{
    public interface ISnapshotStore<TAggregateRoot>
        where TAggregateRoot : AggregateRoot
    {
        Task StoreSnapshotAsync(TAggregateRoot aggregateRoot, CancellationToken ct);
        Task<TAggregateRoot?> LoadSnapshotAsync(string aggregateRootId, CancellationToken ct);
    }
}