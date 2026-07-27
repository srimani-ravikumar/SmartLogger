# LogContext Unit Tests

## Document Information

Version | Date       | Author  | Status        | Description                                                                                                                                                                                            |
------- | ---------- | ------- | ------------- | ------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------ |
1.0.0   | 2026-07-11 | Srimani | Initial Draft | Defined the unit test coverage for the `LogContext` class, validating correlation scope management, nested scope behavior, async context propagation, execution flow isolation, and disposal behavior. |

# Objective

Validate that **LogContext** correctly provides lightweight execution-scoped logging context by:

* Managing the current correlation ID.
* Creating and restoring correlation scopes.
* Supporting nested scopes.
* Propagating context across asynchronous continuations.
* Isolating context between concurrent execution flows.
* Restoring previous context when scopes are disposed.
* Handling edge-case inputs consistently.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Not Required**
* Target Framework: **.NET**
* Test Isolation:

  * Every test must ensure that `LogContext.CorrelationId` is restored to its original value before completion.
  * Scope disposal is performed using `using` statements (or equivalent `Dispose()` calls) to avoid leaking execution context across tests.

# Initial State Tests

## Context should not contain a correlation ID before any scope is created

Validated by:

* `CorrelationId_WithoutActiveScope_ShouldBeNull`

Verifies the default state of the logging context.

# Correlation Scope Tests

## Context should expose the active correlation ID within a scope

Validated by:

* `BeginCorrelationScope_WithValidCorrelationId_ShouldSetCurrentCorrelationId`

## Context should restore the previous correlation ID when the scope is disposed

Validated by:

* `BeginCorrelationScope_Disposed_ShouldRestorePreviousCorrelationId`

Verifies that leaving the scope correctly restores the prior execution context.

## Context should support multiple sequential scopes

Validated by:

* `BeginCorrelationScope_WithSequentialScopes_ShouldRestoreEachScopeIndependently`

Ensures independently created scopes do not interfere with one another.

# Nested Scope Tests

## Context should support nested correlation scopes

Validated by:

* `BeginCorrelationScope_WithNestedScopes_ShouldUseInnermostCorrelationId`

## Context should restore the parent correlation ID after an inner scope is disposed

Validated by:

* `BeginCorrelationScope_DisposeInnerScope_ShouldRestoreOuterCorrelationId`

## Context should restore the original state after all nested scopes are disposed

Validated by:

* `BeginCorrelationScope_DisposeAllNestedScopes_ShouldRestoreInitialCorrelationId`

# Async Context Propagation Tests

## Context should flow across asynchronous continuations

Validated by:

* `BeginCorrelationScope_AcrossAwait_ShouldPreserveCorrelationId`

Verifies that `AsyncLocal<T>` propagates the correlation ID through `await` boundaries.

## Context should remain available inside asynchronous operations

Validated by:

* `BeginCorrelationScope_AsyncOperation_ShouldExposeCorrelationId`

# Concurrent Execution Tests

## Context should isolate correlation IDs between concurrent execution flows

Validated by:

* `BeginCorrelationScope_ConcurrentTasks_ShouldMaintainIndependentCorrelationIds`

Ensures parallel operations cannot overwrite each other's correlation context.

## Context should restore each execution flow independently after completion

Validated by:

* `BeginCorrelationScope_ConcurrentScopes_ShouldRestoreContextIndependently`

# Input Validation Tests

## Context should allow a null correlation ID

Validated by:

* `BeginCorrelationScope_WithNullCorrelationId_ShouldSetCorrelationIdToNull`

> **Note:** This reflects the current implementation. No argument validation is performed.

## Context should allow an empty correlation ID

Validated by:

* `BeginCorrelationScope_WithEmptyCorrelationId_ShouldSetCorrelationId`

## Context should allow a whitespace correlation ID

Validated by:

* `BeginCorrelationScope_WithWhitespaceCorrelationId_ShouldSetCorrelationId`

## Context should support Unicode correlation IDs

Validated by:

* `BeginCorrelationScope_WithUnicodeCorrelationId_ShouldSetCorrelationId`

## Context should support very long correlation IDs

Validated by:

* `BeginCorrelationScope_WithVeryLongCorrelationId_ShouldSetCorrelationId`

# Disposal Behavior Tests

## Disposing a scope should restore the previous correlation ID exactly once

Validated by:

* `Dispose_CalledOnce_ShouldRestorePreviousCorrelationId`

## Disposing nested scopes in reverse order should restore each previous context correctly

Validated by:

* `Dispose_WithNestedScopes_ShouldRestoreContextsInLifoOrder`

# Test Scope

These tests validate only the public behavior of **LogContext**.

The following implementation details are intentionally **not** verified:

* Internal implementation of `AsyncLocal<T>`
* Internal implementation of the private `Scope` class
* CLR execution-context mechanics
* Performance characteristics of `AsyncLocal<T>`
* Thread scheduling behavior provided by the .NET runtime

The tests focus exclusively on the observable behavior exposed through:

* `LogContext.CorrelationId`
* `LogContext.BeginCorrelationScope()`

# Coverage Summary

| Area                           | Covered |
| ------------------------------ | :-----: |
| Initial state                  |    ✅    |
| Correlation scope creation     |    ✅    |
| Scope disposal                 |    ✅    |
| Nested scopes                  |    ✅    |
| Sequential scopes              |    ✅    |
| Async context propagation      |    ✅    |
| Concurrent execution isolation |    ✅    |
| Input validation               |    ✅    |
| Unicode and long values        |    ✅    |
| Context restoration            |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>
