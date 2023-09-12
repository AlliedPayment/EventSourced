using System;

namespace EventSourced.Persistence.EntityFramework.Entities
{
    public class DomainEventEntity
    {
        public Guid Id { get; set; }
        public string EventType { get; set; } = null!;
        public string StreamId { get; set; }
        public string AggregateRootType { get; set; } = null!;
        public string SerializedEvent { get; set; } = null!;
        public int Version { get; set; }
    }
}