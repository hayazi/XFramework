using System.Diagnostics;

namespace XFramework.Infrastructure.Messaging;

public static class MessagingDiagnostics
{
    public const string ActivitySourceName = "XFramework.Messaging";

    public static readonly ActivitySource ActivitySource = new(ActivitySourceName);
}