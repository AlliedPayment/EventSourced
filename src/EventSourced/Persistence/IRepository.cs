using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Domain;

namespace EventSourced.Persistence
{
    public interface IRepository<TAggregateRoot> where TAggregateRoot : AggregateRoot
    {
        Task SaveAsync(TAggregateRoot aggregateRoot, CancellationToken ct);
        Task<TAggregateRoot> GetByIdAsync(string id, CancellationToken ct);
        Task<ICollection<TAggregateRoot>> GetAllAsync(CancellationToken ct);
        Task<bool> ExistsAsync(string id, CancellationToken ct);
    }
}