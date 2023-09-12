using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Persistence.InMemory.Helpers;

namespace EventSourced.Persistence.InMemory
{
    public class InMemoryProjectionStore : IProjectionStore
    {
        private ConcurrentDictionary<Type, object> ProjectionsMap { get; }
        private ConcurrentDictionary<Type, ConcurrentDictionary<string, object>> AggregateProjectionsMap { get; }

        public InMemoryProjectionStore()
            : this(new ConcurrentDictionary<Type, object>())
        {
        }

        public InMemoryProjectionStore(ConcurrentDictionary<Type, object> projectionsMap)
        {
            ProjectionsMap = projectionsMap;
            AggregateProjectionsMap = new ConcurrentDictionary<Type, ConcurrentDictionary<string, object>>();
        }

        public Task<object?> LoadProjectionAsync(Type projectionType, CancellationToken ct)
        {
            var projection = ProjectionsMap.GetValueOrDefault(projectionType);
            return Task.FromResult(projection);
        }

        public Task<ICollection<object>> LoadAllProjectionsAsync(CancellationToken ct)
        {
            var projection = ProjectionsMap.Values;
            return Task.FromResult<ICollection<object>>(projection.ToList());
        }

        public Task<object?> LoadAggregateProjectionAsync(Type projectionType, string aggregateRootId, CancellationToken ct)
        {
            if (AggregateProjectionsMap.TryGetValue(projectionType, out var projectionByIdDictionary))
            {
                if (projectionByIdDictionary.TryGetValue(aggregateRootId, out var projection))
                {
                    return Task.FromResult<object?>(projection);
                }
            }
            return Task.FromResult<object?>(null);
        }

        public Task<IDictionary<string, List<object>>> LoadAllAggregateProjectionsAsync(CancellationToken ct)
        {
            var aggregateIdToProjectionsMap = new Dictionary<string, List<object>>();
            foreach (var projectionsByAggregateId in AggregateProjectionsMap.Values)
            {
                foreach (var (aggregateId, projection) in projectionsByAggregateId)
                {
                    if (aggregateIdToProjectionsMap.ContainsKey(aggregateId))
                    {
                        aggregateIdToProjectionsMap[aggregateId]
                            .Add(projection);
                    }
                    else
                    {
                        aggregateIdToProjectionsMap[aggregateId] = new List<object> {projection};
                    }
                }
            }
            return Task.FromResult<IDictionary<string, List<object>>>(aggregateIdToProjectionsMap);
        }

        public Task StoreProjectionAsync(object projection, CancellationToken ct)
        {
            ProjectionsMap[projection.GetType()] = projection.DeepClone();
            return Task.CompletedTask;
        }

        public Task StoreAggregateProjectionAsync(string streamId, object aggregateProjection, CancellationToken ct)
        {
            var projectionType = aggregateProjection.GetType();
            if (!AggregateProjectionsMap.ContainsKey(projectionType))
            {
                AggregateProjectionsMap[projectionType] = new ConcurrentDictionary<string, object>();
            }

            AggregateProjectionsMap[projectionType][streamId] = aggregateProjection.DeepClone();
            return Task.CompletedTask;
        }
    }
}