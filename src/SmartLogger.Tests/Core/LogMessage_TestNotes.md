# LogMessage Unit Tests

## Document Information

Version | Date       | Author  | Status        | Description                                                                                                                                                           |
------- | ---------- | ------- | ------------- | --------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
1.0.0   | 2026-07-11 | Srimani | Initial Draft | Defined the unit test coverage for the `LogMessage` class, validating builder behavior, property initialization, default values, validation, and object construction. |

# Objective

Validate that **LogMessage** correctly represents an immutable log entry by:

* Constructing log messages through the fluent Builder.
* Initializing all properties correctly.
* Applying default values where appropriate.
* Validating required fields before construction.
* Preserving explicitly supplied values.
* Preventing invalid log message creation.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **None Required**
* Target Framework: **.NET**
* Test Isolation:

  * Each test creates a new `LogMessage.Builder` instance.
  * No shared or static state exists.

# Builder Initialization Tests

## Builder should initialize default values correctly

Validated by:

* `Builder_DefaultInitialization_ShouldSetExpectedDefaults`

Verifies that a newly created builder initializes:

* Timestamp to the current UTC time.
* Source to `"System"`.
* ThreadId to the current managed thread ID.
* Empty message.
* Null CorrelationId.
* No LogLevel.

## Builder should build a valid log message when all required fields are provided

Validated by:

* `Build_WithRequiredFields_ShouldCreateLogMessage`

Verifies that the builder successfully creates a `LogMessage` after specifying both:

* LogLevel
* Message

# Property Assignment Tests

## Builder should assign the specified log level

Validated by:

* `WithLevel_ShouldAssignLogLevel`

## Builder should assign the specified message

Validated by:

* `WithMessage_ShouldAssignMessage`

## Builder should assign the specified source

Validated by:

* `FromSource_ShouldAssignSource`

## Builder should assign the specified correlation ID

Validated by:

* `WithCorrelationId_ShouldAssignCorrelationId`

## Builder should preserve the generated timestamp

Validated by:

* `Build_ShouldPreserveTimestamp`

Ensures the timestamp assigned during builder creation is transferred unchanged to the constructed `LogMessage`.

## Builder should preserve the current managed thread ID

Validated by:

* `Build_ShouldPreserveThreadId`

Ensures the thread identifier captured during builder creation is stored in the final log message.

# Fluent Builder Tests

## Builder methods should support fluent chaining

Validated by:

* `Builder_ShouldSupportMethodChaining`

Verifies that all fluent methods return the same builder instance.

## Builder should correctly construct an object using chained methods

Validated by:

* `Builder_WithFluentCalls_ShouldCreateExpectedLogMessage`

Ensures every configured property is correctly transferred into the constructed object.

# Validation Tests

## Builder should reject construction when LogLevel is not specified

Validated by:

* `Build_WithoutLogLevel_ShouldThrowInvalidOperationException`

Verifies the current implementation throws:

```
InvalidOperationException
```

with the expected validation message.

## Builder should reject construction when message is null

Validated by:

* `Build_WithNullMessage_ShouldThrowInvalidOperationException`

## Builder should reject construction when message is empty

Validated by:

* `Build_WithEmptyMessage_ShouldThrowInvalidOperationException`

## Builder should reject construction when message contains only whitespace

Validated by:

* `Build_WithWhitespaceMessage_ShouldThrowInvalidOperationException`

These tests verify that only meaningful log messages may be created.

# Default Value Tests

## Builder should default the source to "System"

Validated by:

* `Build_WithoutSource_ShouldUseDefaultSource`

## Builder should default the correlation ID to null

Validated by:

* `Build_WithoutCorrelationId_ShouldUseNullCorrelationId`

## Builder should retain the automatically generated timestamp

Validated by:

* `Build_WithoutTimestampOverride_ShouldUseCurrentUtcTimestamp`

Verifies that the generated timestamp falls within the expected UTC time window during object construction.

## Builder should retain the automatically generated thread ID

Validated by:

* `Build_WithoutThreadOverride_ShouldUseCurrentManagedThreadId`

# Input Handling Tests

## Builder should allow Unicode messages

Validated by:

* `WithUnicodeMessage_ShouldBuildSuccessfully`

## Builder should allow Unicode source names

Validated by:

* `WithUnicodeSource_ShouldBuildSuccessfully`

## Builder should allow very long messages

Validated by:

* `WithVeryLongMessage_ShouldBuildSuccessfully`

## Builder should allow very long source names

Validated by:

* `WithVeryLongSource_ShouldBuildSuccessfully`

## Builder should allow null correlation IDs

Validated by:

* `WithNullCorrelationId_ShouldBuildSuccessfully`

## Builder should allow empty correlation IDs

Validated by:

* `WithEmptyCorrelationId_ShouldBuildSuccessfully`

The current implementation performs no validation on CorrelationId.

# Immutability Tests

## Constructed log messages should expose immutable property values

Validated by:

* `Build_ShouldCreateImmutableLogMessage`

Verifies that property values cannot be modified after construction.

## Builder modifications after Build should not affect previously created log messages

Validated by:

* `Builder_ModifiedAfterBuild_ShouldNotChangeExistingLogMessage`

Ensures each constructed `LogMessage` captures a snapshot of the builder state at build time.

# Test Scope

These tests validate only the public behavior of **LogMessage** and its nested **Builder**.

The following responsibilities are intentionally tested separately:

* Log formatting
* Log filtering
* Logger implementation
* Appender behavior
* Configuration resolution
* Logger factory behavior
* Log persistence

# Coverage Summary

| Area                          | Covered |
| ----------------------------- | :-----: |
| Builder initialization        |    ✅    |
| Fluent builder API            |    ✅    |
| Property assignment           |    ✅    |
| Default values                |    ✅    |
| Required field validation     |    ✅    |
| Exception handling            |    ✅    |
| Immutable object construction |    ✅    |
| Unicode input                 |    ✅    |
| Large input handling          |    ✅    |
| Thread metadata               |    ✅    |
| Timestamp generation          |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>
