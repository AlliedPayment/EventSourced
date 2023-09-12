using System;
using EventSourced.Domain.Events;
using EventSourced.Persistence.EntityFramework.Entities;

namespace EventSourced.Persistence.EntityFramework.Mappers
{
    public interface IDomainEventEntityMapper
    {
        DomainEventEntity MapToEntity(DomainEvent domainEvent, string streamId, Type aggregateRootType);
        DomainEvent MapToDomainEvent(DomainEventEntity domainEventEntity);
    }
}