using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Domain;
using EventSourced.Domain.Events;
using EventSourced.Persistence;
using EventSourced.Persistence.EntityFramework;
using EventSourced.Persistence.EntityFramework.Helpers;
using EventSourced.Persistence.EntityFramework.Mappers;
using FluentAssertions;
using Xunit;

namespace EventSourced.Tests.Persistence.EntityFramework
{
    public class EntityFrameworkEventStoreTests
    {
        [Fact]
        public async Task GetByStreamIdAsync_WithExistingValues_LoadsThemCorrectly()
        {
            //Arrange
            var streamId = Guid.NewGuid().ToString();
            var testEvents = new List<DomainEvent>
            {
                new TestEvent(1),
                new TestEvent(2),
                new TestEvent(3)
            };
            var sut = CreateSut();

            //Act
            await sut.StoreEventsAsync(streamId, typeof(TestAggregate), testEvents, CancellationToken.None);
            var loadedStream = await sut.GetByStreamIdAsync(streamId, typeof(TestAggregate), 0, CancellationToken.None);

            //Assert
            loadedStream.Should()
                        .BeInAscendingOrder(e => e.Version);

            loadedStream.Should()
                        .HaveCount(3);
        }

        [Fact]
        public async Task GetByStreamIdAsync_WithExistingValuesAndVersionInformation_LoadsThemCorrectly()
        {
            //Arrange
            var streamId = Guid.NewGuid().ToString();
            var testEvents = new List<DomainEvent>
            {
                new TestEvent(1),
                new TestEvent(2),
                new TestEvent(3)
            };
            var sut = CreateSut();

            //Act
            await sut.StoreEventsAsync(streamId, typeof(TestAggregate), testEvents, CancellationToken.None);
            var loadedStream = await sut.GetByStreamIdAsync(streamId, typeof(TestAggregate), 1, CancellationToken.None);

            //Assert
            loadedStream.Should()
                        .BeInAscendingOrder(e => e.Version);

            loadedStream.Should()
                        .HaveCount(2);
        }

        [Fact]
        public async Task GetAllStreamsOfTypeAsync_WithExistingValues_LoadsThemCorrectly()
        {
            //Arrange
            var streamId = Guid.NewGuid().ToString();
            var testEvents = new List<DomainEvent>
            {
                new TestEvent(1),
                new TestEvent(2),
                new TestEvent(3)
            };
            var sut = CreateSut();

            //Act
            await sut.StoreEventsAsync(streamId, typeof(TestAggregate), testEvents, CancellationToken.None);
            var allStreamsOfType = await sut.GetAllStreamsOfType(typeof(TestAggregate), CancellationToken.None);

            //Assert
            allStreamsOfType.Should()
                            .HaveCount(1);

            allStreamsOfType.Should()
                            .ContainKey(streamId)
                            .WhichValue.Should()
                            .HaveCount(3)
                            .And.BeInAscendingOrder(e => e.Version);
        }

        [Fact]
        public async Task GetEventsOfTypeAsync_WithExistingValues_LoadsThemCorrectly()
        {
            //Arrange
            var streamId = Guid.NewGuid().ToString();
            var testEvents = new List<DomainEvent>
            {
                new TestEvent(1),
                new TestEvent(2),
                new TestEvent(3)
            };
            var sut = CreateSut();

            //Act
            await sut.StoreEventsAsync(streamId, typeof(TestAggregate), testEvents, CancellationToken.None);
            var eventsOfType = await sut.GetEventsOfTypeAsync(typeof(TestEvent), CancellationToken.None);

            //Assert
            eventsOfType.Should()
                        .HaveCount(3)
                        .And.BeInAscendingOrder(e => e.Version);
        }
        
        [Fact]
        public async Task StreamExistsAsync_WithExistingValues_ReturnsTrue()
        {
            //Arrange
            var streamId = Guid.NewGuid().ToString();
            var testEvents = new List<DomainEvent>
            {
                new TestEvent(1),
                new TestEvent(2),
                new TestEvent(3)
            };
            var sut = CreateSut();

            //Act
            await sut.StoreEventsAsync(streamId, typeof(TestAggregate), testEvents, CancellationToken.None);
            var streamExists = await sut.StreamExistsAsync(streamId, typeof(TestAggregate), CancellationToken.None);

            //Assert
            streamExists.Should()
                        .BeTrue();
        }

        [Fact]
        public async Task StreamExistsAsync_WithNonExistingValues_ReturnsFalse()
        {
            //Arrange
            var streamId = Guid.NewGuid().ToString();
            var sut = CreateSut();

            //Act
            var streamExists = await sut.StreamExistsAsync(streamId, typeof(TestAggregate), CancellationToken.None);

            //Assert
            streamExists.Should()
                        .BeFalse();
        }
        
        private IEventStore CreateSut()
        {
            var typeSerializer = new TypeSerializer();
            return new EntityFrameworkEventStore(new DomainEventEntityMapper(typeSerializer, new EventSerializer()),
                                                 TestDbContextFactory.Create(),
                                                 typeSerializer);
        }

        private class TestEvent : DomainEvent
        {
            public int Value { get; }

            public TestEvent(int value)
            {
                Value = value;
                Version = value;
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