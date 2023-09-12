using System;
using EventSourced.Domain;

namespace EventSourced.Projections
{
    public abstract class AggregateProjection<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        public string Id { get; }

        protected AggregateProjection(string id)
        {
            Id = id;
        }
    }
}