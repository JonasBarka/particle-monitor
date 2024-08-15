using Microsoft.Extensions.Logging;

namespace ParticleMonitorTests.Functions
{
    public static class TestExtensions
    {
        public static void AssertRecieved(this ILogger logger, int occurances, LogLevel logLevel)
        {
            logger.Received(occurances).Log(
                logLevel,
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }

        public static void AssertRecieved(this ILogger logger, int occurances)
        {
            logger.Received(occurances).Log(
                Arg.Any<LogLevel>(),
                Arg.Any<EventId>(),
                Arg.Any<object>(),
                Arg.Any<Exception>(),
                Arg.Any<Func<object, Exception?, string>>());
        }
    }
}
