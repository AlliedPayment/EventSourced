using System;
using EventSourced.Sample.Warehouse.Domain.Container;
using EventSourced.TestsSupport;
using FluentAssertions;
using Xunit;

namespace EventSourced.Sample.Warehouse.Domain.Tests
{
    public class ContainerAggregateRootTests
    {
        [Fact]
        public void RemoveItemFromContainer_WhenExists_RemovesIt()
        {
            //Arrange
            var warehouseItemId = new Guid("23707e9a-924e-4b6e-a787-837a488b1664").ToString();
            var aggregateRootId = new Guid( "bc6d3c2f-5b26-4a3f-8bbe-7812ad6181e9").ToString();
            var containerAggregateRoot = TestAggregateRootFactory.CreateAggregateFromEvents<ContainerAggregateRoot>(aggregateRootId, "[{\"$type\":\"EventSourced.Sample.Warehouse.Domain.Container.Events.ContainerCreatedDomainEvent, EventSourced.Sample.Warehouse.Domain\",\"Id\":\"bc6d3c2f-5b26-4a3f-8bbe-7812ad6181e9\",\"Identifier\":\"Coffee Container 01\",\"Version\":1},{\"$type\":\"EventSourced.Sample.Warehouse.Domain.Container.Events.ReceivedItemFromImportLocationDomainEvent, EventSourced.Sample.Warehouse.Domain\",\"WarehouseItemId\":\"23707e9a-924e-4b6e-a787-837a488b1664\",\"Amount\":50,\"Version\":2},{\"$type\":\"EventSourced.Sample.Warehouse.Domain.Container.Events.ReceivedItemFromImportLocationDomainEvent, EventSourced.Sample.Warehouse.Domain\",\"WarehouseItemId\":\"23707e9a-924e-4b6e-a787-837a488b1664\",\"Amount\":50,\"Version\":3}]");
            
            //Act
            containerAggregateRoot.RemoveItemFromContainer(warehouseItemId, 50);
            
            //Assert
            containerAggregateRoot.WarehouseItemsInContainer.Should()
                                  .ContainSingle(x => x.WarehouseItemId == warehouseItemId)
                                  .Which.Amount.Should()
                                  .Be(50);
        }
    }
}