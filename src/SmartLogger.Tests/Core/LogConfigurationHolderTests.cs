using SmartLogger.Core;
using System.Collections.Generic;

namespace SmartLogger.Tests.Core
{
    [TestFixture]
    public class LogConfigurationHolderTests
    {
        [Test]
        public void Constructor_ShouldInitializeDefaultConfiguration()
        {
            // Arrange
            var configuration = new LogConfigurationHolder();

            // Act & Assert
            Assert.That(configuration.RootLogLevel, Is.EqualTo(LogLevel.INFO));
            Assert.That(configuration.LoggerOverrides, Is.Not.Null);
            Assert.That(configuration.Appenders, Is.Not.Null);
            Assert.That(configuration.EnableAsyncLoggingProcess, Is.False);
        }

        [Test]
        public void EnableDefaultConsoleAppender_WithEmptyAppenderCollection_ShouldReturnTrue()
        {
            // Arrange
            var configuration = new LogConfigurationHolder();

            // Act & Assert
            Assert.That(configuration.EnableDefaultConsoleAppender, Is.True);
        }

        [Test]
        public void EnableDefaultConsoleAppender_WithNullAppenderCollection_ShouldReturnTrue()
        {
            // Arrange
            var configuration = new LogConfigurationHolder { Appenders = null! };

            // Act & Assert
            Assert.That(configuration.EnableDefaultConsoleAppender, Is.True);
        }

        [Test]
        public void EnableDefaultConsoleAppender_WithConfiguredAppender_ShouldReturnFalse()
        {
            // Arrange
            var configuration = new LogConfigurationHolder();
            configuration.Appenders.Add(new AppenderConfiguration());

            // Act & Assert
            Assert.That(configuration.EnableDefaultConsoleAppender, Is.False);
        }

        [Test]
        public void EnableAsyncLoggingProcess_WhenEnabled_ShouldReturnTrue()
        {
            // Arrange
            var configuration = new LogConfigurationHolder();

            // Act
            configuration.EnableAsyncLoggingProcess = true;

            // Assert
            Assert.That(configuration.EnableAsyncLoggingProcess, Is.True);
        }

        [Test]
        public void LoggerOverrides_WithConfiguredOverrides_ShouldContainConfiguredEntries()
        {
            // Arrange
            var configuration = new LogConfigurationHolder();
            var overrideConfiguration = new LoggerOverrideConfiguration { LoggerName = "My.Logger", LogLevel = LogLevel.DEBUG };

            // Act
            configuration.LoggerOverrides.Add(overrideConfiguration);

            // Assert
            Assert.That(configuration.LoggerOverrides, Has.Count.EqualTo(1));
            Assert.That(configuration.LoggerOverrides[0], Is.SameAs(overrideConfiguration));
        }

        [Test]
        public void Appenders_WithMultipleConfiguredAppenders_ShouldContainAllConfiguredAppenders()
        {
            // Arrange
            var configuration = new LogConfigurationHolder();
            var firstAppender = new AppenderConfiguration();
            var secondAppender = new AppenderConfiguration();

            // Act
            configuration.Appenders.Add(firstAppender);
            configuration.Appenders.Add(secondAppender);

            // Assert
            Assert.That(configuration.Appenders, Has.Count.EqualTo(2));
            Assert.That(configuration.Appenders[0], Is.SameAs(firstAppender));
            Assert.That(configuration.Appenders[1], Is.SameAs(secondAppender));
        }

        [Test]
        public void Constructor_ShouldInitializeDefaultLoggerOverride()
        {
            // Arrange
            var overrideConfiguration = new LoggerOverrideConfiguration();

            // Act & Assert
            Assert.That(overrideConfiguration.LoggerName, Is.EqualTo(string.Empty));
            Assert.That(overrideConfiguration.LogLevel, Is.EqualTo(LogLevel.INFO));
        }

        [Test]
        public void LoggerOverride_WithCustomLoggerName_ShouldStoreValue()
        {
            // Arrange
            var overrideConfiguration = new LoggerOverrideConfiguration();

            // Act
            overrideConfiguration.LoggerName = "My.Logger";

            // Assert
            Assert.That(overrideConfiguration.LoggerName, Is.EqualTo("My.Logger"));
        }

