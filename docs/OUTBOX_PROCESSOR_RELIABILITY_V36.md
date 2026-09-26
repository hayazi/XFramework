# Outbox Processor Reliability — v36

## Baseline
- Unit tests: 10/10 passed
- Integration tests before this source fix: 17 tests existed, but the new reliability test file failed to compile because it omitted `using Xunit;`.

## Fix
Added the explicit xUnit namespace import to:

`tests/XFramework.IntegrationTests/Scenarios/OutboxProcessorReliabilityTests.cs`

No production behavior was changed in v36.

## Expected result
After applying v36:

- Unit tests: 10/10
- Integration tests: 21/21
- Failed: 0
- Skipped: 0
