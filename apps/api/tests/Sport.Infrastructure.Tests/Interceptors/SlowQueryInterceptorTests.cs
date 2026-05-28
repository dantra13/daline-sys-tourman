using System.Data.Common;
using FluentAssertions;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using Sport.Infrastructure.Interceptors;

namespace Sport.Infrastructure.Tests.Interceptors;

public class SlowQueryInterceptorTests
{
    [Fact]
    public void LogsWarning_When_DurationExceedsThreshold()
    {
        var logger = new ListLogger<SlowQueryInterceptor>();
        var options = Options.Create(new SlowQueryOptions { ThresholdMs = 100 });
        var interceptor = new SlowQueryInterceptor(logger, options);

        var command = Mock.Of<DbCommand>(c => c.CommandText == "SELECT 1");
        var eventData = MakeEventData(command, TimeSpan.FromMilliseconds(150));

        interceptor.ReaderExecuted(command, eventData, result: null!);

        logger.Entries.Should().ContainSingle(e =>
            e.Level == LogLevel.Warning &&
            e.Message.Contains("Slow query") &&
            e.Message.Contains("150"));
    }

    [Fact]
    public void DoesNotLog_When_DurationBelowThreshold()
    {
        var logger = new ListLogger<SlowQueryInterceptor>();
        var options = Options.Create(new SlowQueryOptions { ThresholdMs = 100 });
        var interceptor = new SlowQueryInterceptor(logger, options);

        var command = Mock.Of<DbCommand>(c => c.CommandText == "SELECT 1");
        var eventData = MakeEventData(command, TimeSpan.FromMilliseconds(50));

        interceptor.ReaderExecuted(command, eventData, result: null!);

        logger.Entries.Should().BeEmpty();
    }

    private static CommandExecutedEventData MakeEventData(DbCommand command, TimeSpan duration)
    {
        return new CommandExecutedEventData(
            eventDefinition: null!,
            messageGenerator: null!,
            connection: null!,
            command: command,
            logCommandText: string.Empty,
            context: null,
            executeMethod: DbCommandMethod.ExecuteReader,
            commandId: Guid.NewGuid(),
            connectionId: Guid.NewGuid(),
            result: null,
            async: false,
            logParameterValues: false,
            startTime: DateTimeOffset.UtcNow,
            duration: duration,
            commandSource: CommandSource.Unknown);
    }

    private sealed class ListLogger<T> : ILogger<T>
    {
        public List<(LogLevel Level, string Message)> Entries { get; } = new();
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullScope.Instance;
        public bool IsEnabled(LogLevel logLevel) => true;
        public void Log<TState>(
            LogLevel logLevel, EventId eventId, TState state, Exception? exception,
            Func<TState, Exception?, string> formatter)
                => Entries.Add((logLevel, formatter(state, exception)));

        private sealed class NullScope : IDisposable
        {
            public static readonly NullScope Instance = new();
            public void Dispose() { }
        }
    }
}
