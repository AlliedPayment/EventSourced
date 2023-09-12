using System;
using EventSourced.Configuration;
using EventSourced.Domain;
using EventSourced.Domain.Events;
using EventSourced.Projections;
using EventSourced.Projections.Automatic;
using FluentAssertions;
using Xunit;

namespace EventSourced.Tests.Projections.Automatic
{
    public class AutomaticProjectionsEventMapperTests
    {
        private static readonly AutomaticProjectionOptions EmptyOptions = new();

        [Fact]
        public void Initialize_WhenCalledTwice_Throws()
        {
            //Arrange
            var sut = CreateSut(EmptyOptions);

            //Act
            Action act = () =>
            {
                sut.Initialize();
                sut.Initialize();
            };

            //Assert
            act.Should()
               .Throw<InvalidOperationException>();
        }

        [Fact]
        public void Initialize_WhenCalledWithEmptyProjection_Throws()
        {
            //Arrange
            var sut = CreateSut(CreateOptionsWithEmptyProjection());

            //Act
            Action act = () =>
            {
                sut.Initialize();
            };

            //Assert
            act.Should()
               .Throw<ArgumentException>();
        }

        [Fact]
        public void GetProjectionsAffectedByEvent_WithNonInitializedEvent_ReturnsEmpty()
        {
            //Arrange
            var sut = CreateSut(EmptyOptions);
            sut.Initialize();

            //Act
            var projectionTypes = sut.GetProjectionsAffectedByEvent(new EmptyEvent());

            //Assert
            projectionTypes.Should()
                           .BeEmpty();
        }

        [Fact]
        public void GetProjectionsAffectedByEvent_WithInitializedEvent_ReturnsIt()
        {
            //Arrange
            var sut = CreateSut(CreateOptionsWithValidProjection());
            sut.Initialize();

            //Act
            var projectionTypes = sut.GetProjectionsAffectedByEvent(new EmptyEvent());

            //Assert
            projectionTypes.Should()
                           .Contain(typeof(ProjectionReactingToEmptyEvent));
        }

        [Fact]
        public void GetProjectionsAffectedByAggregateChange_WithInitializedAggregateProjection_ReturnsEmpty()
        {
            //Arrange
            var sut = CreateSut(CreateOptionsWithValidProjection());
            sut.Initialize();

            //Act
            var projectionTypes = sut.GetProjectionsAffectedByAggregateChange(typeof(TestAggregateRoot));

            //Assert
            projectionTypes.Should()
                           .Contain(typeof(TestAggregateProjection));
        }

        private AutomaticProjectionOptions CreateOptionsWithEmptyProjection()
        {
            var options = new AutomaticProjectionOptions();
            options.RegisteredAutomaticProjections.Add(typeof(EmptyProjection));
            return options;
        }

        private AutomaticProjectionOptions CreateOptionsWithValidProjection()
        {
            var options = new AutomaticProjectionOptions();
            options.RegisteredAutomaticProjections.Add(typeof(ProjectionReactingToEmptyEvent));
            options.RegisteredAutomaticAggregateProjections.Add(typeof(TestAggregateProjection));
            return options;
        }

        private IAutomaticProjectionsEventMapper CreateSut(AutomaticProjectionOptions options)
        {
            return new AutomaticProjectionsEventMapper(options);
        }

        private class EmptyProjection
        {
        }

        private class EmptyEvent : DomainEvent
        {
        }

        private class ProjectionReactingToEmptyEvent
        {
            private void Apply(EmptyEvent emptyEvent)
            {
            }
        }

        private class TestAggregateProjection : AggregateProjection<TestAggregateRoot>
        {
            public TestAggregateProjection(string id)
                : base(id)
            {
            }

            private void Apply(EmptyEvent emptyEvent)
            {
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