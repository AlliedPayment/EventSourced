using System;
using EventSourced.Domain;
using EventSourced.Domain.Events;
using EventSourced.Persistence.EntityFramework.Entities;
using EventSourced.Persistence.EntityFramework.Helpers;
using EventSourced.Persistence.EntityFramework.Mappers;
using FluentAssertions;
using Xunit;

namespace EventSourced.Tests.Persistence.EntityFramework.Mappers
{
    public class DomainEventEntityMapperTests
    {
        [Fact]
        public void MapToEntity_WhenMappedBack_MapsCorrectly()
        {
            //Arrange
            var aggregateId = Guid.NewGuid().ToString();
            var testEvent = new TestEvent(42);
            var sut = CreateSut();
            
            //Act
            var entity = sut.MapToEntity(testEvent, aggregateId, typeof(TestAggregate));
            var mappedEvent = (TestEvent) sut.MapToDomainEvent(entity);
            
            //Assert
            mappedEvent.Version.Should()
                       .Be(testEvent.Version);
            
            mappedEvent.Number.Should()
                       .Be(testEvent.Number);
        }

        private IDomainEventEntityMapper CreateSut()
        {
            return new DomainEventEntityMapper(new TypeSerializer(), new EventSerializer());
        }

        private class TestEvent : DomainEvent
        {
            public int Number { get; }

            public TestEvent(int number)
            {
                Number = number;
            }
        }

        private class TestAggregate : AggregateRoot
        {
            public TestAggregate(string id)
                : base(id)
            {
            }
        }
    }
}