namespace XFramework.Application.Contracts.Localization;

public interface ILocalizationService
{
    string Get(
        string key,
        string? defaultValue = null);

    string Get(
        string key,
        params object[] arguments);
}