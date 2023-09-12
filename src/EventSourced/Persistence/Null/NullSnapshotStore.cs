using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Domain;

namespace EventSourced.Persistence.Null
{
    public class NullSnapshotStore<TAggregateRoot> : ISnapshotStore<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        public Task StoreSnapshotAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            return Task.CompletedTask;
        }

        public Task<TAggregateRoot?> LoadSnapshotAsync(string aggregateRootId, CancellationToken ct)
        {
            return Task.FromResult<TAggregateRoot?>(null);
        }
    }
}