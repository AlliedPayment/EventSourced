using System;
using EventSourced.Helpers;
using EventSourced.Sample.Warehouse.Domain.Exceptions;
using EventSourced.Sample.Warehouse.Domain.ImportLocation;
using EventSourced.TestsSupport;
using FluentAssertions;
using Newtonsoft.Json;
using Xunit;

namespace EventSourced.Sample.Warehouse.Domain.Tests
{
    public class ImportLocationAggregateRootTests
    {
        [Fact]
        public void MoveToContainer_WithLesserAmount_Throws()
        {
            //Arrange
            var aggregateState =
                @"{""ImportedItems"":[{""WarehouseItemId"":""4e52e08d-2282-45bd-a480-e8775c12589b"",""Amount"":25},{""WarehouseItemId"":""4af48b93-7c96-49d0-b055-f19380636563"",""Amount"":8},{""WarehouseItemId"":""0ab33a01-8001-452e-9114-b207de6b8122"",""Amount"":40}],""Version"":10,""Id"":""3de7b3d9-0c35-4cd3-8dea-e29baea0f8af""}";
            var importLocationAggregateRoot = TestAggregateRootFactory.CreateAggregateFromState<ImportLocationAggregateRoot>(new Guid("3de7b3d9-0c35-4cd3-8dea-e29baea0f8af").ToString(), aggregateState);
            
            //Act
            Action action = () => importLocationAggregateRoot.MoveItem(new Guid("4af48b93-7c96-49d0-b055-f19380636563"), Guid.NewGuid(), 10);
            
            //Assert
            action.Should()
                  .Throw<BusinessRuleException>();
        }
    }
}