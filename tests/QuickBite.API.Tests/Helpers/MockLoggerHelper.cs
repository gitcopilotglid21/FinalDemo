using Microsoft.Extensions.Logging;
using Moq;

namespace QuickBite.API.Tests.Helpers
{
    /// <summary>
    /// Helper class for creating and verifying mock loggers in tests
    /// </summary>
    public static class MockLoggerHelper
    {
        /// <summary>
        /// Creates a mock logger for the specified type
        /// </summary>
        /// <typeparam name="T">The type for which to create a logger</typeparam>
        /// <returns>A mock logger instance</returns>
        public static Mock<ILogger<T>> CreateMockLogger<T>()
        {
            return new Mock<ILogger<T>>();
        }

        /// <summary>
        /// Verifies that a log message was written at the specified level
        /// </summary>
        /// <typeparam name="T">The logger type</typeparam>
        /// <param name="mockLogger">The mock logger to verify</param>
        /// <param name="logLevel">The expected log level</param>
        /// <param name="times">How many times the log should have been called</param>
        public static void VerifyLog<T>(
            this Mock<ILogger<T>> mockLogger,
            LogLevel logLevel,
            Times times)
        {
            mockLogger.Verify(
                x => x.Log(
                    logLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                times);
        }

        /// <summary>
        /// Verifies that a log message was written at the specified level with a specific message
        /// </summary>
        /// <typeparam name="T">The logger type</typeparam>
        /// <param name="mockLogger">The mock logger to verify</param>
        /// <param name="logLevel">The expected log level</param>
        /// <param name="expectedMessage">The expected message content</param>
        /// <param name="times">How many times the log should have been called</param>
        public static void VerifyLogContains<T>(
            this Mock<ILogger<T>> mockLogger,
            LogLevel logLevel,
            string expectedMessage,
            Times times)
        {
            mockLogger.Verify(
                x => x.Log(
                    logLevel,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains(expectedMessage)),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                times);
        }

        /// <summary>
        /// Verifies that an error was logged with an exception
        /// </summary>
        /// <typeparam name="T">The logger type</typeparam>
        /// <param name="mockLogger">The mock logger to verify</param>
        /// <param name="times">How many times the error should have been logged</param>
        public static void VerifyErrorWithException<T>(
            this Mock<ILogger<T>> mockLogger,
            Times times)
        {
            mockLogger.Verify(
                x => x.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => true),
                    It.IsAny<Exception>(),
                    It.IsAny<Func<It.IsAnyType, Exception?, string>>()),
                times);
        }

        /// <summary>
        /// Verifies that an information log was written
        /// </summary>
        /// <typeparam name="T">The logger type</typeparam>
        /// <param name="mockLogger">The mock logger to verify</param>
        /// <param name="times">How many times the info should have been logged</param>
        public static void VerifyInformation<T>(
            this Mock<ILogger<T>> mockLogger,
            Times times)
        {
            mockLogger.VerifyLog(LogLevel.Information, times);
        }

        /// <summary>
        /// Verifies that an error log was written
        /// </summary>
        /// <typeparam name="T">The logger type</typeparam>
        /// <param name="mockLogger">The mock logger to verify</param>
        /// <param name="times">How many times the error should have been logged</param>
        public static void VerifyError<T>(
            this Mock<ILogger<T>> mockLogger,
            Times times)
        {
            mockLogger.VerifyLog(LogLevel.Error, times);
        }
    }
}