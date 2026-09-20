using XFramework.Application.Contracts.Authorization;

namespace XFramework.Application.Authorization;

public sealed class PermissionDefinitionRegistry
{
    private readonly PermissionDefinitionContext _context;

    public PermissionDefinitionRegistry(
        IEnumerable<IPermissionDefinitionProvider> providers)
    {
        _context = new PermissionDefinitionContext();

        foreach (var provider in providers)
        {
            provider.Define(_context);
        }
    }

    public PermissionDefinitionContext Context =>
        _context;

    public PermissionDefinition? GetPermission(
        string name)
    {
        return _context.GetPermission(name);
    }

    public IReadOnlyList<PermissionDefinition>
        GetGroups()
    {
        return _context.GetGroups();
    }
}