using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Persistence;
using EventSourced.Sample.Warehouse.Domain.Exceptions;
using EventSourced.Sample.Warehouse.Domain.WarehouseItem.Projections;

namespace EventSourced.Sample.Warehouse.Domain.WarehouseItem.Services
{
    class WarehouseItemTitleService : IWarehouseItemTitleService
    {
        private readonly IProjectionStore _projectionStore;

        public WarehouseItemTitleService(IProjectionStore projectionStore)
        {
            _projectionStore = projectionStore;
        }

        public async Task<string> GetWarehouseItemTitleAsync(Guid warehouseItemId, CancellationToken ct)
        {
            var warehouseItem = await _projectionStore.LoadAggregateProjectionAsync<WarehouseItemDetailProjection, WarehouseItemAggregateRoot>(
                warehouseItemId.ToString(),
                ct);
            if (warehouseItem == null)
            {
                throw new BusinessRuleException($"Warehouse item with id {warehouseItemId} was not found");
            }
            return warehouseItem.Title;
        }
    }
}