        [Test]
        public void LoggerOverride_WithCustomLogLevel_ShouldStoreValue()
        {
            // Arrange
            var overrideConfiguration = new LoggerOverrideConfiguration();

            // Act
            overrideConfiguration.LogLevel = LogLevel.DEBUG;

            // Assert
            Assert.That(overrideConfiguration.LogLevel, Is.EqualTo(LogLevel.DEBUG));
        }

        [Test]
        public void Constructor_ShouldInitializeNestedConfigurations()
        {
            // Arrange
            var appenderConfiguration = new AppenderConfiguration();

            // Act & Assert
            Assert.That(appenderConfiguration.Destination, Is.Not.Null);
            Assert.That(appenderConfiguration.Formatter, Is.Not.Null);
            Assert.That(appenderConfiguration.Filter, Is.Null);
            Assert.That(appenderConfiguration.AppenderLogLevel, Is.Null);
        }

        [Test]
        public void Filter_WithCustomObject_ShouldStoreValue()
        {
            // Arrange
            var appenderConfiguration = new AppenderConfiguration();
            var filter = new object();

            // Act
            appenderConfiguration.Filter = filter;

            // Assert
            Assert.That(appenderConfiguration.Filter, Is.SameAs(filter));
        }

        [Test]
        public void AppenderLogLevel_WithConfiguredLevel_ShouldStoreValue()
        {
            // Arrange
            var appenderConfiguration = new AppenderConfiguration();

            // Act
            appenderConfiguration.AppenderLogLevel = LogLevel.WARNING;

            // Assert
            Assert.That(appenderConfiguration.AppenderLogLevel, Is.EqualTo(LogLevel.WARNING));
        }

        [Test]
        public void AppenderLogLevel_WithNullValue_ShouldRemainNull()
        {
            // Arrange
            var appenderConfiguration = new AppenderConfiguration();

            // Act & Assert
            Assert.That(appenderConfiguration.AppenderLogLevel, Is.Null);
        }

        [Test]
        public void Constructor_ShouldInitializeDefaultDestinationConfiguration()
        {
            // Arrange
            var destinationConfiguration = new DestinationConfiguration();

            // Act & Assert
            Assert.That(destinationConfiguration.Type, Is.EqualTo(LogOutputDestination.Console));
            Assert.That(destinationConfiguration.File, Is.Null);
            Assert.That(destinationConfiguration.Database, Is.Null);
        }

        [Test]
        public void Destination_WithFileConfiguration_ShouldStoreConfiguration()
        {
            // Arrange
            var destinationConfiguration = new DestinationConfiguration();
            var fileConfiguration = new FileConfiguration();

            // Act
            destinationConfiguration.File = fileConfiguration;

            // Assert
            Assert.That(destinationConfiguration.File, Is.SameAs(fileConfiguration));
        }

        [Test]
        public void Destination_WithDatabaseConfiguration_ShouldStoreConfiguration()
        {
            // Arrange
            var destinationConfiguration = new DestinationConfiguration();
            var database = new object();

            // Act
            destinationConfiguration.Database = database;

            // Assert
            Assert.That(destinationConfiguration.Database, Is.SameAs(database));
        }

        [Test]
        public void Constructor_ShouldInitializeDefaultFormatterConfiguration()
        {
            // Arrange
            var formatterConfiguration = new FormatterConfiguration();

            // Act & Assert
            Assert.That(formatterConfiguration.OutputFormat, Is.EqualTo(LogOutputFormat.PlainText));
            Assert.That(formatterConfiguration.LayoutType, Is.EqualTo(LogMessageLayoutType.Simple));
            Assert.That(formatterConfiguration.Pattern, Is.EqualTo(string.Empty));
            Assert.That(formatterConfiguration.IncludedJsonFields, Is.Not.Null);
            Assert.That(formatterConfiguration.JsonFieldMappings, Is.Not.Null);
        }

        [Test]
        public void IncludedJsonFields_ShouldContainExpectedDefaultFields()
        {
            // Arrange
            var formatterConfiguration = new FormatterConfiguration();

            // Act & Assert
            Assert.That(formatterConfiguration.IncludedJsonFields, Is.EqualTo(new[]
            {
                "timestamp",
                "level",
                "thread",
                "correlation",
                "source",
                "message"
            }));
        }

