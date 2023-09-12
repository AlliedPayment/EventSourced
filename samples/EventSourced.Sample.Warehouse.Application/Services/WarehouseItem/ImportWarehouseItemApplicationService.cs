using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Persistence;
using EventSourced.Sample.Warehouse.Domain.Exceptions;
using EventSourced.Sample.Warehouse.Domain.ImportLocation;
using EventSourced.Sample.Warehouse.Domain.ImportLocation.Projections;

namespace EventSourced.Sample.Warehouse.Application.Services.WarehouseItem
{
    class ImportWarehouseItemApplicationService : ApplicationServiceBase, IImportWarehouseItemApplicationService
    {
        private readonly IProjectionStore _projectionStore;
        private readonly IRepository<ImportLocationAggregateRoot> _importLocationRepository;

        public ImportWarehouseItemApplicationService(IProjectionStore projectionStore,
                                                     IRepository<ImportLocationAggregateRoot> importLocationRepository)
        {
            _projectionStore = projectionStore;
            _importLocationRepository = importLocationRepository;
        }

        public async Task ImportWarehouseItemAsync(Guid warehouseItemId, int amount, CancellationToken ct)
        {
            var importLocationProjection = await _projectionStore.LoadProjectionAsync<ImportLocationProjection>(ct);
            if (importLocationProjection == null)
            {
                throw new BusinessRuleException("Import location not found.");
            }
            var importLocationId = importLocationProjection.ImportLocationId;
            var importLocation = await _importLocationRepository.GetByIdAsync(importLocationId.ToString(), ct);
            importLocation.ImportWarehouseItem(warehouseItemId, amount);
            await _importLocationRepository.SaveAsync(importLocation, ct);
        }
    }
}