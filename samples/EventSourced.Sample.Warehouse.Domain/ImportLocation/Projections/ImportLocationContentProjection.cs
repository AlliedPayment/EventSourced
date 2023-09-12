using System;
using System.Collections.Generic;
using System.Linq;
using EventSourced.Projections;
using EventSourced.Sample.Warehouse.Domain.ImportLocation.Events;

namespace EventSourced.Sample.Warehouse.Domain.ImportLocation.Projections
{
    public class ImportLocationContentProjection : AggregateProjection<ImportLocationAggregateRoot>
    {
        public ICollection<ImportedItemsValueObject> ImportedItems { get; set; } = new List<ImportedItemsValueObject>();

        public ImportLocationContentProjection(Guid id)
            : base(id.ToString())
        {
        }

        private void Apply(NewItemsImportedDomainEvent domainEvent)
        {
            var existingItem = ImportedItems.SingleOrDefault(i => i.WarehouseItemId == domainEvent.WarehouseItemId);
            if (existingItem != null)
            {
                ImportedItems.Remove(existingItem);
            }
            var existingAmount = existingItem?.Amount ?? 0;
            ImportedItems.Add(new ImportedItemsValueObject(domainEvent.WarehouseItemId, domainEvent.Amount + existingAmount));
        }
        
        private void Apply(ItemMovedFromImportLocationDomainEvent domainEvent)
        {
            var existingValueObject = ImportedItems.Single(i => i.WarehouseItemId == domainEvent.WarehouseItemId);
            ImportedItems.Remove(existingValueObject);
            if (existingValueObject.Amount != domainEvent.Amount)
            {
                ImportedItems.Add(new ImportedItemsValueObject(domainEvent.WarehouseItemId, existingValueObject.Amount - domainEvent.Amount));
            }
        }
    }
}