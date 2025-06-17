using Microsoft.Extensions.Logging;

namespace AideMemoire.Tests.Utilities;

public class TestLogger<T> : ILogger<T> {
    private readonly List<string> _logMessages = [];

    public IReadOnlyList<string> LogMessages => _logMessages.AsReadOnly();

    public IDisposable? BeginScope<TState>(TState state) where TState : notnull =>
        throw new NotImplementedException("BeginScope is not implemented in TestLogger");

    public bool IsEnabled(LogLevel logLevel) => true;

    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) {
        var message = formatter(state, exception);
        _logMessages.Add($"[{logLevel}] {message}");
    }

    public void Clear() => _logMessages.Clear();
}
