using System;
using System.Collections.Generic;
using System.Linq;
using EventSourced.Domain.Events;
using EventSourced.Helpers;

namespace EventSourced.Domain
{
    public abstract class AggregateRoot
    {
        private readonly Queue<DomainEvent> uncommittedDomainEvents;

        public int Version { get; internal set; }

        public string Id { get; }

        protected AggregateRoot(string id)
        {
            Id = id;
            uncommittedDomainEvents = new Queue<DomainEvent>();
        }

        public IList<DomainEvent> DequeueDomainEvents()
        {
            return uncommittedDomainEvents.DequeueAll();
        }

        protected void EnqueueAndApplyEvent(DomainEvent domainEvent)
        {
            domainEvent.Version = GetNextEventVersion();
            uncommittedDomainEvents.Enqueue(domainEvent);
            ApplyEvent(domainEvent);
        }

        private int GetNextEventVersion()
        {
            var lastAppliedEventVersion = uncommittedDomainEvents.LastOrDefault()
                                                                 ?.Version ?? Version;
            return lastAppliedEventVersion + 1;
        }

        public void RebuildFromEvents(IEnumerable<DomainEvent> domainEvents)
        {
            foreach (var domainEvent in domainEvents)
            {
                ApplyEvent(domainEvent);
                Version = domainEvent.Version;
            }
        }

        private void ApplyEvent(DomainEvent domainEvent)
        {
            this.ApplyEventsToObject(domainEvent);
        }
    }
}