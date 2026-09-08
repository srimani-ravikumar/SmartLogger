using NUnit.Framework;
using SmartLogger.Core;
using System;
using System.IO;

namespace SmartLogger.Tests.Configurations
{
    [TestFixture]
    public class XmlConfigurationProviderTests
    {
        private string _testDirectory;
        private string _configFilePath;

        [SetUp]
        public void Setup()
        {
            _testDirectory = Path.Combine(
                Path.GetTempPath(),
                "SmartLoggerTests",
                Guid.NewGuid().ToString());

            Directory.CreateDirectory(_testDirectory);

            _configFilePath = Path.Combine(
                _testDirectory,
                "logger.xml");
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_testDirectory))
            {
                Directory.Delete(_testDirectory, recursive: true);
            }
        }

        [Test]
        public void Load_WithValidXml_ShouldReturnConfiguration()
        {
            // Arrange
            var xml = """
                      <?xml version="1.0" encoding="utf-8"?>
                      <LogConfigurationHolder>
                          <RootLogLevel>INFO</RootLogLevel>
                          <EnableAsyncLoggingProcess>false</EnableAsyncLoggingProcess>
                      </LogConfigurationHolder>
                      """;

            WriteConfiguration(xml);

            var provider = new XmlConfigurationProvider(
                _configFilePath);

            // Act
            var configuration = provider.Load();

            // Assert
            Assert.IsNotNull(configuration);
        }

        [Test]
        public void Load_WithConfiguredValues_ShouldPreserveConfiguration()
        {
            // Arrange
            var xml = """
                      <?xml version="1.0" encoding="utf-8"?>
                      <LogConfigurationHolder>
                          <RootLogLevel>DEBUG</RootLogLevel>
                          <EnableAsyncLoggingProcess>true</EnableAsyncLoggingProcess>
                      </LogConfigurationHolder>
                      """;

            WriteConfiguration(xml);

            var provider = new XmlConfigurationProvider(
                _configFilePath);

            // Act
            var configuration = provider.Load();

            // Assert
            Assert.That(
                configuration.RootLogLevel,
                Is.EqualTo(LogLevel.DEBUG));

            Assert.That(
                configuration.EnableAsyncLoggingProcess,
                Is.True);
        }

        [Test]
        public void Load_WithAppendersAndOverrides_ShouldDeserializeConfiguration()
        {
            // Arrange
            var xml = """
                      <?xml version="1.0" encoding="utf-8"?>
                      <LogConfigurationHolder>
                          <RootLogLevel>INFO</RootLogLevel>

                          <LoggerOverrides>
                              <LoggerOverrideConfiguration>
                                  <LoggerName>SmartLogger.Tests</LoggerName>
                                  <LogLevel>DEBUG</LogLevel>
                              </LoggerOverrideConfiguration>
                          </LoggerOverrides>

                          <Appenders>
                              <AppenderConfiguration>
                                  <Destination>
                                      <Type>Console</Type>
                                  </Destination>
                                  <Formatter>
                                      <OutputFormat>PlainText</OutputFormat>
                                      <LayoutType>Simple</LayoutType>
                                  </Formatter>
                              </AppenderConfiguration>

                              <AppenderConfiguration>
                                  <Destination>
                                      <Type>FileSystem</Type>
                                      <File>
                                          <Directory>Logs</Directory>
                                          <FileName>Application</FileName>
                                          <Extension>log</Extension>
                                      </File>
                                  </Destination>
                                  <Formatter>
                                      <OutputFormat>PlainText</OutputFormat>
                                      <LayoutType>Detailed</LayoutType>
                                  </Formatter>
                              </AppenderConfiguration>
                          </Appenders>
                      </LogConfigurationHolder>
                      """;

            WriteConfiguration(xml);

            var provider = new XmlConfigurationProvider(
                _configFilePath);

            // Act
            var configuration = provider.Load();

            // Assert
            Assert.That(
                configuration.LoggerOverrides.Count,
                Is.EqualTo(1));

            Assert.That(
                configuration.LoggerOverrides[0].LoggerName,
                Is.EqualTo("SmartLogger.Tests"));

            Assert.That(
                configuration.LoggerOverrides[0].LogLevel,
                Is.EqualTo(LogLevel.DEBUG));

            Assert.That(
                configuration.Appenders.Count,
                Is.EqualTo(2));

            Assert.That(
                configuration.Appenders[0].Destination.Type,
                Is.EqualTo(LogOutputDestination.Console));

            Assert.That(
                configuration.Appenders[1].Destination.Type,
                Is.EqualTo(LogOutputDestination.FileSystem));

            Assert.That(
                configuration.Appenders[1].Destination.File.FileName,
                Is.EqualTo("Application"));

            Assert.That(
                configuration.Appenders[1].Formatter.LayoutType,
                Is.EqualTo(LogMessageLayoutType.Detailed));
        }

        [Test]
        public void Load_WithMalformedXml_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var xml = """
                      <?xml version="1.0" encoding="utf-8"?>
                      <LogConfigurationHolder>
                          <RootLogLevel>INFO</RootLogLevel>
                      """;

            WriteConfiguration(xml);

            var provider = new XmlConfigurationProvider(
                _configFilePath);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                provider.Load());

            Assert.That(
                ex.Message,
                Does.Contain("XML"));
        }

        [Test]
        public void Load_WithInvalidXmlValue_ShouldThrowInvalidOperationException()
        {
            // Arrange
            var xml = """
                      <?xml version="1.0" encoding="utf-8"?>
                      <LogConfigurationHolder>
                          <RootLogLevel>INVALID_LEVEL</RootLogLevel>
                      </LogConfigurationHolder>
                      """;

            WriteConfiguration(xml);

            var provider = new XmlConfigurationProvider(
                _configFilePath);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
                provider.Load());
        }

        private void WriteConfiguration(string xml)
        {
            File.WriteAllText(_configFilePath, xml);
        }
    }
}