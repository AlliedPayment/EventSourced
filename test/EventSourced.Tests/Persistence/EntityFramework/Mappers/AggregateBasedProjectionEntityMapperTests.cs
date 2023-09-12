using System;
using EventSourced.Domain;
using EventSourced.Persistence.EntityFramework.Helpers;
using EventSourced.Persistence.EntityFramework.Mappers;
using EventSourced.Projections;
using FluentAssertions;
using Xunit;

namespace EventSourced.Tests.Persistence.EntityFramework.Mappers
{
    public class AggregateBasedProjectionEntityMapperTests
    {
        [Fact]
        public void MapToEntity_WhenUnmapped_KeepsEverythingSet()
        {
            //Arrange
            var aggregateId = Guid.NewGuid().ToString();
            var projection = new TestProjection(aggregateId);
            projection.SetNumber(42);
            var sut = CreateSut();
            
            //Act
            var projectionEntity = sut.MapToEntity(aggregateId, projection);
            var unmappedProjection = (TestProjection) sut.MapToProjection(projectionEntity);
            
            //Assert
            unmappedProjection.NumberWithPrivateSet.Should()
                              .Be(42);
        }

        private IAggregateBasedProjectionEntityMapper CreateSut()
        {
            return new AggregateBasedProjectionEntityMapper(new TypeSerializer());
        }

        private class TestProjection : AggregateProjection<TestAggregateRoot>
        {
            public int NumberWithPrivateSet { get; private set; }

            public TestProjection(string id)
                : base(id)
            {
            }

            public void SetNumber(int number)
            {
                NumberWithPrivateSet = number;
            }
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