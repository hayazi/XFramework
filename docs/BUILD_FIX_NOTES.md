# Build Fix Notes

## v8

Removed the duplicate `BusinessException` declaration from `XFramework.Domain/Exceptions`.

The file was incorrectly declared with the namespace `XFramework.Application.Exceptions`, so the same CLR type name existed in both the Domain and Application assemblies and caused CS0433 in Blazor.

The canonical `BusinessException` remains in:

`XFramework.Application/Exceptions/BusinessException.cs`

No other source changes were made in this fix.
