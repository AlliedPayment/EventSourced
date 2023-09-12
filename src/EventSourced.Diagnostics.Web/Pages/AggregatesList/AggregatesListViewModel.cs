using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using EventSourced.Diagnostics.Web.Configuration;
using EventSourced.Diagnostics.Web.Helpers;
using EventSourced.Diagnostics.Web.Model.Aggregates;
using EventSourced.Diagnostics.Web.Services;

namespace EventSourced.Diagnostics.Web.Pages.AggregatesList
{
    public class AggregatesListViewModel : ViewModelBase
    {
        private readonly IAggregateInformationService _aggregateInformationService;

        public AggregatesListViewModel(IAggregateInformationService aggregateInformationService,
                                       EventSourcedDiagnosticsOptions options)
            : base(options)
        {
            _aggregateInformationService = aggregateInformationService;
        }

        [Bind(Direction.None)]
        public Type AggregateRootType => TypeEncoder.DecodeType(EncodedAggregateRootType);
        [FromRoute("AggregateType")]
        public string EncodedAggregateRootType { get; set; } = null!;
        public string? AggregateDisplayName => AggregateRootType.Name;
        public string? AggregateFullName => AggregateRootType.FullName;
        [Bind(Direction.ServerToClientFirstRequest)]
        public ICollection<AggregateInstancesListItemModel> AggregateInstances { get; set; } =
            new List<AggregateInstancesListItemModel>();
        [Bind(Direction.ServerToClient)]
        public AggregateInstancesListItemModel? SelectedAggregateInstance { get; set; }
        public string? SelectedAggregateInstanceId { get; set; }
        public int? SelectedVersion { get; set; }
        public int? SelectedMaxVersion { get; set; }

        public override async Task Load()
        {
            await base.Load();
            if (!Context.IsPostBack)
            {
                AggregateInstances =
                    await _aggregateInformationService.GetStoredAggregatesOfTypeAsync(AggregateRootType, RequestCancellationToken);
                SelectedAggregateInstance = AggregateInstances.FirstOrDefault();
                SelectedAggregateInstanceId = SelectedAggregateInstance?.Id;
                SelectedVersion = SelectedAggregateInstance?.Version;
                SelectedMaxVersion = SelectedVersion;
            }
        }

        public async Task OnAggregateInstanceChanged()
        {
            AggregateInstances =
                await _aggregateInformationService.GetStoredAggregatesOfTypeAsync(AggregateRootType, RequestCancellationToken);
            SelectedAggregateInstance = AggregateInstances.SingleOrDefault(i => i.Id == SelectedAggregateInstanceId);
            SelectedVersion = SelectedAggregateInstance?.Version;
            SelectedMaxVersion = SelectedVersion;
        }

        public async Task ChangeVersion(int version)
        {
            if (string.IsNullOrEmpty(SelectedAggregateInstanceId)) return;
            SelectedAggregateInstance =
                await _aggregateInformationService.GetStoredAggregateByIdAndVersionAsync(
                    SelectedAggregateInstanceId,
                    AggregateRootType,
                    version,
                    RequestCancellationToken);
            SelectedVersion = SelectedAggregateInstance.Version;
        }
    }
}