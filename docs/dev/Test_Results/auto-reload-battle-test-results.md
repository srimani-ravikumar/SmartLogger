# Auto-Reload Battle Test Results

Tested 2026-10-03 against `FileConfigurationProviderBase` + `JsonConfigurationProvider` + `LoggerManager`/`LoggerFactory` soft-reload. Each scenario run standalone via `LoggerDemo.cs` + matching `scripts/*.ps1`.

| # | Scenario | Result | Notes |
|---|----------|--------|-------|
| 1 | Basic reload - `rootLogLevel` INFO ↔ DEBUG | ✅ Pass | DEBUG lines started appearing mid-run within ~1s of the file write, no restart. |
| 2 | Malformed JSON mid-edit | ✅ Pass | Reload attempt failed silently (caught in `OnConfigurationFileChanged`), logging never paused or crashed, old config stayed active. |
| 3 | Structurally valid, semantically invalid config (`FileSystem` dest. without `file`) | ✅ Pass | `ConfigurationValidator` rejected the reload; old config (Console/INFO) kept running the whole time, no DEBUG leaked through. |
| 4 | Rapid save storm (25 writes / 50ms) | ✅ Pass | No crash. A few transient DEBUG ticks flickered during the storm, but the final applied value matched the last write - debounce + lock held up. |
| 5 | Appender destination switch (Console ↔ FileSystem) at runtime | ✅ Pass | Console output paused exactly while destination was FileSystem; all ticks were safely found in the file sink (no logs lost, no duplicate handles). *(Note: `FileConfiguration.Directory` is resolved relative to the process's **current working directory**, not the app/config file location - worth double-checking if you expect it relative to the exe.)* |
| 6 | Config file deleted, then recreated while watched | ✅ Pass (fixed) | Originally a known gap - see below. After wiring `Created`/`Deleted`/`Renamed`, the recreate now reloads correctly (DEBUG ticks appeared immediately after recreate), with zero crashes across the delete window. |

## Fix applied for Scenario 6

`FileConfigurationProviderBase.EnableAutoReload` only wired `FileSystemWatcher.Changed`, so delete+recreate (and editor "save via temp file + rename" patterns) never triggered a reload.

Changes in [FileConfigurationProviderBase.cs](../../../src/SmartLogger/Configurations/FileConfigurationProviderBase.cs):
- Added `NotifyFilters.FileName` and wired `Created`, `Deleted`, `Renamed` alongside `Changed` to the same handler.
- Replaced the empty `catch { }` with `catch (Exception ex)` that emits `Trace.TraceWarning` (file path, change type, exception message) so rejected reloads are observable instead of fully silent.

Re-tested all 6 scenarios after the fix - all pass, no regressions.

## Remaining candidate for later

- Consider surfacing reload failures as a structured event/log (not just `Trace`) so apps without a trace listener still get visibility, e.g. `LoggerManager`-level `OnConfigurationReloadFailed` callback.
