using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Domain;
using EventSourced.Domain.Events;
using EventSourced.EventBus;
using EventSourced.Exceptions;
using EventSourced.Helpers;
using EventSourced.Snapshots;

namespace EventSourced.Persistence
{
    public class Repository<TAggregateRoot> : IRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private readonly IEventStore _eventStore;
        private readonly IEnumerable<IEventStreamUpdatedEventHandler> _domainEventHandlers;
        private readonly ISnapshotStore<TAggregateRoot> _snapshotStore;
        private readonly ISnapshotCreationStrategy _snapshotCreationStrategy;
        private readonly IDomainEventBus _domainEventBus;

        public Repository(IEventStore eventStore,
                          IEnumerable<IEventStreamUpdatedEventHandler> domainEventHandlers,
                          ISnapshotStore<TAggregateRoot> snapshotStore,
                          ISnapshotCreationStrategy snapshotCreationStrategy,
                          IDomainEventBus domainEventBus)
        {
            _eventStore = eventStore;
            _domainEventHandlers = domainEventHandlers;
            _snapshotStore = snapshotStore;
            _snapshotCreationStrategy = snapshotCreationStrategy;
            _domainEventBus = domainEventBus;
        }

        public async Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            await VerifyAggregateDidNotChangeAsync(aggregateRoot, ct);
            var newDomainEvents = aggregateRoot.DequeueDomainEvents();
            await _eventStore.StoreEventsAsync(aggregateRoot.Id, typeof(TAggregateRoot), newDomainEvents, ct);
            await InvokeDomainEventHandlersAsync(aggregateRoot.Id, newDomainEvents, ct);
            await CreateSnapshotIfNeededAsync(aggregateRoot, newDomainEvents.LastOrDefault()?.Version ?? aggregateRoot.Version, ct);
            await _domainEventBus.PublishDomainEventsAsync(newDomainEvents, ct);
        }

        public async Task<TAggregateRoot> GetByIdAsync(string id, CancellationToken ct)
        {
            var aggregateRoot = await LoadFromSnapshotOrCreateAsync(id, ct);
            var domainEvents = await _eventStore.GetByStreamIdAsync(id, typeof(TAggregateRoot), aggregateRoot.Version, ct);
            aggregateRoot.RebuildFromEvents(domainEvents);
            return aggregateRoot;
        }

        public async Task<ICollection<TAggregateRoot>> GetAllAsync(CancellationToken ct)
        {
            var aggregateToEventsMap = await _eventStore.GetAllStreamsOfType(typeof(TAggregateRoot), ct);
            var aggregateCollection = new List<TAggregateRoot>();
            foreach (var (streamId, events) in aggregateToEventsMap)
            {
                var aggregateRoot = AggregateRootFactory.CreateAggregateRoot<TAggregateRoot>(streamId);
                aggregateRoot.RebuildFromEvents(events);
                aggregateCollection.Add(aggregateRoot);
            }
            return aggregateCollection;
        }

        public Task<bool> ExistsAsync(string id, CancellationToken ct)
        {
            return _eventStore.StreamExistsAsync(id, typeof(TAggregateRoot), ct);
        }

        private async Task VerifyAggregateDidNotChangeAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            if (await _eventStore.StreamExistsAsync(aggregateRoot.Id, typeof(TAggregateRoot), ct))
            {
                var existingAggregateRoot = await GetByIdAsync(aggregateRoot.Id, ct);
                if (existingAggregateRoot.Version != aggregateRoot.Version)
                {
                    throw new AggregateVersionConflictException(
                        $"Conflict occured when trying to save aggregate of type {typeof(TAggregateRoot).Name} version {existingAggregateRoot.Version} already exists.");
                }
            }
        }
        
        private async Task InvokeDomainEventHandlersAsync(string aggregateRootId, IList<DomainEvent> domainEvents, CancellationToken ct)
        {
            foreach (var domainEventHandler in _domainEventHandlers)
            foreach (var domainEvent in domainEvents)
            {
                await domainEventHandler.HandleDomainEventAsync(typeof(TAggregateRoot), aggregateRootId, domainEvent, ct);
            }
        }

        private async Task<TAggregateRoot> LoadFromSnapshotOrCreateAsync(string id, CancellationToken ct)
        {
            var aggregateFromSnapshot = await _snapshotStore.LoadSnapshotAsync(id, ct);
            return aggregateFromSnapshot ?? AggregateRootFactory.CreateAggregateRoot<TAggregateRoot>(id);
        }

        private async Task CreateSnapshotIfNeededAsync(TAggregateRoot aggregateRoot, int version, CancellationToken ct)
        {
            if (_snapshotCreationStrategy.ShouldCreateSnapshot(aggregateRoot))
            {
                aggregateRoot.Version = version;
                await _snapshotStore.StoreSnapshotAsync(aggregateRoot, ct);
            }
        }
    }
}