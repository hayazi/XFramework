namespace XFramework.Application.Contracts.Authorization;

public interface IPermissionDefinitionProvider
{
    void Define(
        IPermissionDefinitionContext context);
}