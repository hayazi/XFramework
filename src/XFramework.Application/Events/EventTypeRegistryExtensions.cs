using System.Reflection;
using XFramework.Domain.Events;

namespace XFramework.Application.Events;

public static class EventTypeRegistryExtensions
{
    public static void RegisterEventsFromAssembly(
        this EventTypeRegistry registry,
        Assembly assembly)
    {
        var eventTypes = assembly
            .GetTypes()
            .Where(type =>
                type is { IsClass: true, IsAbstract: false } &&
                typeof(IDomainEvent).IsAssignableFrom(type) &&
                type.GetCustomAttributes<EventTypeAttribute>()
                    .Any());

        foreach (var eventType in eventTypes)
        {
            registry.Register(eventType);
        }
    }
}