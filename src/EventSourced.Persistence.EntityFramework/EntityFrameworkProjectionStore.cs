using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Persistence.EntityFramework.Helpers;
using EventSourced.Persistence.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;

namespace EventSourced.Persistence.EntityFramework
{
    public class EntityFrameworkProjectionStore : IProjectionStore
    {
        private readonly ITypeBasedProjectionEntityMapper _typeBasedProjectionEntityMapper;
        private readonly EventSourcedDbContext _eventSourcedDbContext;
        private readonly ITypeSerializer _typeSerializer;
        private readonly IAggregateBasedProjectionEntityMapper _aggregateBasedProjectionEntityMapper;

        public EntityFrameworkProjectionStore(ITypeBasedProjectionEntityMapper typeBasedProjectionEntityMapper,
                                              EventSourcedDbContext eventSourcedDbContext,
                                              ITypeSerializer typeSerializer,
                                              IAggregateBasedProjectionEntityMapper aggregateBasedProjectionEntityMapper)
        {
            _typeBasedProjectionEntityMapper = typeBasedProjectionEntityMapper;
            _eventSourcedDbContext = eventSourcedDbContext;
            _typeSerializer = typeSerializer;
            _aggregateBasedProjectionEntityMapper = aggregateBasedProjectionEntityMapper;
        }

        public async Task<object?> LoadProjectionAsync(Type projectionType, CancellationToken ct)
        {
            var serializedProjectionType = _typeSerializer.SerializeType(projectionType);
            var projectionEntity =
                await _eventSourcedDbContext.TypeBasedProjections.SingleOrDefaultAsync(
                    p => p.SerializedProjectionType == serializedProjectionType,
                    ct);
            return projectionEntity != null ? _typeBasedProjectionEntityMapper.MapToProjection(projectionEntity) : null;
        }

        public async Task<ICollection<object>> LoadAllProjectionsAsync(CancellationToken ct)
        {
            var projectionEntities = await _eventSourcedDbContext.TypeBasedProjections.ToListAsync(ct);
            return projectionEntities.Select(_typeBasedProjectionEntityMapper.MapToProjection)
                                     .ToList();
        }

        public async Task<object?> LoadAggregateProjectionAsync(Type projectionType, string aggregateRootId, CancellationToken ct)
        {
            var serializedProjectionType = _typeSerializer.SerializeType(projectionType);
            var projectionEntity = await _eventSourcedDbContext
                                         .AggregateBasedProjections.Where(p => p.SerializedProjectionType == serializedProjectionType)
                                         .Where(p => p.AggregateRootId == aggregateRootId)
                                         .SingleOrDefaultAsync(ct);
            return projectionEntity != null ? _aggregateBasedProjectionEntityMapper.MapToProjection(projectionEntity) : null;
        }

        public async Task<IDictionary<string, List<object>>> LoadAllAggregateProjectionsAsync(CancellationToken ct)
        {
            var projectionEntities = await _eventSourcedDbContext.AggregateBasedProjections.ToListAsync(ct);
            return projectionEntities.GroupBy(p => p.AggregateRootId)
                                     .ToDictionary(p => p.Key,
                                                   p => p
                                                        .Select(_aggregateBasedProjectionEntityMapper.MapToProjection)
                                                        .ToList());
        }

        public async Task StoreProjectionAsync(object projection, CancellationToken ct)
        {
            var projectionEntity = _typeBasedProjectionEntityMapper.MapToEntity(projection);
            if (await _eventSourcedDbContext.TypeBasedProjections.AsNoTracking()
                                            .AnyAsync(p => p.SerializedProjectionType == projectionEntity.SerializedProjectionType,
                                                      ct))
            {
                _eventSourcedDbContext.TypeBasedProjections.Update(projectionEntity);
            }
            else
            {
                await _eventSourcedDbContext.TypeBasedProjections.AddAsync(projectionEntity, ct);
            }
            await _eventSourcedDbContext.SaveChangesAsync(ct);
        }

        public async Task StoreAggregateProjectionAsync(string streamId, object aggregateProjection, CancellationToken ct)
        {
            var projectionEntity = _aggregateBasedProjectionEntityMapper.MapToEntity(streamId, aggregateProjection);
            if (await _eventSourcedDbContext.AggregateBasedProjections.AnyAsync(
                p => p.SerializedProjectionType == projectionEntity.SerializedProjectionType,
                ct))
            {
                _eventSourcedDbContext.AggregateBasedProjections.Update(projectionEntity);
            }
            else
            {
                await _eventSourcedDbContext.AggregateBasedProjections.AddAsync(projectionEntity, ct);
            }
            await _eventSourcedDbContext.SaveChangesAsync(ct);
        }
    }
}