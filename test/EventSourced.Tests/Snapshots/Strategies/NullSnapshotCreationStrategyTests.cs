using System;
using EventSourced.Domain;
using EventSourced.Snapshots;
using EventSourced.Snapshots.Strategies;
using FluentAssertions;
using Xunit;

namespace EventSourced.Tests.Snapshots.Strategies
{
    public class NullSnapshotCreationStrategyTests
    {
        [Fact]
        public void ShouldCreateSnapshot_AlwaysReturnsFalse()
        {
            //Arrange
            var aggregateRoot = new TestAggregateRoot(Guid.NewGuid().ToString());
            var sut = CreateSut();
            
            //Act
            var shouldCreateSnapshot = sut.ShouldCreateSnapshot(aggregateRoot);

            //Assert
            shouldCreateSnapshot.Should()
                                .BeFalse();
        }

        private ISnapshotCreationStrategy CreateSut()
        {
            return new NullSnapshotCreationStrategy();
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