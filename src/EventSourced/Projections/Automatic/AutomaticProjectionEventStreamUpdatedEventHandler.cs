using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Domain.Events;
using EventSourced.Helpers;
using EventSourced.Persistence;

namespace EventSourced.Projections.Automatic
{
    internal class AutomaticProjectionEventStreamUpdatedEventHandler : IEventStreamUpdatedEventHandler
    {
        private readonly IAutomaticProjectionsEventMapper _automaticProjectionsEventMapper;

        private readonly IProjectionStore _projectionStore;

        public AutomaticProjectionEventStreamUpdatedEventHandler(IAutomaticProjectionsEventMapper automaticProjectionsEventMapper,
                                                     IProjectionStore projectionStore)
        {
            _automaticProjectionsEventMapper = automaticProjectionsEventMapper;
            _projectionStore = projectionStore;
        }

        public async Task HandleDomainEventAsync(Type aggregateRootType,
                                                 string aggregateRootId,
                                                 DomainEvent domainEvent,
                                                 CancellationToken ct)
        {
            var affectedProjections = _automaticProjectionsEventMapper.GetProjectionsAffectedByEvent(domainEvent);
            foreach (var projectionType in affectedProjections)
            {
                var projection = await LoadOrCreateProjection(projectionType, ct);
                projection!.ApplyEventsToObject(domainEvent);
                await _projectionStore.StoreProjectionAsync(projection, ct);
            }
        }

        private async Task<object> LoadOrCreateProjection(Type projectionType, CancellationToken ct)
        {
            var projection = await _projectionStore.LoadProjectionAsync(projectionType, ct);
            projection ??= Activator.CreateInstance(projectionType);
            if (projection == null)
            {
                throw new ArgumentException($"Could not create instance of type {projectionType.Name}");
            }
            return projection;
        }
    }
}