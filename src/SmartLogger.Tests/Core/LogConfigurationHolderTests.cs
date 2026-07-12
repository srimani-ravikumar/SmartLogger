using System;
using System.Collections.Generic;
using NUnit.Framework;
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
        public void Constructor_ShouldInitializeNestedAppenderConfigurations()
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
        public void DestinationConfiguration_ShouldInitializeDefaults()
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
        public void FileConfiguration_ShouldInitializeDefaults()
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
        public void FileNamingConfiguration_DefaultsAndCustom()
        {
            var n = new FileNamingConfiguration();
            Assert.That(n.Strategy, Is.EqualTo(FileNamingStrategyType.Date));
            Assert.That(n.DateFormat, Is.EqualTo("yyyy-MM-dd"));

            n.Strategy = FileNamingStrategyType.Timestamp;
            n.DateFormat = "yyyyMMddHHmmss";
            Assert.That(n.Strategy, Is.EqualTo(FileNamingStrategyType.Timestamp));
            Assert.That(n.DateFormat, Is.EqualTo("yyyyMMddHHmmss"));
        }

        [Test]
        public void FileRollingConfiguration_DefaultsAndCustom()
        {
            var r = new FileRollingConfiguration();
            Assert.That(r.Strategy, Is.EqualTo(RollingStrategyType.Daily));
            Assert.That(r.MaxFileSizeMB, Is.EqualTo(10));

            r.Strategy = RollingStrategyType.Size;
            r.MaxFileSizeMB = 50;
            Assert.That(r.Strategy, Is.EqualTo(RollingStrategyType.Size));
            Assert.That(r.MaxFileSizeMB, Is.EqualTo(50));
        }

        [Test]
        public void ArchiveConfiguration_DefaultsAndCustom()
        {
            var a = new ArchiveConfiguration();
            Assert.That(a.Enabled, Is.True);
            Assert.That(a.Directory, Is.EqualTo("Archive"));
            Assert.That(a.Compress, Is.True);

            a.Enabled = false;
            a.Directory = "Old";
            a.Compress = false;
            Assert.That(a.Enabled, Is.False);
            Assert.That(a.Directory, Is.EqualTo("Old"));
            Assert.That(a.Compress, Is.False);
        }

        [Test]
        public void RetentionConfiguration_DefaultsAndCustom()
        {
            var t = new RetentionConfiguration();
            Assert.That(t.RetentionDays, Is.EqualTo(30));

            t.RetentionDays = 7;
            Assert.That(t.RetentionDays, Is.EqualTo(7));
        }

        [Test]
        public void FormatterConfiguration_Defaults()
        {
            var f = new FormatterConfiguration();
            Assert.That(f.OutputFormat, Is.EqualTo(LogOutputFormat.PlainText));
            Assert.That(f.LayoutType, Is.EqualTo(LogMessageLayoutType.Simple));
            Assert.That(f.Pattern, Is.EqualTo(string.Empty));
            CollectionAssert.AreEqual(new[] { "timestamp", "level", "thread", "correlation", "source", "message" }, f.IncludedJsonFields);
            Assert.That(f.JsonFieldMappings, Is.Not.Null);
            Assert.That(f.JsonFieldMappings, Is.Empty);
        }

        [Test]
        public void JsonFieldMappingConfiguration_DefaultsAndCustom()
        {
            var m = new JsonFieldMappingConfiguration();
            Assert.That(m.SourceField, Is.EqualTo(string.Empty));
            Assert.That(m.TargetField, Is.EqualTo(string.Empty));

            m.SourceField = "timestamp";
            m.TargetField = "ts";
            Assert.That(m.SourceField, Is.EqualTo("timestamp"));
            Assert.That(m.TargetField, Is.EqualTo("ts"));
        }

        [Test]
        public void Enumeration_Definitions_ArePresent()
        {
            Assert.That(Enum.IsDefined(typeof(LogOutputDestination), LogOutputDestination.Console));
            Assert.That(Enum.IsDefined(typeof(LogOutputFormat), LogOutputFormat.PlainText));
            Assert.That(Enum.IsDefined(typeof(LogMessageLayoutType), LogMessageLayoutType.Simple));
            Assert.That(Enum.IsDefined(typeof(FileNamingStrategyType), FileNamingStrategyType.Date));
            Assert.That(Enum.IsDefined(typeof(RollingStrategyType), RollingStrategyType.Daily));
        }
    }
}
