using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Persistence;
using EventSourced.Projections;
using EventSourced.Sample.Warehouse.Application.Model;
using EventSourced.Sample.Warehouse.Domain.Exceptions;
using EventSourced.Sample.Warehouse.Domain.ImportLocation;
using EventSourced.Sample.Warehouse.Domain.ImportLocation.Projections;
using EventSourced.Sample.Warehouse.Domain.WarehouseItem.Projections;
using EventSourced.Sample.Warehouse.Domain.WarehouseItem.Services;

namespace EventSourced.Sample.Warehouse.Application.Services.ImportLocation
{
    class GetImportLocationDataApplicationService : ApplicationServiceBase, IGetImportLocationDataApplicationService
    {
        private readonly IProjectionStore _projectionStore;
        private readonly IManualProjectionBuilder _manualProjectionBuilder;
        private readonly IWarehouseItemTitleService _warehouseItemTitleService;

        public GetImportLocationDataApplicationService(IProjectionStore projectionStore,
                                                       IManualProjectionBuilder manualProjectionBuilder,
                                                       IWarehouseItemTitleService warehouseItemTitleService)
        {
            _projectionStore = projectionStore;
            _manualProjectionBuilder = manualProjectionBuilder;
            _warehouseItemTitleService = warehouseItemTitleService;
        }

        public async Task<ICollection<ImportLocationContentListItemModel>> GetImportLocationContentAsync(CancellationToken ct)
        {
            var importLocationProjection = await _projectionStore.LoadProjectionAsync<ImportLocationProjection>(ct);
            if (importLocationProjection == null)
            {
                throw new BusinessRuleException("Import location was not found");
            }
            var importLocationContentProjection = await _manualProjectionBuilder.BuildAggregateProjection<ImportLocationContentProjection, ImportLocationAggregateRoot>(importLocationProjection.ImportLocationId.ToString(), ct);
            var contents = new List<ImportLocationContentListItemModel>();
            foreach (var importedItem in importLocationContentProjection.ImportedItems)
            {
                var warehouseItemTitle = await _warehouseItemTitleService.GetWarehouseItemTitleAsync(importedItem.WarehouseItemId, ct);
                contents.Add(new ImportLocationContentListItemModel(importedItem.WarehouseItemId,
                                                                    warehouseItemTitle,
                                                                    importedItem.Amount));
            }
            return contents;
        }
    }
}