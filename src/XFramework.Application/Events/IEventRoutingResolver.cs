public interface IEventRoutingResolver
{
    string GetRoutingKey(
        string eventType,
        int eventVersion);
}