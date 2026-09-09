# AsyncAppenderWrapper Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                                                                                                         |
| ------- | ---------- | ------- | ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-09 | Srimani | Initial Draft | Defined the unit test coverage for the `AsyncAppenderWrapper` class, validating asynchronous message processing, FIFO ordering, queue backpressure, delegation, worker lifecycle, shutdown behavior, exception isolation, and concurrent producers. |

# Objective

Validate that **AsyncAppenderWrapper** correctly provides asynchronous logging behavior by:

* Buffering log messages in an in-memory queue.
* Processing messages asynchronously through a dedicated worker thread.
* Preserving FIFO ordering.
* Applying queue backpressure when the queue reaches its maximum capacity.
* Dropping the oldest queued message when the queue is full.
* Delegating appender configuration operations to the wrapped appender.
* Isolating exceptions thrown by the underlying appender.
* Gracefully stopping the worker.
* Draining queued messages before shutdown.
* Safely coordinating concurrent producers and the background consumer.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * A fresh `AsyncAppenderWrapper` should be created for every test.
  * `Stop()` should be called during test cleanup to prevent background worker leakage.
  * Tests involving asynchronous processing should use synchronization primitives such as `ManualResetEventSlim`, `CountdownEvent`, or `TaskCompletionSource` rather than arbitrary `Thread.Sleep()` delays.
* Concurrency:

  * Tests should avoid timing-sensitive assertions wherever possible.
  * The underlying `ILogAppender` should be mocked to observe delegation and ordering.

# Initialization Tests

## Wrapper should initialize successfully with a valid inner appender

Validated by:

* `Constructor_WithValidAppender_ShouldInitializeWrapper`

## Wrapper should reject a null inner appender

Validated by:

* `Constructor_WithNullAppender_ShouldThrowArgumentNullException`

Verifies that the wrapper cannot operate without an underlying appender.

## Wrapper should expose the supplied inner appender

Validated by:

* `Constructor_WithValidAppender_ShouldExposeInnerAppender`

Verifies that `InnerAppender` references the exact appender supplied to the constructor.

## Wrapper should start the background worker during construction

Validated by:

* `Constructor_WithValidAppender_ShouldStartBackgroundWorker`

The worker is expected to be running immediately after construction and ready to process queued messages.

# Append Tests

## Wrapper should ignore a null log message

Validated by:

* `Append_WithNullMessage_ShouldNotDelegateToInnerAppender`

Null messages should not enter the queue or reach the underlying appender.

## Wrapper should enqueue a valid message

Validated by:

* `Append_WithValidMessage_ShouldEventuallyDelegateToInnerAppender`

Verifies that an appended message is eventually processed by the background worker.

## Wrapper should process messages asynchronously

Validated by:

* `Append_ShouldReturnWithoutWaitingForInnerAppender`

The caller should not be blocked waiting for the underlying appender to finish its operation.

The test should use a controlled blocking inner appender to verify that `Append()` returns while the worker remains independently blocked.

## Wrapper should signal the worker after enqueueing a message

Validated by:

* `Append_WithQueuedMessage_ShouldWakeWaitingWorker`

Verifies the observable behavior that a waiting worker begins processing after a message is appended.

# FIFO Processing Tests

## Wrapper should process messages in FIFO order

Validated by:

* `Append_WithMultipleMessages_ShouldProcessInFIFOOrder`

Verifies that messages are delegated in the same order in which they were successfully queued.

Example:

```text
Append(A)
Append(B)
Append(C)

        ↓

InnerAppender.Append(A)
InnerAppender.Append(B)
InnerAppender.Append(C)
```

## Wrapper should preserve FIFO order for a burst of messages

Validated by:

* `Append_WithBurstOfMessages_ShouldPreserveFIFOOrder`

Verifies that rapid sequential producers do not reorder messages.

## Wrapper should process all queued messages before normal shutdown

Validated by:

* `Stop_WithQueuedMessages_ShouldDrainQueueBeforeExit`

This is a critical lifecycle guarantee documented by `Stop()`.

# Queue Capacity Tests

## Wrapper should accept messages while the queue is below maximum capacity

Validated by:

* `Append_WhenQueueIsBelowCapacity_ShouldQueueAllMessages`

## Wrapper should maintain the maximum queue size

Validated by:

* `Append_WhenQueueReachesMaximumCapacity_ShouldNotExceedConfiguredLimit`

The queue must never contain more than:

```text
10,000 messages
```

## Wrapper should drop the oldest message when the queue is full

Validated by:

