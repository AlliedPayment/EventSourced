using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Domain;
using EventSourced.Persistence;
using EventSourced.Persistence.Null;
using FluentAssertions;
using Xunit;

namespace EventSourced.Tests.Persistence.Null
{
    public class NullSnapshotStoreTests
    {
        [Fact]
        public async Task LoadSnapshotAsync_WithExistingAggregate_CanNotBeLoaded()
        {
            //Arrange
            var aggregateRoot = new TestAggregateRoot(Guid.NewGuid().ToString());
            var sut = CreateSut();

            //Act
            await sut.StoreSnapshotAsync(aggregateRoot, CancellationToken.None);
            var loadedSnapshot = await sut.LoadSnapshotAsync(aggregateRoot.Id, CancellationToken.None);

            //Assert
            loadedSnapshot.Should()
                          .BeNull();
        }

        private ISnapshotStore<TestAggregateRoot> CreateSut()
        {
            return new NullSnapshotStore<TestAggregateRoot>();
        }

        private class TestAggregateRoot : AggregateRoot
        {
            public TestAggregateRoot(string id)
                : base(id)
            {
            }
        }
    }
}