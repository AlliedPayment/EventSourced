using System;
using Newtonsoft.Json;

namespace EventSourced.Diagnostics.Web.Model.Projections
{
    public class AggregateProjectionValueModel
    {
        public string AggregateId { get; }
        public string SerializedProjection { get; }

        public AggregateProjectionValueModel(string aggregateId, string serializedProjection)
        {
            AggregateId = aggregateId;
            SerializedProjection = serializedProjection;
        }
    }
}