* `Append_WhenQueueIsFull_ShouldDropOldestMessage`

Verifies the current backpressure behavior:

```text
Queue full
    ↓
Dequeue oldest
    ↓
Enqueue newest
```

## Wrapper should retain newer messages when backpressure occurs

Validated by:

* `Append_WhenQueueIsFull_ShouldRetainNewestMessages`

Ensures that the newest messages continue to be accepted after the oldest queued message is removed.

> **Note:** The current implementation intentionally drops the oldest message. The TODO indicates a future policy may instead drop lower-priority messages. Tests should therefore document the current behavior rather than a future design.

# Delegation Tests

## Wrapper should delegate SetLogLevel to the inner appender

Validated by:

* `SetLogLevel_ShouldDelegateToInnerAppender`

## Wrapper should delegate GetLogLevel to the inner appender

Validated by:

* `GetLogLevel_ShouldDelegateToInnerAppender`

## Wrapper should delegate IsEnabled to the inner appender

Validated by:

* `IsEnabled_ShouldDelegateToInnerAppender`

## Wrapper should delegate SetFormatter to the inner appender

Validated by:

* `SetFormatter_ShouldDelegateToInnerAppender`

## Wrapper should delegate GetFormatter to the inner appender

Validated by:

* `GetFormatter_ShouldDelegateToInnerAppender`

## Wrapper should return the inner appender's IsEnabled result

Validated by:

* `IsEnabled_ShouldReturnInnerAppenderResult`

## Wrapper should return the inner appender's GetLogLevel result

Validated by:

* `GetLogLevel_ShouldReturnInnerAppenderResult`

## Wrapper should return the inner appender's formatter

Validated by:

* `GetFormatter_ShouldReturnInnerAppenderFormatter`

These methods should not introduce independent state or alter the values returned by the wrapped appender.

# Worker Exception Handling Tests

## Worker should continue processing after an inner appender exception

Validated by:

* `InnerAppender_WhenAppendThrows_ShouldContinueProcessingNextMessage`

Verifies that an exception from one log write does not terminate the background worker.

Example:

```text
Message A
   ↓
Append throws
   ↓
Exception swallowed
   ↓
Worker continues
   ↓
Message B processed
```

## Worker should isolate logging failures from the producer thread

Validated by:

* `InnerAppender_WhenAppendThrows_ShouldNotPropagateToAppendCaller`

Because processing occurs on the worker thread, exceptions thrown by `InnerAppender.Append()` must not escape through the original `Append()` call.

## Worker should continue running after repeated inner appender failures

Validated by:

* `InnerAppender_WhenMultipleAppendsThrow_ShouldContinueWorkerProcessing`

Verifies that multiple consecutive failures do not terminate the worker.

# Shutdown Tests

## Stop should request worker shutdown

Validated by:

* `Stop_ShouldRequestWorkerShutdown`

## Stop should wake a worker waiting on an empty queue

Validated by:

* `Stop_WhenQueueIsEmpty_ShouldWakeWaitingWorker`

The worker must not remain blocked indefinitely inside `Monitor.Wait()`.

## Stop should wait for the worker to finish

Validated by:

* `Stop_ShouldWaitForWorkerToExit`

Verifies the blocking/join behavior of `Stop()`.

## Stop should process remaining queued messages before exiting

Validated by:

* `Stop_WithPendingMessages_ShouldProcessAllQueuedMessages`

This is one of the most important lifecycle tests.

The expected sequence is:

```text
Stop()
  ↓
_running = false
  ↓
Worker wakes
  ↓
Queue still contains messages
  ↓
Process remaining messages
  ↓
Queue becomes empty
  ↓
Worker exits
  ↓
Join() completes
```

## Stop should complete when the queue is already empty

Validated by:

* `Stop_WithEmptyQueue_ShouldCompleteSuccessfully`

## Stop should not terminate processing prematurely

Validated by:

* `Stop_WithPendingMessages_ShouldNotDropQueuedMessages`

Verifies that setting `_running = false` does not cause the worker to immediately exit while messages remain.

# Repeated Shutdown Tests

## Stop called multiple times should be evaluated for current behavior

Validated by:

* `Stop_CalledMultipleTimes_ShouldNotDeadlock`

> **Note:** The current implementation does not explicitly track whether `Stop()` has already been called. This test should document the current lifecycle behavior and expose any problematic repeated-shutdown behavior.

## Append after Stop should be evaluated explicitly

Validated by:

* `Append_AfterStop_ShouldFollowDefinedLifecycleBehavior`

