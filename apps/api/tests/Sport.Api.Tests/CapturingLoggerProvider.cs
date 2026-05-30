using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;

namespace Sport.Api.Tests;

/// <summary>
/// In-memory <see cref="ILoggerProvider"/> that records every log entry so tests can assert on
/// structured logging behaviour (category, level, rendered message) without a real sink.
/// </summary>
public sealed class CapturingLoggerProvider : ILoggerProvider
{
    public ConcurrentQueue<Entry> Entries { get; } = new();

    public ILogger CreateLogger(string categoryName) => new CapturingLogger(categoryName, Entries);

    public void Dispose() { }

    public sealed record Entry(string Category, LogLevel Level, string Message);

    private sealed class CapturingLogger : ILogger
    {
        private readonly string _category;
        private readonly ConcurrentQueue<Entry> _entries;

        public CapturingLogger(string category, ConcurrentQueue<Entry> entries)
        {
            _category = category;
            _entries = entries;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(
            LogLevel logLevel,
            EventId eventId,
            TState state,
            Exception? exception,
            Func<TState, Exception?, string> formatter)
            => _entries.Enqueue(new Entry(_category, logLevel, formatter(state, exception)));
    }
}
