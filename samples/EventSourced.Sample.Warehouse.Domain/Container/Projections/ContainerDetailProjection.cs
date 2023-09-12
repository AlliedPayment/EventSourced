using System;
using System.Collections.Generic;
using EventSourced.Domain.Events;
using EventSourced.Projections;
using EventSourced.Sample.Warehouse.Domain.Container.Events;

namespace EventSourced.Sample.Warehouse.Domain.Container.Projections
{
    public class ContainerDetailProjection : AggregateProjection<ContainerAggregateRoot>,
                                             IApplyDomainEvent<ContainerCreatedDomainEvent>,
                                             IApplyDomainEvent<ItemMovedToContainerDomainEvent>,
                                             IApplyDomainEvent<ItemRemovedFromContainerDomainEvent>,
                                             IApplyDomainEvent<ReceivedItemFromImportLocationDomainEvent>
    {
        public string Identifier { get; private set; } = string.Empty;
        public IDictionary<Guid, int> ContainedWarehouseItems { get; private set; } = new Dictionary<Guid, int>();

        public ContainerDetailProjection(Guid id)
            : base(id.ToString())
        {
        }

        public void Apply(ContainerCreatedDomainEvent domainEvent)
        {
            Identifier = domainEvent.Identifier;
        }

        public void Apply(ItemMovedToContainerDomainEvent domainEvent)
        {
            var amount = RemoveContainedItemIfExists(domainEvent.WarehouseItemId);

            ContainedWarehouseItems.Add(domainEvent.WarehouseItemId, amount + domainEvent.Amount);
        }

        public void Apply(ItemRemovedFromContainerDomainEvent domainEvent)
        {
            var amount = RemoveContainedItemIfExists(domainEvent.WarehouseItemId);

            if (amount != domainEvent.Amount)
            {
                ContainedWarehouseItems.Add(domainEvent.WarehouseItemId, amount - domainEvent.Amount);
            }
        }

        public void Apply(ReceivedItemFromImportLocationDomainEvent domainEvent)
        {
            var existingAmount = RemoveContainedItemIfExists(domainEvent.WarehouseItemId);

            ContainedWarehouseItems.Add(domainEvent.WarehouseItemId, existingAmount + domainEvent.Amount);
        }

        private int RemoveContainedItemIfExists(Guid warehouseItemId)
        {
            if (ContainedWarehouseItems.TryGetValue(warehouseItemId, out var amount))
            {
                ContainedWarehouseItems.Remove(warehouseItemId);
            }
            return amount;
        }
    }
}