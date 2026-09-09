using Moq;
using SmartLogger.Appenders;
using SmartLogger.Core;
using System.Collections.Concurrent;

namespace SmartLogger.Tests.Appenders;

[TestFixture]
public class AsyncAppenderWrapperTests
{
    private Mock<ILogAppender> _innerAppender = null!;
    private AsyncAppenderWrapper _wrapper = null!;

    [SetUp]
    public void SetUp()
    {
        _innerAppender = new Mock<ILogAppender>(MockBehavior.Loose);
        _wrapper = new AsyncAppenderWrapper(_innerAppender.Object);
    }

    [TearDown]
    public void TearDown()
    {
        if (_wrapper is not null)
        {
            _wrapper.Stop();
        }
    }

    [Test]
    public void Constructor_WithValidAppender_ShouldInitializeWrapper()
    {
        Assert.That(_wrapper, Is.Not.Null);
    }

    [Test]
    public void Constructor_WithNullAppender_ShouldThrowArgumentNullException()
    {
        Assert.Throws<ArgumentNullException>(() =>
            new AsyncAppenderWrapper(null!));
    }

    [Test]
    public void Constructor_WithValidAppender_ShouldExposeInnerAppender()
    {
        Assert.That(_wrapper.InnerAppender, Is.SameAs(_innerAppender.Object));
    }

    [Test]
    public void Append_WithNullMessage_ShouldNotDelegateToInnerAppender()
    {
        _wrapper.Append(null!);

        AssertEventually(
            () => _innerAppender.Verify(
                x => x.Append(It.IsAny<LogMessage>()),
                Times.Never));
    }

    [Test]
    public void Append_WithValidMessage_ShouldEventuallyDelegateToInnerAppender()
    {
        var processed = new ManualResetEventSlim(false);
        var message = CreateMessage("test");

        _innerAppender
            .Setup(x => x.Append(It.IsAny<LogMessage>()))
            .Callback(() => processed.Set());

        _wrapper.Append(message);

        Assert.That(
            processed.Wait(TimeSpan.FromSeconds(2)),
            Is.True,
            "The background worker did not process the queued message.");

        _innerAppender.Verify(
            x => x.Append(It.Is<LogMessage>(m => ReferenceEquals(m, message))),
            Times.Once);
    }

    [Test]
    public void Append_ShouldReturnWithoutWaitingForInnerAppender()
    {
        using var entered = new ManualResetEventSlim(false);
        using var release = new ManualResetEventSlim(false);

        _innerAppender
            .Setup(x => x.Append(It.IsAny<LogMessage>()))
            .Callback(() =>
            {
                entered.Set();
                release.Wait(TimeSpan.FromSeconds(2));
            });

        var start = DateTime.UtcNow;

        _wrapper.Append(CreateMessage("async"));

        Assert.That(
            DateTime.UtcNow - start,
            Is.LessThan(TimeSpan.FromSeconds(1)));

        Assert.That(
            entered.Wait(TimeSpan.FromSeconds(2)),
            Is.True,
            "The worker did not start processing the message.");

        release.Set();
    }

    [Test]
    public void Append_WithMultipleMessages_ShouldProcessInFIFOOrder()
    {
        const int count = 100;
        var processed = new List<LogMessage>();
        var completed = new CountdownEvent(count);
        var sync = new object();

        _innerAppender
            .Setup(x => x.Append(It.IsAny<LogMessage>()))
            .Callback<LogMessage>(message =>
            {
                lock (sync)
                {
                    processed.Add(message);
                }

                completed.Signal();
            });

        var messages = Enumerable
            .Range(0, count)
            .Select(i => CreateMessage($"message-{i}"))
            .ToArray();

        foreach (var message in messages)
        {
            _wrapper.Append(message);
        }

        Assert.That(
            completed.Wait(TimeSpan.FromSeconds(5)),
            Is.True,
            "Not all messages were processed.");

        Assert.That(processed, Is.EqualTo(messages));
    }

    [Test]
    public void Append_WithBurstOfMessages_ShouldPreserveFIFOOrder()
    {
        const int count = 1_000;
        var processed = new List<LogMessage>();
        var completed = new CountdownEvent(count);
        var sync = new object();

        _innerAppender
            .Setup(x => x.Append(It.IsAny<LogMessage>()))
            .Callback<LogMessage>(message =>
            {
                lock (sync)
                {
                    processed.Add(message);
                }

                completed.Signal();
            });

        var messages = Enumerable
            .Range(0, count)
            .Select(i => CreateMessage($"burst-{i}"))
            .ToArray();

        foreach (var message in messages)
        {
            _wrapper.Append(message);
        }

        Assert.That(completed.Wait(TimeSpan.FromSeconds(10)), Is.True);

        Assert.That(processed, Is.EqualTo(messages));
    }

