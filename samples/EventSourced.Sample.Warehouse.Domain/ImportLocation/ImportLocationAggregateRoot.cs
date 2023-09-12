using System;
using System.Collections.Generic;
using System.Linq;
using EventSourced.Domain;
using EventSourced.Sample.Warehouse.Domain.Exceptions;
using EventSourced.Sample.Warehouse.Domain.ImportLocation.Events;

namespace EventSourced.Sample.Warehouse.Domain.ImportLocation
{
    public class ImportLocationAggregateRoot : AggregateRoot
    {
        public ICollection<ImportedItemsValueObject> ImportedItems { get; private set; } = new List<ImportedItemsValueObject>();

        internal ImportLocationAggregateRoot() : base(Guid.NewGuid().ToString())
        {
            EnqueueAndApplyEvent(new ImportLocationCreatedDomainEvent(Guid.Parse(Id)));
        }
        
        public ImportLocationAggregateRoot(string id)
            : base(id)
        {
        }

        public void ImportWarehouseItem(Guid warehouseItemId, int amount)
        {
            if (amount <= 0)
            {
                throw new BusinessRuleException("Number of imported items has to be greater than zero.");
            }

            EnqueueAndApplyEvent(new NewItemsImportedDomainEvent(warehouseItemId, amount));
        }

        public void MoveItem(Guid destinationContainerId, Guid warehouseItemId, int amount)
        {
            var importedItem = ImportedItems.SingleOrDefault(i => i.WarehouseItemId == warehouseItemId);
            if (importedItem == null)
            {
                throw new BusinessRuleException($"Warehouse item with id {warehouseItemId} is not present at import location");
            }
            
            if (importedItem.Amount < amount)
            {
                throw new BusinessRuleException($"Removed warehouse item amount is greater then existing");
            }
            
            EnqueueAndApplyEvent(new ItemMovedFromImportLocationDomainEvent(warehouseItemId, destinationContainerId, amount));
        }

        private void Apply(ImportLocationCreatedDomainEvent domainEvent)
        {
        }
        
        private void Apply(NewItemsImportedDomainEvent domainEvent)
        {
            var existingValueObject = ImportedItems.SingleOrDefault(i => i.WarehouseItemId == domainEvent.WarehouseItemId);
            if (existingValueObject != null)
            {
                ImportedItems.Remove(existingValueObject);
            }
            var existingAmount = existingValueObject?.Amount ?? 0;
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