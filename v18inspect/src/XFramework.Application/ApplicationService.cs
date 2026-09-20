using XFramework.Application.Contracts.Authorization;

namespace XFramework.Application;

public abstract class ApplicationService
{
    protected ICurrentUser CurrentUser { get; }

    protected ApplicationService(
        ICurrentUser currentUser)
    {
        CurrentUser = currentUser;
    }
}