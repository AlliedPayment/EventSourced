using System;

namespace EventSourced.Domain.Snapshosts
{
    public class AggregateSnapshot<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        public string Id { get; }
        public int Version { get; }
        public TAggregateRoot AggregateState { get; set; }

        public AggregateSnapshot(TAggregateRoot aggregateState)
        {
            Id = aggregateState.Id;
            Version = aggregateState.Version;
            AggregateState = aggregateState;
        }
    }
}