using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using EventSourced.Domain.Events;
using EventSourced.Persistence;
using EventSourced.Projections.Automatic;
using FluentAssertions;
using Moq;
using Xunit;

namespace EventSourced.Tests.Projections.Automatic
{
    public class AutomaticProjectionDomainEventHandlerTests
    {
        private static readonly Type AnyAggregateType = typeof(object);

        private static readonly string AnyAggregateId = Guid.NewGuid().ToString();

        private readonly Mock<IAutomaticProjectionsEventMapper> _automaticProjectionEventMapperMock = new();

        private readonly Mock<IProjectionStore> _projectionStoreMock = new();

        [Fact]
        public async Task HandleDomainEventAsync_WithNonExistingProjection_CreatesIt()
        {
            //Arrange
            var domainEvent = new TestEvent(42);
            _automaticProjectionEventMapperMock.Setup(s => s.GetProjectionsAffectedByEvent(It.IsAny<DomainEvent>()))
                                               .Returns(new[] {typeof(TestProjection)});
            var sut = CreateSut();

            //Act
            await sut.HandleDomainEventAsync(AnyAggregateType, AnyAggregateId, domainEvent, CancellationToken.None);

            //Assert
            var storedProjection = GetProjectionStoredInProjectionStore();
            storedProjection.Number.Should()
                            .Be(42);
        }

        [Fact]
        public async Task HandleDomainEventAsync_WithExistingProjection_UpdatesIt()
        {
            //Arrange
            var domainEvent = new TestEvent(42);
            _automaticProjectionEventMapperMock.Setup(s => s.GetProjectionsAffectedByEvent(It.IsAny<DomainEvent>()))
                                               .Returns(new[] {typeof(TestProjection)});
            var originalProjection = new TestProjection
            {
                Number = 1
            };
            SetupProjectionInProjectionStore(originalProjection);
            var sut = CreateSut();

            //Act
            await sut.HandleDomainEventAsync(AnyAggregateType, AnyAggregateId, domainEvent, CancellationToken.None);

            //Assert
            var updatedProjection = GetProjectionStoredInProjectionStore();
            updatedProjection.Number.Should()
                             .Be(42);
        }

        private AutomaticProjectionEventStreamUpdatedEventHandler CreateSut()
        {
            return new(_automaticProjectionEventMapperMock.Object, _projectionStoreMock.Object);
        }

        private void SetupProjectionInProjectionStore(TestProjection projection)
        {
            _projectionStoreMock.Setup(s => s.LoadProjectionAsync(typeof(TestProjection), CancellationToken.None))
                                .ReturnsAsync(projection);
        }

        private TestProjection GetProjectionStoredInProjectionStore()
        {
            return _projectionStoreMock.Invocations.Single(i => i.Method.Name == nameof(IProjectionStore.StoreProjectionAsync))
                                       .Arguments.OfType<TestProjection>()
                                       .Single();
        }

        private class TestEvent : DomainEvent
        {
            public int Number { get; }

            public TestEvent(int number)
            {
                Number = number;
            }
        }

        private class TestProjection
        {
            public int Number { get; set; }

            private void Apply(TestEvent @event)
            {
                Number = @event.Number;
            }
        }
    }
}