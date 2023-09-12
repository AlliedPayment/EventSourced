using System;
using EventSourced.Domain;

namespace EventSourced.Sample.Warehouse.Domain.Container
{
    public class WarehouseItemInContainerValueObject : ValueObject
    {
        public string WarehouseItemId { get; }
        public int Amount { get;}

        public WarehouseItemInContainerValueObject(string warehouseItemId, int amount)
        {
            WarehouseItemId = warehouseItemId;
            Amount = amount;
        }
    }
}