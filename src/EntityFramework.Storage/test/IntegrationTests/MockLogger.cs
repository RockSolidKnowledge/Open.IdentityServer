using System;
using Microsoft.Extensions.Logging;
using Moq;

namespace Open.IdentityServer.EntityFramework.IntegrationTests;

public class MockLogger<T> : ILogger<T>
{
    private readonly ILogger<T> _mock = Mock.Of<ILogger<T>>();

    public static MockLogger<T> Create() => new();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull
        => _mock.BeginScope(state);

    public bool IsEnabled(LogLevel logLevel)
        => _mock.IsEnabled(logLevel);

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        => _mock.Log(logLevel, eventId, state, exception, formatter);

    public void VerifyLog(LogLevel level, string message, Times? times = null)
    {
        Mock.Get(_mock)
            .Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => v.ToString()!.Contains(message)),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times ?? Times.Once());
    }

    public void VerifyLog(LogLevel level, Times? times = null)
    {
        Mock.Get(_mock)
            .Verify(
            x => x.Log(
                level,
                It.IsAny<EventId>(),
                It.IsAny<It.IsAnyType>(),
                It.IsAny<Exception?>(),
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
            times ?? Times.Once());
    }
}