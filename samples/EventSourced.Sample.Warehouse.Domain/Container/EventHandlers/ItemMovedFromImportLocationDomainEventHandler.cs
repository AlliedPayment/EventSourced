using System.Threading;
using System.Threading.Tasks;
using EventSourced.Domain.Events;
using EventSourced.Persistence;
using EventSourced.Sample.Warehouse.Domain.ImportLocation.Events;

namespace EventSourced.Sample.Warehouse.Domain.Container.EventHandlers
{
    public class ItemMovedFromImportLocationDomainEventHandler : IDomainEventHandler<ItemMovedFromImportLocationDomainEvent>
    {
        private readonly IRepository<ContainerAggregateRoot> _containerRepository;

        public ItemMovedFromImportLocationDomainEventHandler(IRepository<ContainerAggregateRoot> containerRepository)
        {
            _containerRepository = containerRepository;
        }

        public async Task HandleDomainEventAsync(ItemMovedFromImportLocationDomainEvent domainEvent, CancellationToken ct)
        {
            var container = await _containerRepository.GetByIdAsync(domainEvent.DestinationContainerId.ToString(), ct);
            container.ReceiveItemFromImportLocation(domainEvent.WarehouseItemId, domainEvent.Amount);
            await _containerRepository.SaveAsync(container, ct);
        }
    }
}