using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Persistence;
using EventSourced.Sample.Warehouse.Domain.Container;

namespace EventSourced.Sample.Warehouse.Application.Services.Container
{
    class ConsumeItemsFromContainerApplicationService : ApplicationServiceBase, IConsumeItemsFromContainerApplicationService
    {
        private readonly IRepository<ContainerAggregateRoot> _containerRepository;

        public ConsumeItemsFromContainerApplicationService(IRepository<ContainerAggregateRoot> containerRepository)
        {
            _containerRepository = containerRepository;
        }

        public async Task ConsumeItemsAsync(Guid containerId, Guid warehouseItemId, int amount, CancellationToken ct)
        {
            var container = await _containerRepository.GetByIdAsync(containerId.ToString(), ct);
            container.RemoveItemFromContainer(warehouseItemId.ToString(), amount);
            await _containerRepository.SaveAsync(container, ct);
        }
    }
}