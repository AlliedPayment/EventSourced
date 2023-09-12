using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using DotVVM.Framework.ViewModel;
using EventSourced.Diagnostics.Web.Configuration;
using EventSourced.Diagnostics.Web.Helpers;
using EventSourced.Diagnostics.Web.Model.Projections;
using EventSourced.Diagnostics.Web.Services;

namespace EventSourced.Diagnostics.Web.Pages.AggregateProjectionTypeDetail
{
    public class AggregateProjectionTypeDetailViewModel : ViewModelBase
    {
        private readonly IProjectionInformationService _projectionInformationService;

        public AggregateProjectionTypeDetailViewModel(IProjectionInformationService projectionInformationService,
                                                      EventSourcedDiagnosticsOptions options)
            : base(options)
        {
            _projectionInformationService = projectionInformationService;
        }

        [Bind(Direction.None)]
        public Type AggregateProjectionType => TypeEncoder.DecodeType(EncodedAggregateProjectionType);
        [FromRoute("ProjectionType")]
        public string EncodedAggregateProjectionType { get; set; } = null!;
        public string ProjectionDisplayName => AggregateProjectionType.Name;
        public string? ProjectionFullName => AggregateProjectionType.FullName;
        [Bind(Direction.ServerToClient)]
        public ICollection<AggregateProjectionValueModel> ProjectionValues { get; set; } = new List<AggregateProjectionValueModel>();
        [Bind(Direction.ServerToClient)]
        public AggregateProjectionValueModel? SelectedValue { get; set; }
        public string? SelectedAggregateId { get; set; }

        public override async Task Load()
        {
            await base.Load();
            ProjectionValues =
                await _projectionInformationService.GetAggregateProjectionsOfTypeAsync(
                    AggregateProjectionType,
                    RequestCancellationToken);
        }

        public void OnSelectedAggregateChanged()
        {
            SelectedValue = ProjectionValues.SingleOrDefault(p => p.AggregateId == SelectedAggregateId);
        }
    }
}