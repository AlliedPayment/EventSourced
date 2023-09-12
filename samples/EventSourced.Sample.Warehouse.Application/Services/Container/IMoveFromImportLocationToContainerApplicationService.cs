using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Persistence;
using EventSourced.Sample.Warehouse.Domain.Container;
using EventSourced.Sample.Warehouse.Domain.Exceptions;
using EventSourced.Sample.Warehouse.Domain.ImportLocation;
using EventSourced.Sample.Warehouse.Domain.ImportLocation.Projections;

namespace EventSourced.Sample.Warehouse.Application.Services.Container
{
    public interface IMoveFromImportLocationToContainerApplicationService
    {
        Task MoveFromImportLocationToContainerAsync(Guid containerId, Guid warehouseItemId, int amount, CancellationToken ct);
    }

    class MoveFromImportLocationToContainerApplicationService : ApplicationServiceBase, IMoveFromImportLocationToContainerApplicationService
    {
        private readonly IRepository<ImportLocationAggregateRoot> _importLocationRepository;
        private readonly IRepository<ContainerAggregateRoot> _containerRepository;
        private readonly IProjectionStore _projectionStore;

        public MoveFromImportLocationToContainerApplicationService(IRepository<ImportLocationAggregateRoot> importLocationRepository, IRepository<ContainerAggregateRoot> containerRepository, IProjectionStore projectionStore)
        {
            _importLocationRepository = importLocationRepository;
            _containerRepository = containerRepository;
            _projectionStore = projectionStore;
        }
        
        public async Task MoveFromImportLocationToContainerAsync(Guid containerId, Guid warehouseItemId, int amount, CancellationToken ct)
        {
            var importLocationProjection = await _projectionStore.LoadProjectionAsync<ImportLocationProjection>(ct);
            if (importLocationProjection == null)
            {
                throw new BusinessRuleException("Import location was not found in system.");
            }
            var importLocation = await _importLocationRepository.GetByIdAsync(importLocationProjection.ImportLocationId.ToString(), ct);

            importLocation.MoveItem(containerId, warehouseItemId, amount);

            await _importLocationRepository.SaveAsync(importLocation, ct);
        }
    }
}