        [Test]
        public void OutputFormat_WithConfiguredValue_ShouldStoreValue()
        {
            // Arrange
            var formatterConfiguration = new FormatterConfiguration();

            // Act
            formatterConfiguration.OutputFormat = LogOutputFormat.Json;

            // Assert
            Assert.That(formatterConfiguration.OutputFormat, Is.EqualTo(LogOutputFormat.Json));
        }

        [Test]
        public void LayoutType_WithConfiguredValue_ShouldStoreValue()
        {
            // Arrange
            var formatterConfiguration = new FormatterConfiguration();

            // Act
            formatterConfiguration.LayoutType = LogMessageLayoutType.Custom;

            // Assert
            Assert.That(formatterConfiguration.LayoutType, Is.EqualTo(LogMessageLayoutType.Custom));
        }

        [Test]
        public void Pattern_WithCustomPattern_ShouldStoreValue()
        {
            // Arrange
            var formatterConfiguration = new FormatterConfiguration();

            // Act
            formatterConfiguration.Pattern = "%date [%level] %message";

            // Assert
            Assert.That(formatterConfiguration.Pattern, Is.EqualTo("%date [%level] %message"));
        }

        [Test]
        public void JsonFieldMappings_WithConfiguredMappings_ShouldContainConfiguredEntries()
        {
            // Arrange
            var formatterConfiguration = new FormatterConfiguration();
            var mapping = new JsonFieldMappingConfiguration { SourceField = "timestamp", TargetField = "ts" };

            // Act
            formatterConfiguration.JsonFieldMappings.Add(mapping);

            // Assert
            Assert.That(formatterConfiguration.JsonFieldMappings, Has.Count.EqualTo(1));
            Assert.That(formatterConfiguration.JsonFieldMappings[0], Is.SameAs(mapping));
        }

        [Test]
        public void Constructor_ShouldInitializeDefaultJsonFieldMapping()
        {
            // Arrange
            var mapping = new JsonFieldMappingConfiguration();

            // Act & Assert
            Assert.That(mapping.SourceField, Is.EqualTo(string.Empty));
            Assert.That(mapping.TargetField, Is.EqualTo(string.Empty));
        }

        [Test]
        public void JsonFieldMapping_WithCustomFields_ShouldStoreValues()
        {
            // Arrange
            var mapping = new JsonFieldMappingConfiguration();

            // Act
            mapping.SourceField = "timestamp";
            mapping.TargetField = "ts";

            // Assert
            Assert.That(mapping.SourceField, Is.EqualTo("timestamp"));
            Assert.That(mapping.TargetField, Is.EqualTo("ts"));
        }

        [Test]
        public void Constructor_ShouldInitializeDefaultFileConfiguration()
        {
            // Arrange
            var fileConfiguration = new FileConfiguration();

            // Act & Assert
            Assert.That(fileConfiguration.BasePath, Is.EqualTo("logs/app"));
            Assert.That(fileConfiguration.Extension, Is.EqualTo("log"));
            Assert.That(fileConfiguration.Naming, Is.Not.Null);
            Assert.That(fileConfiguration.RollingPolicy, Is.Not.Null);
        }

        [Test]
        public void BasePath_WithConfiguredValue_ShouldStoreValue()
        {
            // Arrange
            var fileConfiguration = new FileConfiguration();

            // Act
            fileConfiguration.BasePath = "logs/custom";

            // Assert
            Assert.That(fileConfiguration.BasePath, Is.EqualTo("logs/custom"));
        }

        [Test]
        public void Extension_WithConfiguredValue_ShouldStoreValue()
        {
            // Arrange
            var fileConfiguration = new FileConfiguration();

            // Act
            fileConfiguration.Extension = "txt";

            // Assert
            Assert.That(fileConfiguration.Extension, Is.EqualTo("txt"));
        }

        [Test]
        public void Constructor_ShouldInitializeDefaultNamingConfiguration()
        {
            // Arrange
            var namingConfiguration = new FileNamingConfiguration();

            // Act & Assert
            Assert.That(namingConfiguration.IncludeDate, Is.True);
            Assert.That(namingConfiguration.DateFormat, Is.EqualTo("yyyy-MM-dd"));
            Assert.That(namingConfiguration.IncludeIndex, Is.True);
            Assert.That(namingConfiguration.Separator, Is.EqualTo("-"));
        }

