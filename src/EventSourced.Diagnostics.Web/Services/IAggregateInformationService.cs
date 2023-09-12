 using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
 using EventSourced.Diagnostics.Web.Model.Aggregates;

 namespace EventSourced.Diagnostics.Web.Services
{
    public interface IAggregateInformationService
    {
        Task<ICollection<AggregateTypesListItemModel>> GetStoredAggregateTypesAsync(CancellationToken ct);
        Task<ICollection<AggregateInstancesListItemModel>> GetStoredAggregatesOfTypeAsync(Type aggregateType, CancellationToken ct);
        Task<AggregateInstancesListItemModel> GetStoredAggregateByIdAndVersionAsync(
            string aggregateId,
            Type aggregateType,
            int version,
            CancellationToken ct);
    }
}