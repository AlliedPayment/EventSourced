using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Diagnostics.Web.Mappers;
using EventSourced.Diagnostics.Web.Model.Aggregates;
using EventSourced.Domain;
using EventSourced.Domain.Events;
using EventSourced.Helpers;
using EventSourced.Persistence;
using Newtonsoft.Json;

namespace EventSourced.Diagnostics.Web.Services
{
    public class AggregateInformationService : IAggregateInformationService
    {
        private readonly IEventStore _eventStore;
        private readonly IAggregateInstancesListItemModelMapper _aggregateInstancesListItemModelMapper;

        public AggregateInformationService(IEventStore eventStore, IAggregateInstancesListItemModelMapper aggregateInstancesListItemModelMapper)
        {
            _eventStore = eventStore;
            _aggregateInstancesListItemModelMapper = aggregateInstancesListItemModelMapper;
        }

        public async Task<ICollection<AggregateTypesListItemModel>> GetStoredAggregateTypesAsync(CancellationToken ct)
        {
            var types = await _eventStore.GetAllAggregateTypes(ct);
            return types.Select(t => new AggregateTypesListItemModel(t))
                        .ToList();
        }

        public async Task<ICollection<AggregateInstancesListItemModel>> GetStoredAggregatesOfTypeAsync(Type aggregateType, CancellationToken ct)
        {
            var streams = await _eventStore.GetAllStreamsOfType(aggregateType, ct);
            var aggregates = RebuildAggregatesFromStreams(streams, aggregateType);
            return aggregates.Select((i) => _aggregateInstancesListItemModelMapper.MapToModel(i.aggregateRoot, i.events))
                             .ToList();
        }

        public async Task<AggregateInstancesListItemModel> GetStoredAggregateByIdAndVersionAsync(string aggregateId, Type aggregateType, int version, CancellationToken ct)
        {
            var stream = await _eventStore.GetByStreamIdAsync(aggregateId, aggregateType, 0, ct);
            var versionedStream = stream.Where(e => e.Version <= version)
                                     .ToArray();
            var item = RebuildAggregateFromStream(aggregateType, aggregateId, versionedStream);
            return _aggregateInstancesListItemModelMapper.MapToModel(item.aggregateRoot, item.events);
        }

        private IEnumerable<(AggregateRoot aggregateRoot, DomainEvent[] events)> RebuildAggregatesFromStreams(IDictionary<string, DomainEvent[]> streams, Type aggregateType)
        {
            foreach (var (streamId, events) in streams)
            {
                yield return RebuildAggregateFromStream(aggregateType, streamId, events);
            }
        }

        private static (AggregateRoot aggregateRoot, DomainEvent[] events) RebuildAggregateFromStream(Type aggregateType, string streamId, DomainEvent[] events)
        {
            var aggregateRoot = AggregateRootFactory.CreateAggregateRoot(streamId, aggregateType);
            aggregateRoot.RebuildFromEvents(events);
            return (aggregateRoot, events);
        }
    }
}