using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Domain;

namespace EventSourced.Projections
{
    public interface IManualProjectionBuilder
    {
        Task<TProjection> BuildProjectionAsync<TProjection>(CancellationToken ct) where TProjection : new();

        Task<object> BuildProjectionAsync(Type projectionType, CancellationToken ct);

        Task<TAggregateProjection> BuildAggregateProjection<TAggregateProjection, TAggregateRoot>(string aggregateRootId,
            CancellationToken ct)
            where TAggregateProjection : AggregateProjection<TAggregateRoot> where TAggregateRoot : AggregateRoot;
    }
}