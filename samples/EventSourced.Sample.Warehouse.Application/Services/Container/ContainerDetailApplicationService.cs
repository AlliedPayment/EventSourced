using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Persistence;
using EventSourced.Projections;
using EventSourced.Sample.Warehouse.Application.Model;
using EventSourced.Sample.Warehouse.Domain.Container;
using EventSourced.Sample.Warehouse.Domain.Container.Projections;
using EventSourced.Sample.Warehouse.Domain.WarehouseItem.Projections;
using EventSourced.Sample.Warehouse.Domain.WarehouseItem.Services;

namespace EventSourced.Sample.Warehouse.Application.Services.Container
{
    class ContainerDetailApplicationService : ApplicationServiceBase, IContainerDetailApplicationService
    {
        private readonly IManualProjectionBuilder _manualProjectionBuilder;
        private readonly IProjectionStore _projectionStore;
        private readonly IWarehouseItemTitleService _warehouseItemTitleService;

        public ContainerDetailApplicationService(IManualProjectionBuilder manualProjectionBuilder, IProjectionStore projectionStore, IWarehouseItemTitleService warehouseItemTitleService)
        {
            _manualProjectionBuilder = manualProjectionBuilder;
            _projectionStore = projectionStore;
            _warehouseItemTitleService = warehouseItemTitleService;
        }
        
        public async Task<ContainerDetailModel> GetContainerDetailAsync(String containerId, CancellationToken ct)
        {
            var containerDetailProjection = await _manualProjectionBuilder.BuildAggregateProjection<ContainerDetailProjection, ContainerAggregateRoot>(containerId, ct);
            var allWarehouseItems = await _projectionStore.LoadProjectionAsync<AllWarehouseItemsListProjection>(ct);

            List<ContainerContentListItemModel> list = new();
            foreach (var g in containerDetailProjection.ContainedWarehouseItems)
            {
                var warehouseItemTitle = await _warehouseItemTitleService.GetWarehouseItemTitleAsync(g.Key, ct);
                list.Add(new ContainerContentListItemModel(g.Key, warehouseItemTitle, g.Value));
            }
            return new ContainerDetailModel(containerId,
                                            containerDetailProjection.Identifier,
                                            list);
        }
    }
}