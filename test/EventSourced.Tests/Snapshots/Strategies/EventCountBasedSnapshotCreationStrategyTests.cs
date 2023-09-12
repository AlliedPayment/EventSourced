using System;
using EventSourced.Domain;
using EventSourced.Snapshots;
using EventSourced.Snapshots.Strategies;
using FluentAssertions;
using Xunit;

namespace EventSourced.Tests.Snapshots.Strategies
{
    public class EventCountBasedSnapshotCreationStrategyTests
    {
        [Fact]
        public void ShouldCreateSnapshot_WhenAggregateVersionMatches_ReturnsTrue()
        {
            //Arrange
            var aggregateRoot = new TestAggregateRoot(Guid.NewGuid().ToString());
            aggregateRoot.SetVersion(5);
            var sut = CreateSut(5);
            
            //Act
            var shouldCreateSnapshot = sut.ShouldCreateSnapshot(aggregateRoot);

            //Assert
            shouldCreateSnapshot.Should()
                                .BeTrue();
        }
        
        [Fact]
        public void ShouldCreateSnapshot_WhenAggregateVersionDoesNotMatch_ReturnsFalse()
        {
            //Arrange
            var aggregateRoot = new TestAggregateRoot(Guid.NewGuid().ToString());
            aggregateRoot.SetVersion(4);
            var sut = CreateSut(5);
            
            //Act
            var shouldCreateSnapshot = sut.ShouldCreateSnapshot(aggregateRoot);

            //Assert
            shouldCreateSnapshot.Should()
                                .BeFalse();
        }

        private ISnapshotCreationStrategy CreateSut(int eventCountBetweenSnapshots)
        {
            return new EventCountBasedSnapshotCreationStrategy(eventCountBetweenSnapshots);
        }
        
        private class TestAggregateRoot : AggregateRoot
        {
            public TestAggregateRoot(string id)
                : base(id)
            {
            }

            public void SetVersion(int version)
            {
                Version = version;
            }
        }
    }
}