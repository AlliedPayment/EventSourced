using System;

namespace EventSourced.Diagnostics.Web.Model.Aggregates
{
    public class AggregateInstancesListItemModel
    {
        public string Id { get; }
        public int Version { get; }
        public string SerializedAggregate { get; }
        public string SerializedEvents { get; }

        public AggregateInstancesListItemModel(string id, int version, string serializedAggregate, string serializedEvents)
        {
            Id = id;
            Version = version;
            SerializedAggregate = serializedAggregate;
            SerializedEvents = serializedEvents;
        }
    }
}