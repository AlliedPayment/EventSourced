using System;
using EventSourced.Domain;
using BindingFlags = System.Reflection.BindingFlags;

namespace EventSourced.Helpers
{
    public class AggregateRootFactory
    {
        public static TAggregateRoot CreateAggregateRoot<TAggregateRoot>(string id) where TAggregateRoot : AggregateRoot =>
            (TAggregateRoot) CreateAggregateRoot(id, typeof(TAggregateRoot));

        public static AggregateRoot CreateAggregateRoot(string id, Type type)
        {
            var constructorInfo = type.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public,
                                                      null,
                                                      new[] {typeof(string)},
                                                      null);
            if (constructorInfo == null)
            {
                throw new ArgumentException($"Could not find constructor accepting a string on aggregate of type {type.FullName}");
            }
            else
            {
                return (AggregateRoot) constructorInfo.Invoke(new object?[] {id});
            }
        }
    }
}