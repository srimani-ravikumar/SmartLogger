using SmartLogger.Core;

namespace SmartLogger.Tests.Core
{
    [TestFixture]
    public class LogConfigurationHolderTests
    {
        [Test]
        public void Constructor_ShouldInitializeDefaultConfiguration()
        {
            // Act
            var config = new LogConfigurationHolder();

            // Assert
            Assert.That(config.RootLogLevel, Is.EqualTo(LogLevel.INFO));
            Assert.That(config.LoggerOverrides, Is.Not.Null);
            Assert.That(config.Appenders, Is.Not.Null);
            Assert.That(config.EnableDefaultConsoleAppender, Is.True);
            Assert.That(config.EnableAsyncLoggingProcess, Is.False);
        }

        [Test]
        public void EnableDefaultConsoleAppender_WithEmptyAppenderCollection_ShouldReturnTrue()
        {
            // Arrange
            var config = new LogConfigurationHolder { Appenders = new List<AppenderConfiguration>() };

            // Act / Assert
            Assert.That(config.EnableDefaultConsoleAppender, Is.True);
        }

        [Test]
        public void EnableDefaultConsoleAppender_WithNullAppenderCollection_ShouldReturnTrue()
        {
            // Arrange
            var config = new LogConfigurationHolder { Appenders = null };

            // Act / Assert
            Assert.That(config.EnableDefaultConsoleAppender, Is.True);
        }

        [Test]
        public void EnableDefaultConsoleAppender_WithConfiguredAppender_ShouldReturnFalse()
        {
            // Arrange
            var config = new LogConfigurationHolder();
            config.Appenders.Add(new AppenderConfiguration());

            // Act / Assert
            Assert.That(config.EnableDefaultConsoleAppender, Is.False);
        }

        [Test]
        public void EnableAsyncLoggingProcess_WhenEnabled_ShouldReturnTrue()
        {
            // Arrange
            var config = new LogConfigurationHolder { EnableAsyncLoggingProcess = true };

            // Act / Assert
            Assert.That(config.EnableAsyncLoggingProcess, Is.True);
        }

        [Test]
        public void EnableAsyncLoggingProcess_WhenNotConfigured_ShouldReturnFalse()
        {
            // Arrange
            var config = new LogConfigurationHolder();

            // Act / Assert
            Assert.That(config.EnableAsyncLoggingProcess, Is.False);
        }

        [Test]
        public void LoggerOverrides_WithNoConfiguredOverrides_ShouldBeEmpty()
        {
            // Arrange
            var config = new LogConfigurationHolder();

            // Act / Assert
            Assert.That(config.LoggerOverrides, Is.Empty);
        }

        [Test]
        public void LoggerOverrides_WithConfiguredOverrides_ShouldContainConfiguredEntries()
        {
            // Arrange
            var config = new LogConfigurationHolder();
            config.LoggerOverrides.Add(new LoggerOverrideConfiguration { LoggerName = "My.Logger", LogLevel = LogLevel.DEBUG });

            // Act / Assert
            Assert.That(config.LoggerOverrides, Has.Count.EqualTo(1));
            Assert.That(config.LoggerOverrides[0].LoggerName, Is.EqualTo("My.Logger"));
        }

        [Test]
        public void Appenders_WithMultipleConfiguredAppenders_ShouldContainAllConfiguredAppenders()
        {
            // Arrange
            var config = new LogConfigurationHolder();
            config.Appenders.Add(new AppenderConfiguration());
            config.Appenders.Add(new AppenderConfiguration());

            // Act / Assert
            Assert.That(config.Appenders, Has.Count.EqualTo(2));
        }

        [Test]
        public void LoggerOverrides_WhenReassigned_ShouldReplaceExistingCollection()
        {
            // Arrange
            var config = new LogConfigurationHolder();
            config.LoggerOverrides.Add(new LoggerOverrideConfiguration());

            // Act
            config.LoggerOverrides = new List<LoggerOverrideConfiguration>
            {
                new LoggerOverrideConfiguration { LoggerName = "Replaced" }
            };

            // Assert
            Assert.That(config.LoggerOverrides, Has.Count.EqualTo(1));
            Assert.That(config.LoggerOverrides[0].LoggerName, Is.EqualTo("Replaced"));
        }

