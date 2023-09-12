using System;
using EventSourced.Configuration;
using EventSourced.Domain;
using EventSourced.Domain.Events;
using EventSourced.Persistence;
using EventSourced.Persistence.EntityFramework.Configuration;
using EventSourced.Persistence.InMemory.Configuration;
using EventSourced.Projections;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace EventSourced.Tests.Configuration
{
    public class ServiceCollectionConfigurationTests
    {
        [Fact]
        public void AddEventSourced_WithInMemoryDatabase_AllowsResolvingOfRepository()
        {
            //Arrange
            var serviceCollection = new ServiceCollection();

            //Act
            serviceCollection.AddEventSourced(options => options.UseInMemoryEventStore()
                                                                .UseInMemoryProjectionStore()
                                                                .UseInMemorySnapshotStore()
                                                                .RegisterAutomaticProjection<TestProjection>()
                                                                .RegisterAutomaticAggregateProjection<TestAggregateProjection,
                                                                    TestAggregateRoot>());

            //Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var repository = serviceProvider.GetRequiredService<IRepository<TestAggregateRoot>>();
            repository.Should()
                      .NotBeNull();
        }
        
        [Fact]
        public void AddEventSourced_WithEntityFrameworkDatabase_AllowsResolvingOfRepository()
        {
            //Arrange
            var serviceCollection = new ServiceCollection();

            //Act
            serviceCollection.AddEventSourced(options => options.AddEntityFrameworkSupport(o => { })
                                                                .UseEntityFrameworkEventStore()
                                                                .UseEntityFrameworkProjectionStore()
                                                                .UseEntityFrameworkSnapshotStore()
                                                                .RegisterAutomaticProjection<TestProjection>()
                                                                .RegisterAutomaticAggregateProjection<TestAggregateProjection,
                                                                    TestAggregateRoot>());

            //Assert
            var serviceProvider = serviceCollection.BuildServiceProvider();
            var repository = serviceProvider.GetRequiredService<IRepository<TestAggregateRoot>>();
            repository.Should()
                      .NotBeNull();
        }

        private class TestProjection
        {
            private void Apply(DomainEvent eventObject)
            {
            }
        }

        private class TestAggregateProjection : AggregateProjection<TestAggregateRoot>
        {
            public TestAggregateProjection(string id)
                : base(id)
            {
            }

            private void Apply(DomainEvent eventObject)
            {
            }
        }

        private class TestAggregateRoot : AggregateRoot
        {
            public TestAggregateRoot()
                : base(Guid.NewGuid().ToString())
            {
            }

            public TestAggregateRoot(string id)
                : base(id)
            {
            }
        }
    }
}