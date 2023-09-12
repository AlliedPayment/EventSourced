using System;
using System.Threading;
using EventSourced.Domain;
using EventSourced.Persistence;
using Moq;

namespace EventSourced.Tests.TestDoubles.Extensions
{
    public static class SnapshotStoreMockExtensions
    {
        public static Mock<ISnapshotStore<TAggregateRoot>> WithLoadSnapshotAsync<TAggregateRoot>(
            this Mock<ISnapshotStore<TAggregateRoot>> mock,
            TAggregateRoot aggregateRoot) 
            where TAggregateRoot : AggregateRoot
        {
            mock.Setup(s => s.LoadSnapshotAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(aggregateRoot);
            return mock;
        }
    }
}