        [Test]
        public void Appenders_WhenReassigned_ShouldReplaceExistingCollection()
        {
            // Arrange
            var config = new LogConfigurationHolder();
            config.Appenders.Add(new AppenderConfiguration());

            // Act
            config.Appenders = new List<AppenderConfiguration>();

            // Assert
            Assert.That(config.Appenders, Is.Empty);
            Assert.That(config.EnableDefaultConsoleAppender, Is.True);
        }

        [Test]
        public void Constructor_ShouldInitializeDefaultLoggerOverride()
        {
            // Act
            var ov = new LoggerOverrideConfiguration();

            // Assert
            Assert.That(ov.LoggerName, Is.EqualTo(string.Empty));
            Assert.That(ov.LogLevel, Is.EqualTo(LogLevel.INFO));
        }

        [Test]
        public void LoggerOverride_WithCustomLoggerName_ShouldStoreValue()
        {
            // Arrange
            var ov = new LoggerOverrideConfiguration { LoggerName = "Custom" };

            // Act / Assert
            Assert.That(ov.LoggerName, Is.EqualTo("Custom"));
        }

        [Test]
        public void LoggerOverride_WithCustomLogLevel_ShouldStoreValue()
        {
            // Arrange
            var ov = new LoggerOverrideConfiguration { LogLevel = LogLevel.ERROR };

            // Act / Assert
            Assert.That(ov.LogLevel, Is.EqualTo(LogLevel.ERROR));
        }

