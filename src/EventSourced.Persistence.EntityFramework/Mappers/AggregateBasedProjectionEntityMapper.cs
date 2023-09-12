using System;
using EventSourced.Persistence.EntityFramework.Entities;
using EventSourced.Persistence.EntityFramework.Helpers;
using EventSourced.Projections;
using Newtonsoft.Json;

namespace EventSourced.Persistence.EntityFramework.Mappers
{
    public class AggregateBasedProjectionEntityMapper : IAggregateBasedProjectionEntityMapper
    {
        private readonly ITypeSerializer _typeSerializer;

        public AggregateBasedProjectionEntityMapper(ITypeSerializer typeSerializer)
        {
            _typeSerializer = typeSerializer;
        }

        public AggregateBasedProjectionEntity MapToEntity(string aggregateRootId, object projection)
        {
            var serializedType = _typeSerializer.SerializeType(projection.GetType());
            return new AggregateBasedProjectionEntity
            {
                AggregateRootId = aggregateRootId,
                SerializedProjectionType = serializedType,
                SerializedProjectionData = JsonConvert.SerializeObject(projection, JsonSerializerSettingsProvider.Settings)
            };
        }

        public object MapToProjection(AggregateBasedProjectionEntity entity)
        {
            var deserializedType = _typeSerializer.DeserializeType(entity.SerializedProjectionType);
            var instance = Activator.CreateInstance(deserializedType, entity.AggregateRootId)!;
            JsonConvert.PopulateObject(entity.SerializedProjectionData, instance, JsonSerializerSettingsProvider.Settings);
            return instance;
        } 
    }
}