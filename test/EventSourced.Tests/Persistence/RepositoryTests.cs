using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Domain;
using EventSourced.Domain.Events;
using EventSourced.EventBus;
using EventSourced.Exceptions;
using EventSourced.Persistence;
using EventSourced.Snapshots;
using EventSourced.Tests.TestDoubles.Extensions;
using FluentAssertions;
using Moq;
using Xunit;

[assembly: InternalsVisibleTo("DynamicProxyGenAssembly2")]

namespace EventSourced.Tests.Persistence
{
    public class RepositoryTests
    {
        private readonly Mock<IEventStore> _eventStoreMock = new();
        private readonly Mock<ISnapshotStore<TestAggregate>> _snapshotStoreMock = new();
        private readonly Mock<ISnapshotCreationStrategy> _snapshotCreationStrategyMock = new();
        private readonly Mock<IDomainEventBus> _domainEventBus = new();

        #region SaveAsync()

        [Fact]
        public async Task SaveAsync_WithExistingChanges_DequeuesAllEventsFromAggregate()
        {
            //Arrange
            var testAggregate = new TestAggregate();
            testAggregate.EnqueueTestEvent();
            var repository = CreateSut();

            //Act
            await repository.SaveAsync(testAggregate, CancellationToken.None);

            //Assert
            var domainEventsInQueue = testAggregate.DequeueDomainEvents();
            domainEventsInQueue.Should()
                               .BeEmpty();
        }

        [Fact]
        public async Task SaveAsync_WithExistingChanges_StoresThemInEventStore()
        {
            //Arrange
            var testAggregate = new TestAggregate();
            testAggregate.EnqueueTestEvent();
            var repository = CreateSut();

            //Act
            await repository.SaveAsync(testAggregate, CancellationToken.None);

            //Assert
            VerifyEventStoreSaveMethodCalled();
        }

        [Fact]
        public async Task SaveAsync_WithDomainEventHandlers_HandlersAreCalled()
        {
            //Arrange
            var mockDomainEventListener = new Mock<IEventStreamUpdatedEventHandler>();
            var testAggregate = new TestAggregate();
            testAggregate.EnqueueTestEvent();
            var repository = CreateSut(mockDomainEventListener.Object);

            //Act
            await repository.SaveAsync(testAggregate, CancellationToken.None);

            //Assert
            mockDomainEventListener.Verify(
                s => s.HandleDomainEventAsync(It.IsAny<Type>(),
                                              It.IsAny<string>(),
                                              It.IsAny<DomainEvent>(),
                                              It.IsAny<CancellationToken>()),
                Times.Once);
        }

        [Fact]
        public async Task SaveAsync_WithVersionConflict_ThrowsException()
        {
            //Arrange
            var updatedAggregate = new TestAggregate();
            updatedAggregate.SetVersion(1);
            updatedAggregate.EnqueueTestEvent();

            _eventStoreMock.WithStreamExistsAsync(true)
                           .WithGetByStreamIdAsync(updatedAggregate.Id, new[] {new TestEvent {Version = 2}});
            var repository = CreateSut();

            //Act
            Func<Task> action = () => repository.SaveAsync(updatedAggregate, CancellationToken.None);

            //Assert
            await action.Should()
                        .ThrowAsync<AggregateVersionConflictException>();
        }

        [Fact]
        public async Task SaveAsync_WithExistingValueWithoutVersionConflict_ThrowsException()
        {
            //Arrange
            var updatedAggregate = new TestAggregate();
            updatedAggregate.SetVersion(1);
            updatedAggregate.EnqueueTestEvent();

            _eventStoreMock.WithStreamExistsAsync(true)
                           .WithGetByStreamIdAsync(updatedAggregate.Id, new[] {new TestEvent {Version = 1}});
            var repository = CreateSut();

            //Act
            await repository.SaveAsync(updatedAggregate, CancellationToken.None);

            //Assert
            VerifyEventStoreSaveMethodCalled();
        }

        [Fact]
        public async Task SaveAsync_WithSnapshotStrategyTriggered_StoresTheSnapshot()
        {
            //Arrange
            var updatedAggregate = new TestAggregate();
            updatedAggregate.EnqueueTestEvent();

            _snapshotCreationStrategyMock.WithShouldCreateSnapshot<TestAggregate>(true);
            var repository = CreateSut();

            //Act
            await repository.SaveAsync(updatedAggregate, CancellationToken.None);

            //Assert
            VerifySnapshotCreated();
        }

        [Fact]
        public async Task SaveAsync_WithNewEvents_EventsArePublishedToEventBus()
        {
            //Arrange
            var updatedAggregate = new TestAggregate();
            updatedAggregate.EnqueueTestEvent();

            _snapshotCreationStrategyMock.WithShouldCreateSnapshot<TestAggregate>(true);
            var repository = CreateSut();

            //Act
            await repository.SaveAsync(updatedAggregate, CancellationToken.None);

            //Assert
            VerifyDomainEventsPublished();
        }
        
        #endregion

        #region GetByIdAsync()

