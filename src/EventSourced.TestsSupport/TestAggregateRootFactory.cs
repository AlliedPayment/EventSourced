using System;
using System.Collections.Generic;
using EventSourced.Domain;
using EventSourced.Domain.Events;
using EventSourced.Helpers;
using Newtonsoft.Json;

namespace EventSourced.TestsSupport
{
    public static class TestAggregateRootFactory
    {
        public static TAggregateRoot CreateAggregateFromEvents<TAggregateRoot>(string id, string eventsArrayJson)
            where TAggregateRoot : AggregateRoot
        {
            var aggregateRoot = AggregateRootFactory.CreateAggregateRoot<TAggregateRoot>(id);
            var events = JsonConvert.DeserializeObject<IEnumerable<DomainEvent>>(
                eventsArrayJson,
                new JsonSerializerSettings {TypeNameHandling = TypeNameHandling.Auto});
            if (events == null)
            {
                throw new ArgumentException($"The factory was unable to deserialize events from json {eventsArrayJson}");
            }
            aggregateRoot.RebuildFromEvents(events);
            return aggregateRoot;
        }
        
        public static TAggregateRoot CreateAggregateFromState<TAggregateRoot>(string id , string aggregateStateJson)
            where TAggregateRoot : AggregateRoot
        {
            var aggregateRoot = AggregateRootFactory.CreateAggregateRoot<TAggregateRoot>(id);
            JsonConvert.PopulateObject(aggregateStateJson, aggregateRoot);
            return aggregateRoot;
        } 
    }
}