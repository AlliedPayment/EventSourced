using System;
using System.ComponentModel.DataAnnotations;

namespace EventSourced.Persistence.EntityFramework.Entities
{
    public class AggregateSnapshotEntity
    {
        [Key]
        public string AggregateRootId { get; set; }
        public int Version { get; set; }
        public string AggregateRootType { get; set; } = null!;
        public string SerializedAggregateState { get; set; } = null!;
    }
}