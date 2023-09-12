using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Persistence;
using EventSourced.Sample.Warehouse.Application.Model;
using EventSourced.Sample.Warehouse.Domain.WarehouseItem;
using EventSourced.Sample.Warehouse.Domain.WarehouseItem.Projections;

namespace EventSourced.Sample.Warehouse.Application.Services.WarehouseItem
{
    public interface IWarehouseItemDetailApplicationService
    {
        Task<WarehouseItemDetailModel> GetWarehouseItemDetailAsync(Guid warehouseItemId, CancellationToken ct);
    }

    public class WarehouseItemDetailApplicationService : ApplicationServiceBase, IWarehouseItemDetailApplicationService
    {
        private readonly IProjectionStore _projectionStore;

        public WarehouseItemDetailApplicationService(IProjectionStore projectionStore)
        {
            _projectionStore = projectionStore;
        }

        public async Task<WarehouseItemDetailModel> GetWarehouseItemDetailAsync(Guid warehouseItemId, CancellationToken ct)
        {
            var warehouseItemDetailProjection =
                await _projectionStore.LoadAggregateProjectionAsync<WarehouseItemDetailProjection, WarehouseItemAggregateRoot>(
                    warehouseItemId.ToString(),
                    ct);
            if (warehouseItemDetailProjection == null)
            {
                throw new ArgumentException();
            }
            return new WarehouseItemDetailModel(Guid.Parse(warehouseItemDetailProjection.Id), warehouseItemDetailProjection.Title);
        }
    }
}