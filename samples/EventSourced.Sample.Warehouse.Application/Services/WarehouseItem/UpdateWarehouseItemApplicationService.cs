using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Persistence;
using EventSourced.Sample.Warehouse.Domain.WarehouseItem;

namespace EventSourced.Sample.Warehouse.Application.Services.WarehouseItem
{
    public interface IUpdateWarehouseItemApplicationService
    {
        Task UpdateAsync(Guid warehouseItemId, string title, CancellationToken ct);
    }

    public class UpdateWarehouseItemApplicationService : ApplicationServiceBase, IUpdateWarehouseItemApplicationService
    {
        private readonly IRepository<WarehouseItemAggregateRoot> _warehouseItemRepository;

        public UpdateWarehouseItemApplicationService(IRepository<WarehouseItemAggregateRoot> warehouseItemRepository)
        {
            _warehouseItemRepository = warehouseItemRepository;
        }

        public async Task UpdateAsync(Guid warehouseItemId, string title, CancellationToken ct)
        {
            var warehouseItem = await _warehouseItemRepository.GetByIdAsync(warehouseItemId.ToString(), ct);
            warehouseItem.UpdateTitle(title);
            await _warehouseItemRepository.SaveAsync(warehouseItem, ct);
        }
    }
}