    [Test]
    public void Stop_WithEmptyQueue_ShouldCompleteSuccessfully()
    {
        Assert.DoesNotThrow(() => _wrapper.Stop());
    }

    [Test]
    public void Stop_WithQueuedMessages_ShouldDrainQueueBeforeExit()
    {
        const int count = 100;
        using var blockWorker = new ManualResetEventSlim(false);
        using var workerEntered = new ManualResetEventSlim(false);

        var processed = new ConcurrentQueue<LogMessage>();

        _innerAppender
            .Setup(x => x.Append(It.IsAny<LogMessage>()))
            .Callback<LogMessage>(message =>
            {
                workerEntered.Set();
                blockWorker.Wait(TimeSpan.FromSeconds(5));
                processed.Enqueue(message);
            });

        var messages = Enumerable
            .Range(0, count)
            .Select(i => CreateMessage($"drain-{i}"))
            .ToArray();

        foreach (var message in messages)
        {
            _wrapper.Append(message);
        }

        Assert.That(workerEntered.Wait(TimeSpan.FromSeconds(2)), Is.True);

        var stopCompleted = new ManualResetEventSlim(false);
        var stopThread = new Thread(() =>
        {
            _wrapper.Stop();
            stopCompleted.Set();
        });

        stopThread.Start();

        Assert.That(
            stopCompleted.Wait(TimeSpan.FromMilliseconds(200)),
            Is.False,
            "Stop completed before the worker was released.");

        blockWorker.Set();

        Assert.That(
            stopCompleted.Wait(TimeSpan.FromSeconds(5)),
            Is.True,
            "Stop did not complete after the queue drained.");

        Assert.That(processed.Count, Is.EqualTo(count));
        Assert.That(processed.ToArray(), Is.EqualTo(messages));
    }

    [Test]
    public void InnerAppender_WhenAppendThrows_ShouldContinueProcessingNextMessage()
    {
        var attempts = new ConcurrentQueue<LogMessage>();
        using var secondMessageProcessed = new ManualResetEventSlim(false);

        _innerAppender
            .Setup(x => x.Append(It.IsAny<LogMessage>()))
            .Callback<LogMessage>(message =>
            {
                attempts.Enqueue(message);

                // First append fails intentionally.
                if (attempts.Count == 1)
                {
                    throw new InvalidOperationException("Simulated logging failure.");
                }

                // Second append must still be processed.
                secondMessageProcessed.Set();
            });

        var first = CreateMessage("first");
        var second = CreateMessage("second");

        _wrapper.Append(first);
        _wrapper.Append(second);

        Assert.That(
            secondMessageProcessed.Wait(TimeSpan.FromSeconds(5)),
            Is.True,
            "The worker did not continue processing after the first append failure.");

        var processedAttempts = attempts.ToArray();

        Assert.That(processedAttempts.Length, Is.EqualTo(2));
        Assert.That(processedAttempts[0], Is.SameAs(first));
        Assert.That(processedAttempts[1], Is.SameAs(second));
    }

    [Test]
    public void InnerAppender_WhenAppendThrows_ShouldNotPropagateToAppendCaller()
    {
        using var processed = new ManualResetEventSlim(false);

        _innerAppender
            .Setup(x => x.Append(It.IsAny<LogMessage>()))
            .Callback(() =>
            {
                processed.Set();
                throw new InvalidOperationException("Simulated logging failure.");
            });

        Assert.DoesNotThrow(() => _wrapper.Append(CreateMessage("failure")));

        Assert.That(processed.Wait(TimeSpan.FromSeconds(2)), Is.True);
    }

