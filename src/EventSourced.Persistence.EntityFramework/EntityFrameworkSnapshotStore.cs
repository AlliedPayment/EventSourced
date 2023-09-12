using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Domain;
using EventSourced.Domain.Snapshosts;
using EventSourced.Persistence.EntityFramework.Helpers;
using EventSourced.Persistence.EntityFramework.Mappers;
using Microsoft.EntityFrameworkCore;

namespace EventSourced.Persistence.EntityFramework
{
    public class EntityFrameworkSnapshotStore<TAggregateRoot> : ISnapshotStore<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        private readonly IAggregateSnapshotEntityMapper _aggregateSnapshotEntityMapper;
        private readonly EventSourcedDbContext _dbContext;
        private readonly ITypeSerializer _typeSerializer;

        public EntityFrameworkSnapshotStore(IAggregateSnapshotEntityMapper aggregateSnapshotEntityMapper, EventSourcedDbContext dbContext, ITypeSerializer typeSerializer)
        {
            _aggregateSnapshotEntityMapper = aggregateSnapshotEntityMapper;
            _dbContext = dbContext;
            _typeSerializer = typeSerializer;
        }

        public async Task StoreSnapshotAsync(TAggregateRoot aggregateRoot, CancellationToken ct)
        {
            var aggregateSnapshot = new AggregateSnapshot<TAggregateRoot>(aggregateRoot);
            var aggregateSnapshotEntity = _aggregateSnapshotEntityMapper.MapToEntity(aggregateSnapshot);
            if (await _dbContext.AggregateSnapshots.AnyAsync(s => s.AggregateRootId == aggregateRoot.Id, cancellationToken: ct))
            {
                _dbContext.AggregateSnapshots.Update(aggregateSnapshotEntity);
            }
            else
            {
                await _dbContext.AggregateSnapshots.AddAsync(aggregateSnapshotEntity, ct);
            }
            await _dbContext.SaveChangesAsync(ct);
        }

        public async Task<TAggregateRoot?> LoadSnapshotAsync(string aggregateRootId, CancellationToken ct)
        {
            var aggregateType = typeof(TAggregateRoot);
            var serializedAggregateType = _typeSerializer.SerializeType(aggregateType);
            var existingSnapshot = await _dbContext.AggregateSnapshots.Where(s => s.AggregateRootType == serializedAggregateType)
                      .Where(s => s.AggregateRootId == aggregateRootId)
                      .SingleOrDefaultAsync(ct);
            return existingSnapshot != null
                ? _aggregateSnapshotEntityMapper.MapToAggregateRoot<TAggregateRoot>(existingSnapshot)
                : null;
        }
    }
}