        [Test]
        public void FileNaming_WithCustomizedConfiguration_ShouldStoreValues()
        {
            // Arrange
            var namingConfiguration = new FileNamingConfiguration();

            // Act
            namingConfiguration.IncludeDate = false;
            namingConfiguration.DateFormat = "yyyyMMdd";
            namingConfiguration.IncludeIndex = false;
            namingConfiguration.Separator = "_";

            // Assert
            Assert.That(namingConfiguration.IncludeDate, Is.False);
            Assert.That(namingConfiguration.DateFormat, Is.EqualTo("yyyyMMdd"));
            Assert.That(namingConfiguration.IncludeIndex, Is.False);
            Assert.That(namingConfiguration.Separator, Is.EqualTo("_"));
        }

        [Test]
        public void Constructor_ShouldInitializeDefaultRollingPolicy()
        {
            // Arrange
            var rollingPolicy = new RollingPolicyConfiguration();

            // Act & Assert
            Assert.That(rollingPolicy.RollingType, Is.EqualTo(RollingType.None));
            Assert.That(rollingPolicy.MaxFileSizeMB, Is.EqualTo(10));
            Assert.That(rollingPolicy.Interval, Is.EqualTo(RollingInterval.None));
            Assert.That(rollingPolicy.MaxRetainedFiles, Is.EqualTo(7));
            Assert.That(rollingPolicy.DateFormat, Is.EqualTo("yyyy-MM-dd"));
        }

        [Test]
        public void RollingPolicy_WithSizeBasedConfiguration_ShouldStoreValues()
        {
            // Arrange
            var rollingPolicy = new RollingPolicyConfiguration();

            // Act
            rollingPolicy.RollingType = RollingType.Size;
            rollingPolicy.MaxFileSizeMB = 25;
            rollingPolicy.Interval = RollingInterval.Day;
            rollingPolicy.MaxRetainedFiles = 15;
            rollingPolicy.DateFormat = "yyyyMMdd";

            // Assert
            Assert.That(rollingPolicy.RollingType, Is.EqualTo(RollingType.Size));
            Assert.That(rollingPolicy.MaxFileSizeMB, Is.EqualTo(25));
            Assert.That(rollingPolicy.Interval, Is.EqualTo(RollingInterval.Day));
            Assert.That(rollingPolicy.MaxRetainedFiles, Is.EqualTo(15));
            Assert.That(rollingPolicy.DateFormat, Is.EqualTo("yyyyMMdd"));
        }

        [Test]
        public void RollingPolicy_WithTimeBasedConfiguration_ShouldStoreValues()
        {
            // Arrange
            var rollingPolicy = new RollingPolicyConfiguration();

            // Act
            rollingPolicy.RollingType = RollingType.Time;
            rollingPolicy.Interval = RollingInterval.Hour;
            rollingPolicy.MaxRetainedFiles = 30;

            // Assert
            Assert.That(rollingPolicy.RollingType, Is.EqualTo(RollingType.Time));
            Assert.That(rollingPolicy.Interval, Is.EqualTo(RollingInterval.Hour));
            Assert.That(rollingPolicy.MaxRetainedFiles, Is.EqualTo(30));
        }

        [Test]
        public void RollingPolicy_WithHybridConfiguration_ShouldStoreValues()
        {
            // Arrange
            var rollingPolicy = new RollingPolicyConfiguration();

            // Act
            rollingPolicy.RollingType = RollingType.Hybrid;
            rollingPolicy.MaxFileSizeMB = 50;
            rollingPolicy.Interval = RollingInterval.Month;
            rollingPolicy.MaxRetainedFiles = 100;

            // Assert
            Assert.That(rollingPolicy.RollingType, Is.EqualTo(RollingType.Hybrid));
            Assert.That(rollingPolicy.MaxFileSizeMB, Is.EqualTo(50));
            Assert.That(rollingPolicy.Interval, Is.EqualTo(RollingInterval.Month));
            Assert.That(rollingPolicy.MaxRetainedFiles, Is.EqualTo(100));
        }
    }
}