    [Test]
    public void InnerAppender_WhenMultipleAppendsThrow_ShouldContinueWorkerProcessing()
    {
        const int failureCount = 5;
        using var successfulProcessing = new ManualResetEventSlim(false);
        var attempts = 0;

        _innerAppender
            .Setup(x => x.Append(It.IsAny<LogMessage>()))
            .Callback(() =>
            {
                var attempt = Interlocked.Increment(ref attempts);

                if (attempt <= failureCount)
                {
                    throw new InvalidOperationException("Simulated failure.");
                }

                successfulProcessing.Set();
            });

        for (var i = 0; i < failureCount + 1; i++)
        {
            _wrapper.Append(CreateMessage($"message-{i}"));
        }

        Assert.That(
            successfulProcessing.Wait(TimeSpan.FromSeconds(5)),
            Is.True,
            "The worker stopped after repeated inner appender failures.");
    }

    [Test]
    public void SetLogLevel_ShouldDelegateToInnerAppender()
    {
        _wrapper.SetLogLevel(LogLevel.ERROR);

        _innerAppender.Verify(
            x => x.SetLogLevel(LogLevel.ERROR),
            Times.Once);
    }

    [Test]
    public void GetLogLevel_ShouldDelegateToInnerAppender()
    {
        _innerAppender
            .Setup(x => x.GetLogLevel(LogLevel.DEBUG))
            .Returns(LogLevel.ERROR);

        var result = _wrapper.GetLogLevel(LogLevel.DEBUG);

        Assert.That(result, Is.EqualTo(LogLevel.ERROR));

        _innerAppender.Verify(
            x => x.GetLogLevel(LogLevel.DEBUG),
            Times.Once);
    }

    [Test]
    public void IsEnabled_ShouldDelegateToInnerAppender()
    {
        _innerAppender
            .Setup(x => x.IsEnabled(LogLevel.WARNING))
            .Returns(true);

        var result = _wrapper.IsEnabled(LogLevel.WARNING);

        Assert.That(result, Is.True);

        _innerAppender.Verify(
            x => x.IsEnabled(LogLevel.WARNING),
            Times.Once);
    }

    [Test]
    public void SetFormatter_ShouldDelegateToInnerAppender()
    {
        var formatter = new Mock<ILogOutputFormatterStrategy>().Object;

        _wrapper.SetFormatter(formatter);

        _innerAppender.Verify(
            x => x.SetFormatter(formatter),
            Times.Once);
    }

    [Test]
    public void GetFormatter_ShouldDelegateToInnerAppender()
    {
        var formatter = new Mock<ILogOutputFormatterStrategy>().Object;

        _innerAppender
            .Setup(x => x.GetFormatter())
            .Returns(formatter);

        var result = _wrapper.GetFormatter();

        Assert.That(result, Is.SameAs(formatter));

        _innerAppender.Verify(
            x => x.GetFormatter(),
            Times.Once);
    }

    [Test]
    public void IsEnabled_ShouldReturnInnerAppenderResult()
    {
        _innerAppender
            .Setup(x => x.IsEnabled(LogLevel.ERROR))
            .Returns(false);

        Assert.That(_wrapper.IsEnabled(LogLevel.ERROR), Is.False);
    }

    [Test]
    public void Stop_WhenWorkerIsWaiting_ShouldWakeAndComplete()
    {
        Assert.DoesNotThrow(() => _wrapper.Stop());
    }

    [Test]
    public void Append_AfterStop_ShouldNotDelegateToInnerAppender()
    {
        _wrapper.Stop();

        _wrapper.Append(CreateMessage("after-stop"));

        Thread.Sleep(100);

        _innerAppender.Verify(
            x => x.Append(It.IsAny<LogMessage>()),
            Times.Never);
    }