        [Test]
        public void LoggerOverride_WithoutLoggerName_ShouldRemainEmpty()
        {
            // Arrange
            var ov = new LoggerOverrideConfiguration { LogLevel = LogLevel.WARNING };

            // Act / Assert
            Assert.That(ov.LoggerName, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Constructor_ShouldInitializeNestedConfigurations()
        {
            // Act
            var app = new AppenderConfiguration();

            // Assert
            Assert.That(app.Destination, Is.Not.Null);
            Assert.That(app.Formatter, Is.Not.Null);
            Assert.That(app.Filter, Is.Null);
            Assert.That(app.AppenderLogLevel, Is.Null);
        }

        [Test]
        public void Filter_WithCustomObject_ShouldStoreValue()
        {
            // Arrange
            var app = new AppenderConfiguration();
            var filter = new object();
            app.Filter = filter;

            // Act / Assert
            Assert.That(app.Filter, Is.SameAs(filter));
        }

        [Test]
        public void AppenderLogLevel_WithConfiguredLevel_ShouldStoreValue()
        {
            // Arrange
            var app = new AppenderConfiguration { AppenderLogLevel = LogLevel.DEBUG };

            // Act / Assert
            Assert.That(app.AppenderLogLevel, Is.EqualTo(LogLevel.DEBUG));
        }

        [Test]
        public void AppenderLogLevel_WithNullValue_ShouldRemainNull()
        {
            // Arrange
            var app = new AppenderConfiguration { AppenderLogLevel = null };

            // Act / Assert
            Assert.That(app.AppenderLogLevel, Is.Null);
        }

        [Test]
        public void Constructor_ShouldInitializeDefaultDestinationConfiguration()
        {
            // Act
            var dest = new DestinationConfiguration();

            // Assert
            Assert.That(dest.Type, Is.EqualTo(LogOutputDestination.Console));
            Assert.That(dest.File, Is.Null);
        }

        [Test]
        public void Destination_WithFileConfiguration_ShouldStoreConfiguration()
        {
            // Arrange
            var dest = new DestinationConfiguration { File = new FileConfiguration { Directory = "X" } };

            // Act / Assert
            Assert.That(dest.File, Is.Not.Null);
            Assert.That(dest.File.Directory, Is.EqualTo("X"));
        }

        [Test]
        public void Destination_WithConsoleType_ShouldLeaveFileConfigurationNull()
        {
            // Arrange
            var dest = new DestinationConfiguration { Type = LogOutputDestination.Console };

            // Act / Assert
            Assert.That(dest.File, Is.Null);
        }

        [Test]
        public void Destination_WithUnknownType_ShouldStoreValue()
        {
            // Arrange
            var dest = new DestinationConfiguration { Type = LogOutputDestination.Unknown };

            // Act / Assert
            Assert.That(dest.Type, Is.EqualTo(LogOutputDestination.Unknown));
        }

        [Test]
        public void Constructor_ShouldInitializeDefaultFileConfiguration()
        {
            // Act
            var file = new FileConfiguration();

            // Assert
            Assert.That(file.Directory, Is.EqualTo("Logs"));
            Assert.That(file.FileName, Is.EqualTo("Application"));
            Assert.That(file.Extension, Is.EqualTo("log"));
            Assert.That(file.Naming, Is.Not.Null);
            Assert.That(file.Rolling, Is.Not.Null);
            Assert.That(file.Archive, Is.Not.Null);
            Assert.That(file.Retention, Is.Not.Null);
        }

        [Test]
        public void Directory_WithConfiguredValue_ShouldStoreValue()
        {
            // Arrange
            var file = new FileConfiguration { Directory = "C:\\Logs" };

            // Act / Assert
            Assert.That(file.Directory, Is.EqualTo("C:\\Logs"));
        }

        [Test]
        public void FileName_WithConfiguredValue_ShouldStoreValue()
        {
            // Arrange
            var file = new FileConfiguration { FileName = "Payments" };

            // Act / Assert
            Assert.That(file.FileName, Is.EqualTo("Payments"));
        }

        [Test]
        public void Extension_WithConfiguredValue_ShouldStoreValue()
        {
            // Arrange
            var file = new FileConfiguration { Extension = "json" };

            // Act / Assert
            Assert.That(file.Extension, Is.EqualTo("json"));
        }

        [Test]
        public void Constructor_ShouldInitializeDefaultNamingConfiguration()
        {
            // Act
            var naming = new FileNamingConfiguration();

            // Assert
            Assert.That(naming.Strategy, Is.EqualTo(FileNamingStrategyType.Date));
            Assert.That(naming.DateFormat, Is.EqualTo("yyyy-MM-dd"));
        }

        [Test]
        public void FileNaming_WithCustomizedConfiguration_ShouldStoreValues()
        {
            // Arrange
            var naming = new FileNamingConfiguration
            {
                Strategy = FileNamingStrategyType.Timestamp,
                DateFormat = "yyyyMMddHHmmss"
            };

            // Act / Assert
            Assert.That(naming.Strategy, Is.EqualTo(FileNamingStrategyType.Timestamp));
            Assert.That(naming.DateFormat, Is.EqualTo("yyyyMMddHHmmss"));
        }

        [Test]
        public void FileNaming_WithCustomStrategy_ShouldIgnoreDateFormatRequirement()
        {
            // Arrange
            var naming = new FileNamingConfiguration
            {
                Strategy = FileNamingStrategyType.Custom,
                DateFormat = string.Empty
            };

            // Act / Assert
            Assert.That(naming.Strategy, Is.EqualTo(FileNamingStrategyType.Custom));
            Assert.That(naming.DateFormat, Is.EqualTo(string.Empty));
        }

        [Test]
        public void Constructor_ShouldInitializeDefaultRollingConfiguration()
        {
            // Act
            var rolling = new FileRollingConfiguration();

            // Assert
            Assert.That(rolling.Strategy, Is.EqualTo(RollingStrategyType.Daily));
            Assert.That(rolling.MaxFileSizeMB, Is.EqualTo(10));
        }

        [Test]
        public void RollingConfiguration_WithSizeStrategy_ShouldStoreValues()
        {
            // Arrange
            var rolling = new FileRollingConfiguration
            {
                Strategy = RollingStrategyType.Size,
                MaxFileSizeMB = 50
            };

            // Act / Assert
            Assert.That(rolling.Strategy, Is.EqualTo(RollingStrategyType.Size));
            Assert.That(rolling.MaxFileSizeMB, Is.EqualTo(50));
        }

        [Test]
        public void RollingConfiguration_WithDailyStrategy_ShouldRetainDefaultMaxFileSize()
        {
            // Arrange
            var rolling = new FileRollingConfiguration { Strategy = RollingStrategyType.Daily };

            // Act / Assert
            Assert.That(rolling.MaxFileSizeMB, Is.EqualTo(10));
        }

        [Test]
        public void Constructor_ShouldInitializeDefaultArchiveConfiguration()
        {
            // Act
            var archive = new ArchiveConfiguration();

            // Assert
            Assert.That(archive.Enabled, Is.True);
            Assert.That(archive.Directory, Is.EqualTo("Archive"));
            Assert.That(archive.Compress, Is.True);
        }

        [Test]
        public void ArchiveConfiguration_WithCustomizedValues_ShouldStoreValues()
        {
            // Arrange
            var archive = new ArchiveConfiguration
            {
                Directory = "Old",
                Compress = false
            };

            // Act / Assert
            Assert.That(archive.Directory, Is.EqualTo("Old"));
            Assert.That(archive.Compress, Is.False);
        }

        [Test]
        public void ArchiveConfiguration_WhenDisabled_ShouldReturnFalse()
        {
            // Arrange
            var archive = new ArchiveConfiguration { Enabled = false };

            // Act / Assert
            Assert.That(archive.Enabled, Is.False);
        }

        [Test]
        public void Constructor_ShouldInitializeDefaultRetentionConfiguration()
        {
            // Act
            var retention = new RetentionConfiguration();

            // Assert
            Assert.That(retention.RetentionDays, Is.EqualTo(30));
        }

        [Test]
        public void RetentionConfiguration_WithConfiguredRetentionDays_ShouldStoreValue()
        {
            // Arrange
            var retention = new RetentionConfiguration { RetentionDays = 7 };

            // Act / Assert
            Assert.That(retention.RetentionDays, Is.EqualTo(7));
        }

        [Test]
        public void Constructor_ShouldInitializeDefaultFormatterConfiguration()
        {
            // Act
            var formatter = new FormatterConfiguration();

            // Assert
            Assert.That(formatter.OutputFormat, Is.EqualTo(LogOutputFormat.PlainText));
            Assert.That(formatter.LayoutType, Is.EqualTo(LogMessageLayoutType.Simple));
            Assert.That(formatter.Pattern, Is.EqualTo(string.Empty));
            Assert.That(formatter.IncludedJsonFields, Is.Not.Null);
            Assert.That(formatter.JsonFieldMappings, Is.Empty);
        }

        [Test]
        public void IncludedJsonFields_ShouldContainExpectedDefaultFields()
        {
            // Act
            var formatter = new FormatterConfiguration();

            // Assert
            Assert.That(
                formatter.IncludedJsonFields,
                Is.EqualTo(new[] { "timestamp", "level", "thread", "correlation", "source", "message" }));
        }

        [Test]
        public void OutputFormat_WithConfiguredValue_ShouldStoreValue()
        {
            // Arrange
            var formatter = new FormatterConfiguration { OutputFormat = LogOutputFormat.Json };

            // Act / Assert
            Assert.That(formatter.OutputFormat, Is.EqualTo(LogOutputFormat.Json));
        }

        [Test]
        public void LayoutType_WithConfiguredValue_ShouldStoreValue()
        {
            // Arrange
            var formatter = new FormatterConfiguration { LayoutType = LogMessageLayoutType.Detailed };

            // Act / Assert
            Assert.That(formatter.LayoutType, Is.EqualTo(LogMessageLayoutType.Detailed));
        }

        [Test]
        public void Pattern_WithCustomPattern_ShouldStoreValue()
        {
            // Arrange
            var formatter = new FormatterConfiguration
            {
                LayoutType = LogMessageLayoutType.Custom,
                Pattern = "%date [%level] %message"
            };

            // Act / Assert
            Assert.That(formatter.Pattern, Is.EqualTo("%date [%level] %message"));
        }

        [Test]
        public void Pattern_WithSimpleLayout_ShouldRemainEmpty()
        {
            // Arrange
            var formatter = new FormatterConfiguration { LayoutType = LogMessageLayoutType.Simple };

            // Act / Assert
            Assert.That(formatter.Pattern, Is.EqualTo(string.Empty));
        }

        [Test]
        public void JsonFieldMappings_WithConfiguredMappings_ShouldContainConfiguredEntries()
        {
            // Arrange
            var formatter = new FormatterConfiguration();
            formatter.JsonFieldMappings.Add(new JsonFieldMappingConfiguration
            {
                SourceField = "timestamp",
                TargetField = "ts"
            });

            // Act / Assert
            Assert.That(formatter.JsonFieldMappings, Has.Count.EqualTo(1));
            Assert.That(formatter.JsonFieldMappings[0].TargetField, Is.EqualTo("ts"));
        }

        [Test]
        public void IncludedJsonFields_WhenReassigned_ShouldContainOnlyConfiguredFields()
        {
            // Arrange
            var formatter = new FormatterConfiguration
            {
                IncludedJsonFields = new List<string> { "level", "message" }
            };

            // Act / Assert
            Assert.That(formatter.IncludedJsonFields, Is.EqualTo(new[] { "level", "message" }));
        }

        [Test]
        public void IncludedJsonFields_WhenCleared_ShouldBeEmpty()
        {
            // Arrange
            var formatter = new FormatterConfiguration();

            // Act
            formatter.IncludedJsonFields.Clear();

            // Assert
            Assert.That(formatter.IncludedJsonFields, Is.Empty);
        }

        [Test]
        public void Constructor_ShouldInitializeDefaultJsonFieldMapping()
        {
            // Act
            var mapping = new JsonFieldMappingConfiguration();

            // Assert
            Assert.That(mapping.SourceField, Is.EqualTo(string.Empty));
            Assert.That(mapping.TargetField, Is.EqualTo(string.Empty));
        }

        [Test]
        public void JsonFieldMapping_WithCustomFields_ShouldStoreValues()
        {
            // Arrange
            var mapping = new JsonFieldMappingConfiguration
            {
                SourceField = "correlation",
                TargetField = "cid"
            };

            // Act / Assert
            Assert.That(mapping.SourceField, Is.EqualTo("correlation"));
            Assert.That(mapping.TargetField, Is.EqualTo("cid"));
        }

        [Test]
        public void LogOutputDestination_ShouldContainExpectedValues()
        {
            Assert.That(Enum.IsDefined(typeof(LogOutputDestination), LogOutputDestination.Unknown), Is.True);
            Assert.That(Enum.IsDefined(typeof(LogOutputDestination), LogOutputDestination.Console), Is.True);
            Assert.That(Enum.IsDefined(typeof(LogOutputDestination), LogOutputDestination.FileSystem), Is.True);
            Assert.That(Enum.IsDefined(typeof(LogOutputDestination), LogOutputDestination.DatabaseSystem), Is.True);
        }

        [Test]
        public void LogOutputFormat_ShouldContainExpectedValues()
        {
            Assert.That(Enum.IsDefined(typeof(LogOutputFormat), LogOutputFormat.PlainText), Is.True);
            Assert.That(Enum.IsDefined(typeof(LogOutputFormat), LogOutputFormat.Json), Is.True);
            Assert.That(Enum.IsDefined(typeof(LogOutputFormat), LogOutputFormat.Xml), Is.True);
            Assert.That(Enum.IsDefined(typeof(LogOutputFormat), (LogOutputFormat)2), Is.False);
        }

        [Test]
        public void LogMessageLayoutType_ShouldContainExpectedValues()
        {
            Assert.That(Enum.IsDefined(typeof(LogMessageLayoutType), LogMessageLayoutType.Simple), Is.True);
            Assert.That(Enum.IsDefined(typeof(LogMessageLayoutType), LogMessageLayoutType.Detailed), Is.True);
            Assert.That(Enum.IsDefined(typeof(LogMessageLayoutType), LogMessageLayoutType.Custom), Is.True);
        }

        [Test]
        public void FileNamingStrategyType_ShouldContainExpectedValues()
        {
            Assert.That(Enum.IsDefined(typeof(FileNamingStrategyType), FileNamingStrategyType.Date), Is.True);
            Assert.That(Enum.IsDefined(typeof(FileNamingStrategyType), FileNamingStrategyType.Timestamp), Is.True);
            Assert.That(Enum.IsDefined(typeof(FileNamingStrategyType), FileNamingStrategyType.Custom), Is.True);
        }

        [Test]
        public void RollingStrategyType_ShouldContainExpectedValues()
        {
            Assert.That(Enum.IsDefined(typeof(RollingStrategyType), RollingStrategyType.Daily), Is.True);
            Assert.That(Enum.IsDefined(typeof(RollingStrategyType), RollingStrategyType.Size), Is.True);
        }
    }
}
