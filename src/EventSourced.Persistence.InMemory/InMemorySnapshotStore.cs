using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Domain;
using EventSourced.Domain.Snapshosts;
using EventSourced.Persistence.InMemory.Helpers;

namespace EventSourced.Persistence.InMemory
{
    public class InMemorySnapshotStore<TAggregateRoot> : ISnapshotStore<TAggregateRoot> 
        where TAggregateRoot : AggregateRoot
    {
        private ConcurrentDictionary<string, AggregateSnapshot<TAggregateRoot>> Snapshots { get; }

        public InMemorySnapshotStore()
            : this(new Dictionary<string, AggregateSnapshot<TAggregateRoot>>())
        {
        }

        public InMemorySnapshotStore(IDictionary<string, AggregateSnapshot<TAggregateRoot>> snapshots)
        {
            Snapshots = new ConcurrentDictionary<string, AggregateSnapshot<TAggregateRoot>>(snapshots);
        }

        public Task StoreSnapshotAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            var aggregateSnapshot = new AggregateSnapshot<TAggregateRoot>(aggregateRoot.DeepCloneGeneric());
            Snapshots[aggregateRoot.Id] = aggregateSnapshot;
            return Task.CompletedTask;
        }

        public Task<TAggregateRoot?> LoadSnapshotAsync(string aggregateRootId, CancellationToken ct)
        {
            if (Snapshots.TryGetValue(aggregateRootId, out var aggregateSnapshot))
            {
                return Task.FromResult<TAggregateRoot?>(aggregateSnapshot.AggregateState);
            }
            else
            {
                return Task.FromResult<TAggregateRoot?>(null);
            }
        }
    }
}