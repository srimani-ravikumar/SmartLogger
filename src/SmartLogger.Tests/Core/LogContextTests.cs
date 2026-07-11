using SmartLogger.Core;
using System.Reflection;

namespace SmartLogger.Tests.Core
{
    [TestFixture]
    public class LogContextTests
    {
        [SetUp]
        public void Setup()
        {
            ResetCorrelationContext();
        }

        [TearDown]
        public void TearDown()
        {
            ResetCorrelationContext();
        }

        private static void ResetCorrelationContext()
        {
            var field = typeof(LogContext).GetField("_correlationId", BindingFlags.NonPublic | BindingFlags.Static);
            var asyncLocal = field?.GetValue(null);
            var valueProperty = asyncLocal?.GetType().GetProperty("Value");
            valueProperty?.SetValue(asyncLocal, null);
        }

        [Test]
        public void CorrelationId_WithoutActiveScope_ShouldReturnNull()
        {
            // Arrange
            // Act
            var correlationId = LogContext.CorrelationId;

            // Assert
            Assert.That(correlationId, Is.Null);
        }

        [Test]
        public void BeginCorrelationScope_WithValue_ShouldSetCurrentCorrelationId()
        {
            // Arrange
            // Act
            using (LogContext.BeginCorrelationScope("req-123"))
            {
                // Assert
                Assert.That(LogContext.CorrelationId, Is.EqualTo("req-123"));
            }
        }

        [Test]
        public void BeginCorrelationScope_WhenDisposed_ShouldRestorePreviousCorrelationId()
        {
            // Arrange
            using (LogContext.BeginCorrelationScope("outer"))
            {
                Assert.That(LogContext.CorrelationId, Is.EqualTo("outer"));

                // Act
                using (LogContext.BeginCorrelationScope("inner"))
                {
                    Assert.That(LogContext.CorrelationId, Is.EqualTo("inner"));
                }

                // Assert
                Assert.That(LogContext.CorrelationId, Is.EqualTo("outer"));
            }

            Assert.That(LogContext.CorrelationId, Is.Null);
        }

        [Test]
        public async Task BeginCorrelationScope_ShouldFlowAcrossAsyncBoundary()
        {
            // Arrange
            using (LogContext.BeginCorrelationScope("req-async"))
            {
                // Act
                var observed = await Task.Run(() => LogContext.CorrelationId);

                // Assert
                Assert.That(observed, Is.EqualTo("req-async"));
            }
        }

        [Test]
        public async Task BeginCorrelationScope_ShouldRemainIsolatedAcrossConcurrentFlows()
        {
            // Arrange
            using (LogContext.BeginCorrelationScope("outer-flow"))
            {
                var firstTask = Task.Run(() => LogContext.CorrelationId);
                var secondTask = Task.Run(() =>
                {
                    using (LogContext.BeginCorrelationScope("inner-flow"))
                    {
                        return LogContext.CorrelationId;
                    }
                });

                // Act
                var observedOuter = await firstTask;
                var observedInner = await secondTask;

                // Assert
                Assert.That(observedOuter, Is.EqualTo("outer-flow"));
                Assert.That(observedInner, Is.EqualTo("inner-flow"));
            }
        }

        [Test]
        public void BeginCorrelationScope_WithNullValue_ShouldAllowNullContext()
        {
            // Arrange
            // Act
            using (LogContext.BeginCorrelationScope(null))
            {
                // Assert
                Assert.That(LogContext.CorrelationId, Is.Null);
            }
        }
    }
}
