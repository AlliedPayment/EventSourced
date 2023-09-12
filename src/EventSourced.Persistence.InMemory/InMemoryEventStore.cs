using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Domain.Events;

namespace EventSourced.Persistence.InMemory
{
    public class InMemoryEventStore : IEventStore
    {
        private ConcurrentDictionary<StreamIdentification, List<DomainEvent>> StreamsDictionary { get; }

        public InMemoryEventStore()
            : this(new Dictionary<StreamIdentification, List<DomainEvent>>())
        {
        }

        public InMemoryEventStore(IDictionary<StreamIdentification, List<DomainEvent>> originalState)
        {
            StreamsDictionary = new ConcurrentDictionary<StreamIdentification, List<DomainEvent>>(originalState);
        }

        public Task StoreEventsAsync(string streamId, Type aggregateRootType, IList<DomainEvent> domainEvents, CancellationToken ct)
        {
            var streamIdentification = new StreamIdentification(streamId, aggregateRootType);
            StreamsDictionary.AddOrUpdate(streamIdentification,
                                          _ => domainEvents.ToList(),
                                          (_, existingEvents) => existingEvents.Concat(domainEvents)
                                                                               .ToList());
            return Task.CompletedTask;
        }

        public Task<DomainEvent[]> GetByStreamIdAsync(string streamId,
                                                       Type aggregateRootType,
                                                       int fromEventVersion,
                                                       CancellationToken ct)
        {
            var streamIdentification = new StreamIdentification(streamId, aggregateRootType);

            if (StreamsDictionary.TryGetValue(streamIdentification, out var events))
            {
                var eventsArray = events
                    .Where(e => e.Version > fromEventVersion)
                    .ToArray();
                return Task.FromResult(eventsArray);
            }

            throw new NotImplementedException();
        }

        public Task<bool> StreamExistsAsync(string streamId, Type aggregateRootType, CancellationToken ct)
        {
            var streamExists = StreamsDictionary.ContainsKey(new StreamIdentification(streamId, aggregateRootType));
            return Task.FromResult(streamExists);
        }

        public Task<IDictionary<string, DomainEvent[]>> GetAllStreamsOfType(Type aggregateRootType, CancellationToken ct)
        {
            IDictionary<string, DomainEvent[]> allStreams = StreamsDictionary.Where(d => d.Key.AggregateRootType == aggregateRootType)
                                                                             .Select(d => new
                                                                             {
                                                                                 d.Key.StreamId, DomainEvents = d.Value.ToArray()
                                                                             })
                                                                             .ToDictionary(d => d.StreamId, d => d.DomainEvents);
            return Task.FromResult(allStreams);
        }

        public Task<DomainEvent[]> GetEventsOfTypeAsync(Type eventType, CancellationToken ct)
        {
            var events = StreamsDictionary.Values.SelectMany(v => v)
                                          .Where(v => v.GetType() == eventType)
                                          .ToArray();
            return Task.FromResult(events);
        }

        public Task<ICollection<Type>> GetAllAggregateTypes(CancellationToken ct)
        {
            var types = StreamsDictionary.Keys.Select(v => v.AggregateRootType)
                                    .Distinct()
                                    .ToList();
            return Task.FromResult<ICollection<Type>>(types);
        }
        
    }

    public record StreamIdentification(string StreamId, Type AggregateRootType);
}