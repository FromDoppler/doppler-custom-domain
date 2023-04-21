using Microsoft.Extensions.Logging;
using Moq;
using System;

namespace DopplerCustomDomain.Test
{
    public static class LoggerMockExtensions
    {
        public static void VerifyLog<T>(this Mock<ILogger<T>> mock, LogLevel logLevel, string contains, Func<Times> times)
        {
            mock.Verify(x => x.Log(
                logLevel,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, _) => (v.ToString() ?? "").Contains(contains)),
                null,
                It.IsAny<Func<It.IsAnyType, Exception?, string>>()), times);
        }
    }
}