> **Note:** The current implementation does not reject calls to `Append()` after `Stop()`. Such messages can still be queued even though the worker has exited. This is an important lifecycle edge case and should be documented by the test.

A future implementation may choose to explicitly reject or ignore appends after shutdown.

# Concurrent Producer Tests

## Multiple producer threads should safely enqueue messages

Validated by:

* `Append_FromMultipleThreads_ShouldNotLoseUnexpectedMessages`

Verifies that concurrent calls to `Append()` do not corrupt the queue.

## Concurrent producers should not cause collection corruption

Validated by:

* `Append_Concurrently_ShouldNotThrowCollectionExceptions`

The internal `Queue<T>` is protected by `_lock`; callers should be able to concurrently enqueue messages safely.

## Concurrent producers should eventually be processed by the worker

Validated by:

* `Append_Concurrently_ShouldEventuallyProcessQueuedMessages`

## FIFO ordering should be preserved for messages from a single producer

Validated by:

* `Append_FromSingleProducer_ShouldPreserveProducerOrder`

> **Note:** With multiple concurrent producers, there is no globally meaningful ordering guarantee based solely on the wall-clock order of calls. Tests should therefore avoid assuming an ordering between independently concurrent producers unless synchronization explicitly establishes one.

# Worker Lifecycle Tests

## Worker should wait efficiently when no messages are available

Validated by:

* `Worker_WithEmptyQueue_ShouldRemainWaiting`

This validates observable lifecycle behavior rather than testing `Monitor.Wait()` itself.

## Worker should resume when a message becomes available

Validated by:

* `Worker_WhenMessageIsEnqueued_ShouldResumeProcessing`

## Worker should exit only after shutdown is requested and the queue is empty

Validated by:

* `Worker_ShouldExitOnlyWhenStoppedAndQueueIsEmpty`

This verifies the worker's core exit condition:

```csharp
if (_queue.Count == 0 && !_running)
{
    return;
}
```

# Test Scope

These tests validate the public behavior and concurrency contract of **AsyncAppenderWrapper**.

The following responsibilities are intentionally tested separately within the underlying **ILogAppender** implementations:

* Actual log writing
* File I/O
* Console output
* Formatting
* Log-level filtering implementation
* Appender-specific configuration
* Destination-specific error handling

The following are intentionally **not** tested as implementation details:

* `Queue<T>` internal implementation
* `Thread` implementation
* `Monitor` implementation
* CLR thread scheduling
* Exact worker thread execution timing

Tests should validate observable behavior rather than framework primitives.

# Implementation Notes

## Current backpressure policy

The implementation uses a fixed queue capacity:

```text
MaxQueueSize = 10,000
```

When the queue reaches capacity, the oldest message is discarded.

This should be treated as the **current contract** until configurable queue sizing and priority-aware dropping are implemented.

## Current shutdown semantics

`Stop()` is designed to:

1. Mark the worker as no longer running.
2. Wake the worker.
3. Allow remaining messages to drain.
4. Join the worker thread.

This makes shutdown a **draining shutdown**, rather than an immediate discard.

## Current post-shutdown behavior

The class currently does not maintain a separate `_stopped` state.

Therefore:

```text
Append()
   ↓
_running == false
   ↓
message can still enter queue
   ↓
worker is no longer processing
```

This behavior should be explicitly tested and documented because it may represent a future lifecycle bug.

# Coverage Summary

| Area                            | Covered |
| ------------------------------- | :-----: |
| Constructor validation          |    ✅    |
| Inner appender exposure         |    ✅    |
| Worker initialization           |    ✅    |
| Null message handling           |    ✅    |
| Asynchronous processing         |    ✅    |
| FIFO processing                 |    ✅    |
| Queue buffering                 |    ✅    |
| Queue capacity                  |    ✅    |
| Backpressure                    |    ✅    |
| Oldest-message eviction         |    ✅    |
| Log-level delegation            |    ✅    |
| Formatter delegation            |    ✅    |
| Enabled-state delegation        |    ✅    |
| Worker exception isolation      |    ✅    |
| Exception recovery              |    ✅    |
| Graceful shutdown               |    ✅    |
| Queue draining                  |    ✅    |
| Worker wake-up                  |    ✅    |
| Concurrent producers            |    ✅    |
| Post-shutdown behavior          |    ⚠️   |
| Repeated Stop behavior          |    ⚠️   |
| Configurable queue size         |    ❌*   |
| Priority-based message dropping |    ❌*   |

* Not currently supported by the implementation.

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>