    [Test]
    public void Append_FromMultipleThreads_ShouldNotThrowCollectionExceptions()
    {
        const int producerCount = 8;
        const int messagesPerProducer = 100;

        var exceptions = new ConcurrentBag<Exception>();
        var completed = new CountdownEvent(producerCount * messagesPerProducer);

        _innerAppender
            .Setup(x => x.Append(It.IsAny<LogMessage>()))
            .Callback(() => completed.Signal());

        var threads = Enumerable
            .Range(0, producerCount)
            .Select(producer => new Thread(() =>
            {
                try
                {
                    for (var i = 0; i < messagesPerProducer; i++)
                    {
                        _wrapper.Append(
                            CreateMessage($"producer-{producer}-message-{i}"));
                    }
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }))
            .ToArray();

        foreach (var thread in threads)
        {
            thread.Start();
        }

        foreach (var thread in threads)
        {
            thread.Join();
        }

        Assert.That(exceptions, Is.Empty);
        Assert.That(
            completed.Wait(TimeSpan.FromSeconds(10)),
            Is.True,
            "Not all concurrently queued messages were processed.");
    }

    [Test]
    public void Append_FromSingleProducer_ShouldPreserveProducerOrder()
    {
        const int count = 500;

        var processed = new List<LogMessage>();
        var completed = new CountdownEvent(count);
        var sync = new object();

        _innerAppender
            .Setup(x => x.Append(It.IsAny<LogMessage>()))
            .Callback<LogMessage>(message =>
            {
                lock (sync)
                {
                    processed.Add(message);
                }

                completed.Signal();
            });

        var messages = Enumerable
            .Range(0, count)
            .Select(i => CreateMessage($"ordered-{i}"))
            .ToArray();

        foreach (var message in messages)
        {
            _wrapper.Append(message);
        }

        Assert.That(completed.Wait(TimeSpan.FromSeconds(10)), Is.True);
        Assert.That(processed, Is.EqualTo(messages));
    }

    [Test]
    public void Append_WhenQueueIsFull_ShouldDropOldestMessage()
    {
        // Hold the worker on the first message so the remaining messages
        // accumulate in the queue.
        using var workerEntered = new ManualResetEventSlim(false);
        using var releaseWorker = new ManualResetEventSlim(false);

        var processed = new ConcurrentQueue<LogMessage>();

        _innerAppender
            .Setup(x => x.Append(It.IsAny<LogMessage>()))
            .Callback<LogMessage>(message =>
            {
                processed.Enqueue(message);
                workerEntered.Set();
                releaseWorker.Wait(TimeSpan.FromSeconds(10));
            });

        var first = CreateMessage("worker-blocker");
        _wrapper.Append(first);

        Assert.That(workerEntered.Wait(TimeSpan.FromSeconds(2)), Is.True);

        const int queuedCount = 10_000;
        var messages = Enumerable
            .Range(0, queuedCount)
            .Select(i => CreateMessage($"queued-{i}"))
            .ToArray();

        foreach (var message in messages)
        {
            _wrapper.Append(message);
        }

        // Adding one more message while the queue is full must evict queued-0.
        var newest = CreateMessage("newest");
        _wrapper.Append(newest);

        releaseWorker.Set();

        _wrapper.Stop();

        var processedMessages = processed.ToArray();

        Assert.That(processedMessages, Does.Contain(first));
        Assert.That(processedMessages, Does.Contain(newest));
        Assert.That(processedMessages, Does.Not.Contain(messages[0]));

        for (var i = 1; i < messages.Length; i++)
        {
            Assert.That(processedMessages, Does.Contain(messages[i]));
        }
    }

    [Test]
    public void Append_WhenQueueIsBelowCapacity_ShouldQueueAllMessages()
    {
        using var workerEntered = new ManualResetEventSlim(false);
        using var releaseWorker = new ManualResetEventSlim(false);

        var processed = new ConcurrentQueue<LogMessage>();

        _innerAppender
            .Setup(x => x.Append(It.IsAny<LogMessage>()))
            .Callback<LogMessage>(message =>
            {
                processed.Enqueue(message);
                workerEntered.Set();
                releaseWorker.Wait(TimeSpan.FromSeconds(10));
            });

        var blocker = CreateMessage("blocker");
        _wrapper.Append(blocker);

        Assert.That(workerEntered.Wait(TimeSpan.FromSeconds(2)), Is.True);

        const int count = 100;
        var messages = Enumerable
            .Range(0, count)
            .Select(i => CreateMessage($"message-{i}"))
            .ToArray();

        foreach (var message in messages)
        {
            _wrapper.Append(message);
        }

        releaseWorker.Set();
        _wrapper.Stop();

        Assert.That(processed.ToArray(), Is.EqualTo(new[] { blocker }.Concat(messages)));
    }

    private static LogMessage CreateMessage(string message)
    {
        return new LogMessage.Builder()
            .WithLevel(LogLevel.INFO)
            .WithMessage(message)
            .Build();
    }

    private static void AssertEventually(Action assertion)
    {
        var deadline = DateTime.UtcNow.AddSeconds(2);
        Exception? lastException = null;

        while (DateTime.UtcNow < deadline)
        {
            try
            {
                assertion();
                return;
            }
            catch (Exception ex)
            {
                lastException = ex;
                Thread.Sleep(10);
            }
        }

        throw new AssertionException(
            $"Condition was not satisfied within the timeout. Last failure: {lastException?.Message}");
    }
}