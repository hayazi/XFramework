using Microsoft.Extensions.Localization;
using XFramework.Application.Contracts.Localization;

namespace XFramework.Blazor.Localization;

public sealed class BlazorLocalizationService(
    IStringLocalizer<SharedResource> localizer)
    : ILocalizationService
{
    public string Get(
        string key,
        string? defaultValue = null)
    {
        var value = localizer[key];

        if (!value.ResourceNotFound)
            return value.Value;

        return defaultValue ?? key;
    }

    public string Get(
        string key,
        params object[] arguments)
    {
        var value = localizer[key, arguments];

        if (!value.ResourceNotFound)
            return value.Value;

        return key;
    }
}