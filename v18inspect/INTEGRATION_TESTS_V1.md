# XFramework Integration Tests v1

This project adds the first real RabbitMQ integration smoke test.

## Prerequisites

- .NET 10 SDK
- RabbitMQ reachable at localhost:5672

Example Docker command:

```powershell
docker run -d --name xframework-rabbitmq -p 5672:5672 -p 15672:15672 rabbitmq:management
```

## Run

```powershell
dotnet restore .\XFramework.slnx
dotnet test .\tests\XFramework.IntegrationTests\
```

The first test verifies broker connectivity, publish/consume, and manual acknowledgement. It does not yet exercise XFramework's EventBus/EventProcessor pipeline. Those are the next integration scenarios.
