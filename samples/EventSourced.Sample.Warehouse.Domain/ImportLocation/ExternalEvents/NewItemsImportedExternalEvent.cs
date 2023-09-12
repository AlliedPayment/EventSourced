using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.ExternalEvents;
using EventSourced.Persistence;
using EventSourced.Sample.Warehouse.Domain.ImportLocation.Projections;

namespace EventSourced.Sample.Warehouse.Domain.ImportLocation.ExternalEvents
{
    public record NewItemsImportedExternalEvent(Guid WarehouseItemId, int Amount);
    
    public class NewItemsImportedExternalEventHandler : IExternalEventHandler<NewItemsImportedExternalEvent>
    {
        private readonly IProjectionStore _projectionStore;
        private readonly IRepository<ImportLocationAggregateRoot> _importLocationRepository;

        public NewItemsImportedExternalEventHandler(IProjectionStore projectionStore, IRepository<ImportLocationAggregateRoot> importLocationRepository)
        {
            _projectionStore = projectionStore;
            _importLocationRepository = importLocationRepository;
        }

        public async Task HandleAsync(NewItemsImportedExternalEvent externalEvent, CancellationToken ct)
        {
            var importLocationProjection = await _projectionStore.LoadProjectionAsync<ImportLocationProjection>(ct);
            
            if (importLocationProjection == null)
            {
                throw new ArgumentException("Import location projection was not found in the system.");
            }
            
            var importLocation = await _importLocationRepository.GetByIdAsync(importLocationProjection.ImportLocationId.ToString(), ct);
            importLocation.ImportWarehouseItem(externalEvent.WarehouseItemId, externalEvent.Amount);
            await _importLocationRepository.SaveAsync(importLocation, ct);
        }
    }
}