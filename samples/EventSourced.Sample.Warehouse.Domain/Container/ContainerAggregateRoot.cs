using System;
using System.Collections.Generic;
using System.Linq;
using EventSourced.Domain;
using EventSourced.Sample.Warehouse.Domain.Container.Events;
using EventSourced.Sample.Warehouse.Domain.Exceptions;

namespace EventSourced.Sample.Warehouse.Domain.Container
{
    public class ContainerAggregateRoot : AggregateRoot
    {
        public string Identifier { get; private set; } = string.Empty;
        public ICollection<WarehouseItemInContainerValueObject> WarehouseItemsInContainer { get; } =
            new List<WarehouseItemInContainerValueObject>();

        public ContainerAggregateRoot(string identifier)
            : base(Guid.NewGuid().ToString())
        {
            EnqueueAndApplyEvent(new ContainerCreatedDomainEvent(Guid.Parse(Id), identifier));                
        }   
        
        private ContainerAggregateRoot(Guid id)
            : base(id.ToString())
        {
        }

        public void ReceiveItemFromImportLocation(Guid warehouseItemId, int amount)
        {
            if (amount <= 0)
            {
                throw new BusinessRuleException("Can not receive negative amount from import location.");
            }
            
            EnqueueAndApplyEvent(new ReceivedItemFromImportLocationDomainEvent(warehouseItemId, amount));
        }

        public void RemoveItemFromContainer(string  warehouseItemId, int amount)
        {
            var warehouseItemInContainer = FindWarehouseItemInContainer(warehouseItemId);
            if (warehouseItemInContainer == null)
            {
                throw new BusinessRuleException($"Warehouse item with id {warehouseItemId} not found in container.");
            }

            if (warehouseItemInContainer.Amount < amount)
            {
                throw new BusinessRuleException(
                    $"Warehouse item amount is less than you want to move. Current amount is {warehouseItemInContainer.Amount}.");
            }
            
            EnqueueAndApplyEvent(new ItemRemovedFromContainerDomainEvent(Guid.Parse(warehouseItemId), amount));
        } 

        public void MoveItemToContainer(Guid warehouseItemId, int amount)
        {
            if (amount <= 0)
            {
                throw new BusinessRuleException("Can not receive negative amount to container.");
            }
            
            EnqueueAndApplyEvent(new ItemMovedToContainerDomainEvent(warehouseItemId, amount));
        } 
        
        private void AddOrUpdateAmountOfItemsInContainer(Guid warehouseItemId, int amount)
        {
            var existingWarehouseItem = FindWarehouseItemInContainer(warehouseItemId.ToString());
            if (existingWarehouseItem != null)
            {
                WarehouseItemsInContainer.Remove(existingWarehouseItem);
            }
            var existingItemsCount = existingWarehouseItem?.Amount ?? 0;
            WarehouseItemsInContainer.Add(new WarehouseItemInContainerValueObject(warehouseItemId.ToString(), amount + existingItemsCount));
        }

        private void RemoveAmountOfItemFromContainer(Guid warehouseItemId, int amount)
        {
            var warehouseItemInContainer = FindWarehouseItemInContainer(warehouseItemId.ToString());
            if (warehouseItemInContainer == null)
            {
                throw new BusinessRuleException($"Warehouse item with id {warehouseItemId} was not found in container.");
            }
            else
            {
                WarehouseItemsInContainer.Remove(warehouseItemInContainer);
            }

            if (warehouseItemInContainer.Amount > amount)
            {
                WarehouseItemsInContainer.Add(new WarehouseItemInContainerValueObject(warehouseItemId.ToString(), warehouseItemInContainer.Amount - amount));
            }
        }
        
        private WarehouseItemInContainerValueObject? FindWarehouseItemInContainer(string warehouseItemId)
        {
            return WarehouseItemsInContainer.SingleOrDefault(i => i.WarehouseItemId == warehouseItemId);
        }

        private void Apply(ItemRemovedFromContainerDomainEvent domainEvent)
        {
            RemoveAmountOfItemFromContainer(domainEvent.WarehouseItemId, domainEvent.Amount);
        }
        
        private void Apply(ReceivedItemFromImportLocationDomainEvent domainEvent)
        {
            AddOrUpdateAmountOfItemsInContainer(domainEvent.WarehouseItemId, domainEvent.Amount);
        }

        private void Apply(ItemMovedToContainerDomainEvent domainEvent)
        {
            AddOrUpdateAmountOfItemsInContainer(domainEvent.WarehouseItemId, domainEvent.Amount);
        }
        
        private void Apply(ContainerCreatedDomainEvent domainEvent)
        {
            Identifier = domainEvent.Identifier;
        }
    }
}