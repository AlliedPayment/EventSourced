using System;
using EventSourced.Domain;
using EventSourced.Domain.Snapshosts;
using EventSourced.Persistence.EntityFramework.Helpers;
using EventSourced.Persistence.EntityFramework.Mappers;
using FluentAssertions;
using Xunit;

namespace EventSourced.Tests.Persistence.EntityFramework.Mappers
{
    public class AggregateSnapshotEntityMapperTests
    {
        [Fact]
        public void MapToEntity_WithValidSnapshot_CanBeDeserialized()
        {
            //Arrange
            var originalAggregateState = new TestAggregateRoot(42);
            var aggregateSnapshot =
                new AggregateSnapshot<TestAggregateRoot>(originalAggregateState);
            var sut = CreateSut();
            
            //Act
            var aggregateSnapshotEntity = sut.MapToEntity(aggregateSnapshot);
            var aggregateRootFromSnapshot = sut.MapToAggregateRoot<TestAggregateRoot>(aggregateSnapshotEntity);

            //Assert
            aggregateRootFromSnapshot.SomeAggregateValue.Should()
                                     .Be(originalAggregateState.SomeAggregateValue);
            aggregateRootFromSnapshot.Id.Should()
                                     .Be(originalAggregateState.Id);
            aggregateRootFromSnapshot.Version.Should()
                                     .Be(originalAggregateState.Version);
        }

        private IAggregateSnapshotEntityMapper CreateSut()
        {
            return new AggregateSnapshotEntityMapper(new TypeSerializer());
        }

        private class TestAggregateRoot : AggregateRoot
        {
            public int SomeAggregateValue { get; private set; }

            public TestAggregateRoot(int someAggregateValue) 
                : this(Guid.NewGuid().ToString())
            {
                SomeAggregateValue = someAggregateValue;
                Version = someAggregateValue;
            }
            
            public TestAggregateRoot(string id)
                : base(id)
            {
            }
        }
    }
}