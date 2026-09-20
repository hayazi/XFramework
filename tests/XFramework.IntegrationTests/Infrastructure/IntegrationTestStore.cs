namespace XFramework.IntegrationTests.Infrastructure;

public sealed class IntegrationTestStore
{
    private readonly object _sync = new();
    private readonly Dictionary<Guid, string> _messages = new();

    public void Record(Guid id, string message)
    {
        lock (_sync)
        {
            _messages[id] = message;
        }
    }

    public bool Contains(Guid id)
    {
        lock (_sync)
        {
            return _messages.ContainsKey(id);
        }
    }

    public int Count(Guid id)
    {
        lock (_sync)
        {
            return _messages.ContainsKey(id) ? 1 : 0;
        }
    }

    public string? Get(Guid id)
    {
        lock (_sync)
        {
            return _messages.TryGetValue(id, out var message) ? message : null;
        }
    }

    public void Clear()
    {
        lock (_sync)
        {
            _messages.Clear();
        }
    }
}
