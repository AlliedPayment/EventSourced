using System;
using EventSourced.Projections;
using EventSourced.Sample.Warehouse.Domain.WarehouseItem.Events;

namespace EventSourced.Sample.Warehouse.Domain.WarehouseItem.Projections
{
    public class WarehouseItemDetailProjection : AggregateProjection<WarehouseItemAggregateRoot>
    {
        public string Title { get; private set; } = string.Empty;

        public WarehouseItemDetailProjection(string id)
            : base(id)
        {
        }

        private void Apply(WarehouseItemCreatedDomainEvent @event)
        {
            Title = @event.Title;
        }

        private void Apply(WarehouseItemTitleUpdatedDomainEvent @event)
        {
            Title = @event.NewTitle;
        }
    }
}