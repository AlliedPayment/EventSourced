using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Persistence;
using EventSourced.Sample.Warehouse.Domain.Container;

namespace EventSourced.Sample.Warehouse.Application.Services.Container
{
    class MoveItemsBetweenContainersApplicationService : ApplicationServiceBase, IMoveItemsBetweenContainersApplicationService
    {
        private readonly IRepository<ContainerAggregateRoot> _containerRepository;

        public MoveItemsBetweenContainersApplicationService(IRepository<ContainerAggregateRoot> containerRepository)
        {
            _containerRepository = containerRepository;
        }

        public async Task MoveItemBetweenContainersAsync(string sourceContainerId,
                                                         string destinationContainerId,
                                                         string  warehouseItemId,
                                                         int amount,
                                                         CancellationToken ct)
        {
            var sourceContainer = await _containerRepository.GetByIdAsync(sourceContainerId, ct);
            var destinationContainer = await _containerRepository.GetByIdAsync(destinationContainerId, ct);
            sourceContainer.RemoveItemFromContainer(warehouseItemId, amount);
            destinationContainer.MoveItemToContainer(Guid.Parse(warehouseItemId), amount);
            await _containerRepository.SaveAsync(sourceContainer, ct);
            await _containerRepository.SaveAsync(destinationContainer, ct);
        }
    }
}