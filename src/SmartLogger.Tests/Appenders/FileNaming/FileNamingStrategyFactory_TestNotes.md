# FileNamingStrategyFactory Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                                                              |
| ------- | ---------- | ------- | ------------- | -------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-09 | Srimani | Initial Draft | Defined the unit test coverage for the `FileNamingStrategyFactory` class, validating strategy selection, supported strategy creation, unsupported strategy handling, and invalid configuration behavior. |

# Objective

Validate that **FileNamingStrategyFactory** correctly creates the configured file naming strategy by:

* Selecting the appropriate strategy based on `FileNamingStrategyType`.
* Returning the expected concrete strategy implementation.
* Returning strategies through the `IFileNamingStrategy` abstraction.
* Rejecting currently unsupported naming strategies.
* Rejecting invalid strategy enum values.
* Handling invalid configuration input appropriately.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * Each test creates an independent `FileConfiguration`.
  * Factory behavior is tested independently from the internal naming logic of each concrete strategy.

# Strategy Creation Tests

## Factory should create a Date naming strategy for the Date strategy configuration

Validated by:

* `Create_WithDateStrategy_ShouldReturnDateFileNamingStrategy`

Verifies that:

```text
FileNamingStrategyType.Date
```

is mapped to:

```text
DateFileNamingStrategy
```

## Factory should return the created strategy through the IFileNamingStrategy abstraction

Validated by:

* `Create_WithDateStrategy_ShouldReturnFileNamingStrategy`

Ensures the concrete implementation satisfies the factory's public contract:

```csharp
IFileNamingStrategy
```

## Factory should create the strategy using the supplied configuration

Validated by:

* `Create_WithDateStrategy_ShouldUseConfiguredStrategy`

Verifies that the factory uses the supplied `FileConfiguration` when constructing the selected naming strategy.

The factory should not create a strategy using an unrelated or default configuration.

# Unsupported Strategy Tests

## Factory should reject the Timestamp strategy while it is not supported

Validated by:

* `Create_WithTimestampStrategy_ShouldThrowNotSupportedException`

The current implementation does not enable:

```csharp
FileNamingStrategyType.Timestamp
```

Therefore the factory should throw:

```text
NotSupportedException
```

with an error message identifying the unsupported strategy.

> **Note:** Once `TimestampFileNamingStrategy` is enabled in the factory, this test must be changed to validate successful strategy creation instead.

## Factory should reject the Custom strategy while it is not supported

Validated by:

* `Create_WithCustomStrategy_ShouldThrowNotSupportedException`

The current implementation does not provide a custom strategy implementation through the factory.

Therefore:

```text
FileNamingStrategyType.Custom
```

must result in `NotSupportedException`.

## Factory should reject unknown strategy enum values

Validated by:

* `Create_WithUnsupportedEnumValue_ShouldThrowNotSupportedException`

Verifies that an enum value not explicitly handled by the factory falls through to the default branch and produces `NotSupportedException`.

For example:

```csharp
(FileNamingStrategyType)999
```

should not silently select a default strategy.

# Configuration Validation Tests

## Factory should reject a null configuration

Validated by:

* `Create_WithNullConfiguration_ShouldThrowNullReferenceException`

The current implementation directly accesses:

```csharp
configuration.Naming.Strategy
```

Therefore a null configuration currently results in:

```text
NullReferenceException
```

> **Note:** This reflects the current implementation. A future enhancement could explicitly validate the argument and throw `ArgumentNullException`.

## Factory should reject a configuration with null Naming configuration

Validated by:

* `Create_WithNullNamingConfiguration_ShouldThrowNullReferenceException`

Verifies that the factory cannot determine the requested strategy when:

```csharp
configuration.Naming == null
```

> **Note:** This also reflects current implementation behavior and could be replaced with explicit argument/configuration validation in a future enhancement.

# Strategy Mapping Tests

## Factory should map each supported strategy to its corresponding implementation

Validated by:

* `Create_WithDateStrategy_ShouldReturnDateFileNamingStrategy`

The factory should maintain an explicit mapping between:

```text
FileNamingStrategyType
        ↓
Concrete IFileNamingStrategy
```

Currently supported mapping:

| Strategy    | Implementation                |    Status   |
| ----------- | ----------------------------- | :---------: |
| `Date`      | `DateFileNamingStrategy`      | ✅ Supported |
| `Timestamp` | `TimestampFileNamingStrategy` |   ⏳ Future  |
| `Custom`    | Application-supplied strategy |   ⏳ Future  |

# Test Scope

These tests validate only the responsibilities of **FileNamingStrategyFactory**.

The following responsibilities are intentionally tested separately within the concrete naming strategy implementations:

* Date-based file name generation
* Timestamp generation
* File name formatting
* File extension handling
* Rolling index formatting
* Filesystem-safe file names
* Directory/path handling
* Rolling decisions
* Archive processing
* Retention processing
* File system operations

The factory should only determine **which naming strategy to instantiate**.

# Coverage Summary

| Area                               | Covered |
| ---------------------------------- | :-----: |
| Strategy selection                 |    ✅    |
| Date strategy creation             |    ✅    |
| Interface contract                 |    ✅    |
| Configuration propagation          |    ✅    |
| Timestamp strategy rejection       |    ✅    |
| Custom strategy rejection          |    ✅    |
| Unknown enum handling              |    ✅    |
| Null configuration                 |    ✅    |
| Null naming configuration          |    ✅    |
| Unsupported strategy handling      |    ✅    |
| Strategy-to-implementation mapping |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>
