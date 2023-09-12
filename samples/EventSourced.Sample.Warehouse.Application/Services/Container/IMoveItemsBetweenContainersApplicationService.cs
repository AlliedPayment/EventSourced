using System;
using System.Threading;
using System.Threading.Tasks;

namespace EventSourced.Sample.Warehouse.Application.Services.Container
{
    public interface IMoveItemsBetweenContainersApplicationService
    {
        Task MoveItemBetweenContainersAsync(string sourceContainerId,
                                            string destinationContainer,
                                            string warehouseItemId,
                                            int amount,
                                            CancellationToken ct);
    }
}