# RollingStrategyFactory Unit Tests

## Document Information

| Version | Date       | Author  | Status        | Description                                                                                                                                                                                                                              |
| ------- | ---------- | ------- | ------------- | ---------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------------- |
| 1.0.0   | 2026-09-09 | Srimani | Initial Draft | Defined the unit test coverage for the `RollingStrategyFactory` class, validating rolling strategy selection, supported strategy creation, configuration propagation, unsupported strategy handling, and invalid configuration behavior. |

# Objective

Validate that **RollingStrategyFactory** correctly creates the configured rolling strategy by:

* Selecting the appropriate strategy based on `RollingStrategyType`.
* Returning the expected concrete rolling strategy implementation.
* Returning strategies through the `IRollingStrategy` abstraction.
* Passing the supplied configuration to configuration-dependent strategies.
* Rejecting currently unsupported rolling strategies.
* Rejecting invalid rolling strategy enum values.
* Handling invalid configuration input appropriately.

# Test Environment

* Testing Framework: **NUnit**
* Mocking Framework: **Moq**
* Target Framework: **.NET**
* Test Isolation:

  * Each test creates an independent `FileConfiguration`.
  * Factory behavior is tested independently from the internal rolling logic of each concrete strategy.

# Strategy Creation Tests

## Factory should create a Daily rolling strategy for the Daily strategy configuration

Validated by:

* `Create_WithDailyStrategy_ShouldReturnDailyRollingStrategy`

Verifies that:

```text
RollingStrategyType.Daily
```

is mapped to:

```text
DailyRollingStrategy
```

## Factory should create a Size rolling strategy for the Size strategy configuration

Validated by:

* `Create_WithSizeStrategy_ShouldReturnSizeRollingStrategy`

Verifies that:

```text
RollingStrategyType.Size
```

is mapped to:

```text
SizeRollingStrategy
```

## Factory should return the created strategy through the IRollingStrategy abstraction

Validated by:

* `Create_WithDailyStrategy_ShouldReturnRollingStrategy`
* `Create_WithSizeStrategy_ShouldReturnRollingStrategy`

Ensures both concrete implementations satisfy the factory contract:

```csharp
IRollingStrategy
```

# Configuration Propagation Tests

## Factory should create the Size rolling strategy using the supplied configuration

Validated by:

* `Create_WithSizeStrategy_ShouldUseConfiguredConfiguration`

Verifies that the supplied `FileConfiguration` is passed to:

```csharp
new SizeRollingStrategy(configuration)
```

The factory must not create the Size strategy using a different or default configuration.

## Factory should not require rolling configuration for the Daily strategy implementation

Validated by:

* `Create_WithDailyStrategy_ShouldCreateStrategyWithoutAdditionalConfiguration`

Verifies that the Daily strategy is created independently because its current constructor does not require `FileConfiguration`.

# Unsupported Strategy Tests

## Factory should reject unsupported rolling strategy values

Validated by:

* `Create_WithUnsupportedStrategy_ShouldThrowNotSupportedException`

Verifies that a rolling strategy not explicitly handled by the factory results in:

```text
NotSupportedException
```

with an error message identifying the unsupported strategy.

## Factory should reject unknown rolling strategy enum values

Validated by:

* `Create_WithUnsupportedEnumValue_ShouldThrowNotSupportedException`

For example:

```csharp
(RollingStrategyType)999
```

should not silently select a default rolling strategy.

# Configuration Validation Tests

## Factory should reject a null configuration

Validated by:

* `Create_WithNullConfiguration_ShouldThrowNullReferenceException`

The current implementation directly accesses:

```csharp
configuration.Rolling.Strategy
```

Therefore a null configuration currently results in:

```text
NullReferenceException
```

> **Note:** This reflects the current implementation. A future enhancement could explicitly validate the argument and throw `ArgumentNullException`.

## Factory should reject a configuration with null Rolling configuration

Validated by:

* `Create_WithNullRollingConfiguration_ShouldThrowNullReferenceException`

Verifies that the factory cannot determine the requested strategy when:

```csharp
configuration.Rolling == null
```

> **Note:** This also reflects current implementation behavior and could be replaced with explicit configuration validation in a future enhancement.

# Strategy Mapping Tests

## Factory should maintain the expected mapping between rolling strategy types and implementations

Validated by:

* `Create_WithDailyStrategy_ShouldReturnDailyRollingStrategy`
* `Create_WithSizeStrategy_ShouldReturnSizeRollingStrategy`

The factory should maintain an explicit mapping:

| Strategy | Implementation         |    Status   |
| -------- | ---------------------- | :---------: |
| `Daily`  | `DailyRollingStrategy` | ✅ Supported |
| `Size`   | `SizeRollingStrategy`  | ✅ Supported |

Any future `RollingStrategyType` must be explicitly added to the factory rather than implicitly falling back to an existing strategy.

# Test Scope

These tests validate only the responsibilities of **RollingStrategyFactory**.

The following responsibilities are intentionally tested separately within the concrete rolling strategy implementations:

* Daily rolling decision logic
* Date/time boundary detection
* Size threshold evaluation
* File size calculation
* Maximum file size handling
* Rolling state management
* File system interaction
* File renaming
* Archive processing
* Retention processing
* Concurrent rolling behavior
* Thread safety

The factory should only determine **which rolling strategy to instantiate** and provide the required configuration.

# Coverage Summary

| Area                               | Covered |
| ---------------------------------- | :-----: |
| Strategy selection                 |    ✅    |
| Daily strategy creation            |    ✅    |
| Size strategy creation             |    ✅    |
| Interface contract                 |    ✅    |
| Configuration propagation          |    ✅    |
| Unsupported strategy handling      |    ✅    |
| Unknown enum handling              |    ✅    |
| Null configuration                 |    ✅    |
| Null rolling configuration         |    ✅    |
| Strategy-to-implementation mapping |    ✅    |

<p align="center"><strong>© 2026 Srimani. All rights reserved.</strong></p>