        [Fact]
        public async Task GetByIdAsync_WithExistingAggregate_RebuildsItFromEvents()
        {
            //Arrange
            var aggregateId = Guid.NewGuid().ToString();
            var existingEvents = new[]
            {
                new TestEvent(),
                new TestEvent(),
                new TestEvent(),
                new TestEvent()
            };
            _eventStoreMock.WithGetByStreamIdAsync(aggregateId, existingEvents);
            var repository = CreateSut();

            //Act
            var aggregate = await repository.GetByIdAsync(aggregateId, CancellationToken.None);

            //Assert
            aggregate.EventsCount.Should()
                     .Be(4);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingAggregate_VersionIsSetCorrectly()
        {
            //Arrange
            var aggregateId = Guid.NewGuid().ToString();
            var existingEvents = new[]
            {
                new TestEvent(1),
                new TestEvent(2),
                new TestEvent(3),
                new TestEvent(4),
                new TestEvent(5)
            };
            _eventStoreMock.WithGetByStreamIdAsync(aggregateId, existingEvents);
            var repository = CreateSut();

            //Act
            var aggregate = await repository.GetByIdAsync(aggregateId, CancellationToken.None);

            //Assert
            aggregate.Version.Should()
                     .Be(5);
        }

        [Fact]
        public async Task GetByIdAsync_WithExistingSnapshot_UsesTheSnapshotVersion()
        {
            //Arrange
            var aggregateId = Guid.NewGuid().ToString();
            var existingEvents = Array.Empty<DomainEvent>();
            _eventStoreMock.WithGetByStreamIdAsync(aggregateId, existingEvents);
            var aggregateRootFromSnapshot = new TestAggregate(aggregateId);
            aggregateRootFromSnapshot.SetVersion(5);
            _snapshotStoreMock.WithLoadSnapshotAsync(aggregateRootFromSnapshot);
            var repository = CreateSut();

            //Act
            var aggregate = await repository.GetByIdAsync(aggregateId, CancellationToken.None);

            //Assert
            aggregate.Version.Should()
                     .Be(5);
        }

        #endregion

        #region GetAllAsync()

        [Fact]
        public async Task GetAllAsync_WithExistingAggregates_RebuildsAllFromEvents()
        {
            //Arrange
            var aggregateId = Guid.NewGuid().ToString();
            var aggregateId2 = Guid.NewGuid().ToString();
            var existingEvents = new[]
            {
                new TestEvent(),
                new TestEvent(),
                new TestEvent(),
                new TestEvent()
            };
            _eventStoreMock.WithGetAllStreamsOfType(new Dictionary<string, DomainEvent[]>
            {
                {aggregateId, existingEvents.ToArray()},
                {aggregateId2, existingEvents.ToArray()}
            });
            var repository = CreateSut();

            //Act
            var aggregates = await repository.GetAllAsync(CancellationToken.None);

            //Assert
            aggregates.Should()
                      .HaveCount(2);

            aggregates.Should()
                      .OnlyContain(x => x.EventsCount == 4);
        }

        #endregion

        #region ExistsAsync()

        [Fact]
        public async Task ExistsAsync_WithExistingStream_ReturnsTrue()
        {
            //Arrange
            var aggregateId = Guid.NewGuid().ToString();
            _eventStoreMock.WithStreamExistsAsync(true);
            var repository = CreateSut();

            //Act
            var result = await repository.ExistsAsync(aggregateId, CancellationToken.None);

            //Assert
            result.Should()
                  .BeTrue();
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingStream_ReturnsFalse()
        {
            //Arrange
            var aggregateId = Guid.NewGuid().ToString();
            _eventStoreMock.WithStreamExistsAsync(false);
            var repository = CreateSut();

            //Act
            var result = await repository.ExistsAsync(aggregateId, CancellationToken.None);

            //Assert
            result.Should()
                  .BeFalse();
        }
        
        #endregion
        
        private void VerifyEventStoreSaveMethodCalled()
        {
            _eventStoreMock.Verify(s => s.StoreEventsAsync(It.IsAny<string>(),
                                                           It.IsAny<Type>(),
                                                           It.IsAny<IList<DomainEvent>>(),
                                                           It.IsAny<CancellationToken>()),
                                   Times.Once);
        }

        private void VerifySnapshotCreated()
        {
            _snapshotStoreMock.Verify(s => s.StoreSnapshotAsync(It.IsAny<TestAggregate>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        private void VerifyDomainEventsPublished()
        {
            _domainEventBus.Verify(s =>s.PublishDomainEventsAsync(It.IsAny<IEnumerable<DomainEvent>>(), It.IsAny<CancellationToken>()), Times.Once);
        }

        private IRepository<TestAggregate> CreateSut(params IEventStreamUpdatedEventHandler[] domainEventHandlers)
        {
            return new Repository<TestAggregate>(_eventStoreMock.Object,
                                                 domainEventHandlers.ToList(),
                                                 _snapshotStoreMock.Object,
                                                 _snapshotCreationStrategyMock.Object,
                                                 _domainEventBus.Object);
        }

        internal class TestEvent : DomainEvent
        {
            private static int EventsCount = 0;

            public TestEvent()
            {
                Version = EventsCount++;
            }

            public TestEvent(int version)
            {
                Version = version;
                EventsCount++;
            }
        }

        internal class TestAggregate : AggregateRoot
        {
            public int EventsCount { get; private set; }

            public TestAggregate()
                : this(Guid.NewGuid().ToString())
            {
            }

            public TestAggregate(string id)
                : base(id)
            {
            }

            public void EnqueueTestEvent()
            {
                EnqueueAndApplyEvent(new TestEvent());
            }

            public void SetVersion(int version)
            {
                Version = version;
            }

            public void Apply(TestEvent testEvent)
            {
                EventsCount++;
            }
        }
    }
}