using System;
using System.Linq;
using EventSourced.Domain;
using EventSourced.Domain.Events;
using FluentAssertions;
using Xunit;

namespace EventSourced.Tests.Domain
{
    public class AggregateRootTests
    {
        [Fact]
        public void EnqueuedDomainEvent_IsAddedToQueue_And_CanBeDequeued()
        {
            //Arrange
            var sut = CreateTestAggregateRootWithApplyForTestEvent();
            const string testParameterValue = "Test value";

            //Act
            sut.EnqueueTestDomainEvent(testParameterValue);

            //Assert
            var uncommittedDomainEvents = sut.DequeueDomainEvents();
            uncommittedDomainEvents.Should()
                                   .HaveCount(1)
                                   .And.ContainSingle(e => e.As<TestDomainEvent>()
                                                            .Parameter == testParameterValue);
        }

        [Fact]
        public void EnqueuedDomainEvent_AppliesTheEventToAggregate()
        {
            //Arrange
            var sut = CreateTestAggregateRootWithApplyForTestEvent();
            const string testParameterValue = "Test value";

            //Act
            sut.EnqueueTestDomainEvent(testParameterValue);

            //Assert
            sut.ParameterValue.Should()
               .Be(testParameterValue);
        }

        [Fact]
        public void EnqueuedDomainEvent_SetsVersionsOfTheEvent()
        {
            //Arrange
            var sut = CreateTestAggregateRootWithApplyForTestEvent();
            const string testParameterValue = "Test value";

            //Act
            sut.EnqueueTestDomainEvent(testParameterValue);
            sut.EnqueueTestDomainEvent(testParameterValue);
            var dequeueDomainEvents = sut.DequeueDomainEvents();

            //Assert
            dequeueDomainEvents.First()
                               .Version.Should()
                               .Be(1);
            dequeueDomainEvents.Last()
                               .Version.Should()
                               .Be(2);
        }

        [Fact]
        public void EnqueuedDomainEvent_DequeuedForSecondTimeShouldBeEmpty()
        {
            //Arrange
            var sut = CreateTestAggregateRootWithApplyForTestEvent();
            const string testParameterValue = "Test value";

            //Act
            sut.EnqueueTestDomainEvent(testParameterValue);
            var firstDequeuedEvents = sut.DequeueDomainEvents();
            var secondDequeuedEvents = sut.DequeueDomainEvents();

            //Assert
            firstDequeuedEvents.Should()
                               .HaveCount(1);
            secondDequeuedEvents.Should()
                                .BeEmpty();
        }

        private TestAggregateRootWithApplyForTestEvent CreateTestAggregateRootWithApplyForTestEvent()
        {
            return new(Guid.NewGuid().ToString());
        }

        private class TestAggregateRootWithApplyForTestEvent : AggregateRoot
        {
            public string ParameterValue { get; private set; } = string.Empty;

            public TestAggregateRootWithApplyForTestEvent(string id)
                : base(id)
            {
            }

            public void EnqueueTestDomainEvent(string parameter)
            {
                var domainEvent = new TestDomainEvent(parameter);
                EnqueueAndApplyEvent(domainEvent);
            }

            private void Apply(TestDomainEvent domainEvent)
            {
                ParameterValue = domainEvent.Parameter;
            }
        }

        private class TestDomainEvent : DomainEvent
        {
            public string Parameter { get; }

            public TestDomainEvent(string parameter)
            {
                Parameter = parameter;
            }
        }
    }
}