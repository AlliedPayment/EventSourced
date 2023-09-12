using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Persistence;

namespace EventSourced.Sample.Warehouse.Domain.Container.Services
{
    public class MoveItemBetweenContainersService : IMoveItemBetweenContainersService
    {
        private readonly IRepository<ContainerAggregateRoot> _containerRepository;

        public MoveItemBetweenContainersService(IRepository<ContainerAggregateRoot> containerRepository)
        {
            _containerRepository = containerRepository;
        }

        public async  Task MoveItemBetweenContainersAsync(Guid sourceContainerId, Guid destinationContainerId, Guid warehouseItemId, int amount, CancellationToken ct)
        {
            var sourceContainer = await _containerRepository.GetByIdAsync(sourceContainerId.ToString(), ct);
            var destinationContainer = await _containerRepository.GetByIdAsync(destinationContainerId.ToString(), ct);
            
            sourceContainer.RemoveItemFromContainer(warehouseItemId.ToString(), amount);
            destinationContainer.MoveItemToContainer(warehouseItemId, amount);
            
            await _containerRepository.SaveAsync(sourceContainer, ct);
            await _containerRepository.SaveAsync(destinationContainer, ct);
        } 
    }
}