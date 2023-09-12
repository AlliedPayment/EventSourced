using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Domain.Events;
using EventSourced.Persistence.EntityFramework.Helpers;
using EventSourced.Persistence.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;

namespace EventSourced.Persistence.EntityFramework
{
    public class EntityFrameworkEventStore : IEventStore
    {
        private readonly IDomainEventEntityMapper _domainEventEntityMapper;
        private readonly EventSourcedDbContext _dbContext;
        private readonly ITypeSerializer _typeSerializer;

        public EntityFrameworkEventStore(IDomainEventEntityMapper domainEventEntityMapper,
                                         EventSourcedDbContext dbContext,
                                         ITypeSerializer typeSerializer)
        {
            _domainEventEntityMapper = domainEventEntityMapper;
            _dbContext = dbContext;
            _typeSerializer = typeSerializer;
        }

        public async Task StoreEventsAsync(string streamId,
                                           Type aggregateRootType,
                                           IList<DomainEvent> domainEvents,
                                           CancellationToken ct)
        {
            foreach (var domainEvent in domainEvents)
            {
                var entity = _domainEventEntityMapper.MapToEntity(domainEvent, streamId, aggregateRootType);
                await _dbContext.Events.AddAsync(entity, ct);
            }
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<DomainEvent[]> GetByStreamIdAsync(string streamId,
                                                            Type aggregateRootType,
                                                            int fromEventVersion,
                                                            CancellationToken ct)
        {
            var serializedAggregateType = _typeSerializer.SerializeType(aggregateRootType);
            var eventEntities = await _dbContext.Events
                                                .Where(e => e.StreamId == streamId)
                                                .Where(e => e.AggregateRootType == serializedAggregateType)
                                                .Where(e => e.Version > fromEventVersion)
                                                .ToListAsync(ct);
            return eventEntities.Select(_domainEventEntityMapper.MapToDomainEvent)
                                .ToArray();
        }

        public Task<bool> StreamExistsAsync(string streamId, Type aggregateRootType, CancellationToken ct)
        {
            var serializedAggregateType = _typeSerializer.SerializeType(aggregateRootType);
            return _dbContext.Events.Where(e => e.StreamId == streamId)
                             .Where(e => e.AggregateRootType == serializedAggregateType)
                             .AnyAsync(ct);
        }

        public async Task<IDictionary<string, DomainEvent[]>> GetAllStreamsOfType(Type aggregateRootType, CancellationToken ct)
        {
            var serializedAggregateType = _typeSerializer.SerializeType(aggregateRootType);
            var eventEntities = await _dbContext.Events.Where(e => e.AggregateRootType == serializedAggregateType)
                                                .ToListAsync(ct);
            return eventEntities.GroupBy(e => e.StreamId)
                                .ToDictionary(e => e.Key,
                                              g => g.Select(_domainEventEntityMapper.MapToDomainEvent)
                                                    .ToArray());
        }

        public async Task<DomainEvent[]> GetEventsOfTypeAsync(Type eventType, CancellationToken ct)
        {
            var serializedEventType = _typeSerializer.SerializeType(eventType);
            var eventEntities = await _dbContext.Events.Where(e => e.EventType == serializedEventType)
                                                .ToArrayAsync(ct);
            return eventEntities.Select(_domainEventEntityMapper.MapToDomainEvent)
                                .ToArray();
        }

        public async Task<ICollection<Type>> GetAllAggregateTypes(CancellationToken ct)
        {
            var serializedTypes = await _dbContext.Events.Select(e => e.AggregateRootType)
                      .Distinct()
                      .ToListAsync(ct);
            return serializedTypes.Select(_typeSerializer.DeserializeType)
                                  .ToList();
        }
    }
}