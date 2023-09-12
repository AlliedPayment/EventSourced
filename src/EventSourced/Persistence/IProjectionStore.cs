using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourced.Persistence
{
    public interface IProjectionStore
    {
        public async Task<TProjection?> LoadProjectionAsync<TProjection>(CancellationToken ct)
        {
            return (TProjection?) await LoadProjectionAsync(typeof(TProjection), ct);
        }

        Task<object?> LoadProjectionAsync(Type projectionType, CancellationToken ct);
        Task<ICollection<object>> LoadAllProjectionsAsync(CancellationToken ct);
        Task StoreProjectionAsync(object projection, CancellationToken ct);

        public async Task<TAggregateProjection?> LoadAggregateProjectionAsync<TAggregateProjection, TAggregateRoot>(
            string aggregateRootId,
            CancellationToken ct)
        {
            return (TAggregateProjection?) await LoadAggregateProjectionAsync(typeof(TAggregateProjection), aggregateRootId, ct);
        }

        Task<object?> LoadAggregateProjectionAsync(Type projectionType, string aggregateRootId, CancellationToken ct);
        Task<IDictionary<string, List<object>>> LoadAllAggregateProjectionsAsync(CancellationToken ct);
        Task StoreAggregateProjectionAsync(string streamId, object aggregateProjection, CancellationToken ct);
    }
}