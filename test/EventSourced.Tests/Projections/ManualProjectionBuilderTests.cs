using System;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Domain;
using EventSourced.Domain.Events;
using EventSourced.Persistence;
using EventSourced.Projections;
using EventSourced.Tests.TestDoubles.Extensions;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventSourced.Tests.Projections
{
    public class ManualProjectionBuilderTests
    {
        private readonly Mock<IEventStore> _eventStoreMock = new();

        [Fact]
        public async Task BuildProjectionAsync_WithExistingApplicableEvents_BuildsTheProjection()
        {
            //Arrange
            DomainEvent[] existingEvents =
            {
                new TestEvent(),
                new TestEvent(),
                new TestEvent()
            };
            _eventStoreMock.WithGetEventsOfTypeAsync(existingEvents);
            var sut = CreateSut();

            //Act
            var projection = await sut.BuildProjectionAsync<EventCountProjection>(CancellationToken.None);

            //Assert
            projection.AppliedEventsCount.Should()
                      .Be(3);
        }

        [Fact]
        public async Task BuildAggregateProjectionAsync_WithExistingApplicableEvents_BuildsTheProjection()
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
            var sut = CreateSut();

            //Act
            var projection =
                await sut.BuildAggregateProjection<EventCountAggregateProjection, TestAggregateRoot>(
                    aggregateId,
                    CancellationToken.None);

            //Assert
            projection.Id.Should()
                      .Be(aggregateId);

            projection.AppliedEventsCount.Should()
                      .Be(4);
        }

        private IManualProjectionBuilder CreateSut()
        {
            return new ManualProjectionBuilder(_eventStoreMock.Object);
        }

        private class TestAggregateRoot : AggregateRoot
        {
            public TestAggregateRoot(string id)
                : base(id)
            {
            }
        }

        private class TestEvent : DomainEvent
        {
        }

        private class EventCountProjection
        {
            public int AppliedEventsCount { get; private set; }

            private void Apply(TestEvent @event)
            {
                AppliedEventsCount++;
            }
        }

        private class EventCountAggregateProjection : AggregateProjection<TestAggregateRoot>
        {
            public int AppliedEventsCount { get; private set; }

            public EventCountAggregateProjection(string id)
                : base(id)
            {
            }

            private void Apply(TestEvent @event)
            {
                AppliedEventsCount++;
            }
        }
    }
}