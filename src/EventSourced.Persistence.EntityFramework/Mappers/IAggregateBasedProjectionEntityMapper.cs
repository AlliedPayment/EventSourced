using System;
using EventSourced.Persistence.EntityFramework.Entities;

namespace EventSourced.Persistence.EntityFramework.Mappers
{
    public interface IAggregateBasedProjectionEntityMapper
    {
        AggregateBasedProjectionEntity MapToEntity(string aggregateRootId, object projection);
        object MapToProjection(AggregateBasedProjectionEntity entity);
    }
}