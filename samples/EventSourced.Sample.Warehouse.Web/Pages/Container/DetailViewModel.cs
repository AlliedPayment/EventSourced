using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using EventSourced.Sample.Warehouse.Application.Model;
using EventSourced.Sample.Warehouse.Application.Services.Container;
using EventSourced.Sample.Warehouse.Web.Models;

namespace EventSourced.Sample.Warehouse.Web.Pages.Container
{
    public class DetailViewModel : MasterPageViewModel
    {
        private readonly IContainerDetailApplicationService _containerDetailApplicationService;
        private readonly IConsumeItemsFromContainerApplicationService _consumeItemsFromContainerApplicationService;
        private readonly IGetAllContainersApplicationService _getAllContainersApplicationService;
        private readonly IMoveItemsBetweenContainersApplicationService _moveItemsBetweenContainersApplicationService;

        public DetailViewModel(IContainerDetailApplicationService containerDetailApplicationService, IConsumeItemsFromContainerApplicationService consumeItemsFromContainerApplicationService, IGetAllContainersApplicationService getAllContainersApplicationService, IMoveItemsBetweenContainersApplicationService moveItemsBetweenContainersApplicationService)
        {
            _containerDetailApplicationService = containerDetailApplicationService;
            _consumeItemsFromContainerApplicationService = consumeItemsFromContainerApplicationService;
            _getAllContainersApplicationService = getAllContainersApplicationService;
            _moveItemsBetweenContainersApplicationService = moveItemsBetweenContainersApplicationService;
        }
        
        [FromRoute(nameof(ContainerId))]
        public Guid ContainerId { get; set; }
        [Bind(Direction.ServerToClient)]
        public ContainerDetailModel ContainerDetailModel { get; set; }

        public ConsumeItemsDialogModel? ConsumeItemsDialogModel { get; set; }
        public MoveItemsDialogModel? MoveItemsDialogModel { get; set; }
        
        public override async Task Load()
        {
            await base.Load();
            await ReloadDataAsync();
        }

        private async Task ReloadDataAsync()
        {
            ContainerDetailModel = await _containerDetailApplicationService.GetContainerDetailAsync(ContainerId.ToString(), RequestCancellationToken);
        }

        public void ShowConsumeItemsDialog()
        {
            ConsumeItemsDialogModel = new ConsumeItemsDialogModel(ContainerId,
                                                                  GetAvailableWarehouseItems());
        }

        public async Task ShowMoveItemsDialogAsync()
        {
            var containers = await _getAllContainersApplicationService.GetAllAsync(RequestCancellationToken);

            MoveItemsDialogModel = new MoveItemsDialogModel(ContainerId,
                                                            GetAvailableWarehouseItems(),
                                                            containers.Where(c => c.ContainerId != ContainerId)
                                                                      .ToList());
        }

        private List<DialogWarehouseItemModel> GetAvailableWarehouseItems()
        {
            return ContainerDetailModel
                   .ContainerContents
                   .Select(
                       c => new DialogWarehouseItemModel(
                           c.WarehouseItemId,
                           c.WarehouseItemName))
                   .ToList();
        }
        
        [BusinessRuleExceptionFilter(nameof(ConsumeItemsDialogModel))]
        public async Task ConsumeItemsAsync(ConsumeItemsDialogModel dialogModel)
        {
            await _consumeItemsFromContainerApplicationService.ConsumeItemsAsync(dialogModel.ContainerId, dialogModel.SelectedWarehouseItemId, dialogModel.Amount, RequestCancellationToken);
            await ReloadDataAsync();
        }

        [BusinessRuleExceptionFilter(nameof(MoveItemsDialogModel))]
        public async Task MoveItemsAsync(MoveItemsDialogModel dialogModel)
        {
            await _moveItemsBetweenContainersApplicationService.MoveItemBetweenContainersAsync(dialogModel.SourceContainerId.ToString(),
                dialogModel.DestinationContainerId.ToString(),
                dialogModel.SelectedWarehouseItemId.ToString(),
                dialogModel.Amount,
                RequestCancellationToken);
            await ReloadDataAsync();
        }
    }
}