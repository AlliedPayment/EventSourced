using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Domain.Events;

namespace EventSourced.Persistence
{
    public interface IEventStore
    {
        Task StoreEventsAsync(string streamId, Type aggregateRootType, IList<DomainEvent> domainEvents, CancellationToken ct);
        Task<DomainEvent[]> GetByStreamIdAsync(string streamId, Type aggregateRootType, int fromEventVersion, CancellationToken ct);
        Task<bool> StreamExistsAsync(string streamId, Type aggregateRootType, CancellationToken ct);
        Task<IDictionary<string, DomainEvent[]>> GetAllStreamsOfType(Type aggregateRootType, CancellationToken ct);
        Task<DomainEvent[]> GetEventsOfTypeAsync(Type eventType, CancellationToken ct);
        Task<ICollection<Type>> GetAllAggregateTypes(CancellationToken ct);
    }
}