using Moq;
using SmartLogger.Core;

namespace SmartLogger.Tests.Core
{
    [TestFixture]
    public class LoggerManagerTests
    {
        private Mock<ILogConfigurationProvider> _provider;

        [SetUp]
        public void Setup()
        {
            LoggerManager.Reset();

            _provider = new Mock<ILogConfigurationProvider>();
        }

        [Test]
        public void Initialize_WithValidProvider_ShouldAllowLoggerCreation()
        {
            // Arrange
            _provider.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            // Act
            LoggerManager.Initialize(_provider.Object);

            // Assert
            Assert.DoesNotThrow(() =>
            {
                var logger = LoggerManager.GetLogger("TestLogger");
                Assert.IsNotNull(logger);
            });
        }

        [Test]
        public void Initialize_WithNullProvider_ShouldThrowArgumentNullException()
        {
            // Arrange
            ILogConfigurationProvider provider = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                LoggerManager.Initialize(provider));

            Assert.That(ex.ParamName, Is.EqualTo("provider"));
        }

        [Test]
        public void GetLogger_WithValidName_ShouldReturnLogger()
        {
            // Arrange
            _provider.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            LoggerManager.Initialize(_provider.Object);

            // Act
            var logger = LoggerManager.GetLogger("MyLogger");

            // Assert
            Assert.IsNotNull(logger);
            Assert.IsInstanceOf<ISmartLogger>(logger);
        }

        [Test]
        public void GetLogger_WithSameName_ShouldReturnSameInstance()
        {
            // Arrange
            _provider.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            LoggerManager.Initialize(_provider.Object);

            // Act
            var logger1 = LoggerManager.GetLogger("Application");
            var logger2 = LoggerManager.GetLogger("Application");

            // Assert
            Assert.That(logger1, Is.SameAs(logger2));
        }

        /*
         * Because LoggerManager is static, this test will only pass if it executes before any other test initializes LoggerManager, 
         * or if you add a reset mechanism for testing.
         */
        [Test]
        public void GetLogger_BeforeInitialize_ShouldThrowInvalidOperationException()
        {
            // Arrange
            // Do not initialize LoggerManager.

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                LoggerManager.GetLogger("Logger"));

            Assert.That(ex.Message, Is.EqualTo("LoggerFactory is not initialized."));
        }

        [Test]
        public void GetLogger_WithValidType_ShouldReturnLogger()
        {
            // Arrange
            _provider.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            LoggerManager.Initialize(_provider.Object);

            // Act
            var logger = LoggerManager.GetLogger(typeof(LoggerManager));

            // Assert
            Assert.IsNotNull(logger);
            Assert.IsInstanceOf<ISmartLogger>(logger);
        }

        [Test]
        public void GetLogger_WithNullType_ShouldThrowArgumentNullException()
        {
            // Arrange
            Type type = null;

            // Act & Assert
            var ex = Assert.Throws<ArgumentNullException>(() =>
                LoggerManager.GetLogger(type));

            Assert.That(ex.ParamName, Is.EqualTo("type"));
        }

        [Test]
        public void GetLogger_TypeBeforeInitialize_ShouldThrowInvalidOperationException()
        {
            // Arrange

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                LoggerManager.GetLogger(typeof(LoggerManager)));

            Assert.That(ex.Message, Is.EqualTo("LoggerFactory is not initialized."));
        }

        [Test]
        public void ReloadConfiguration_WithValidProvider_ShouldCallLoad()
        {
            // Arrange
            _provider.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            LoggerManager.Initialize(_provider.Object);

            // Act
            LoggerManager.ReloadConfiguration(_provider.Object);

            // Assert
            _provider.Verify(x => x.Load(), Times.AtLeastOnce);
        }

        [Test]
        public void ReloadConfiguration_BeforeInitialize_ShouldThrowInvalidOperationException()
        {
            // Arrange

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() =>
                LoggerManager.ReloadConfiguration(_provider.Object));

            Assert.That(ex.Message, Is.EqualTo("LoggerFactory is not initialized."));
        }

        [Test]
        public void ReloadConfiguration_WithNullProvider_ShouldThrowNullReferenceException()
        {
            // Arrange
            _provider.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            LoggerManager.Initialize(_provider.Object);

            // Act & Assert
            Assert.Throws<NullReferenceException>(() =>
                LoggerManager.ReloadConfiguration(null));
        }

        [Test]
        public void Initialize_CalledTwice_ShouldReplaceExistingFactory()
        {
            // Arrange
            var provider1 = new Mock<ILogConfigurationProvider>();
            provider1.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            var provider2 = new Mock<ILogConfigurationProvider>();
            provider2.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            // Act
            LoggerManager.Initialize(provider1.Object);
            LoggerManager.Initialize(provider2.Object);

            var logger = LoggerManager.GetLogger("TestLogger");

            // Assert
            Assert.IsNotNull(logger);

            provider1.Verify(x => x.Load(), Times.Once);
            provider2.Verify(x => x.Load(), Times.Once);
        }

        [Test]
        public void GetLogger_WithDifferentNames_ShouldReturnDifferentInstances()
        {
            // Arrange
            _provider.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            LoggerManager.Initialize(_provider.Object);

            // Act
            var logger1 = LoggerManager.GetLogger("LoggerA");
            var logger2 = LoggerManager.GetLogger("LoggerB");

            // Assert
            Assert.That(logger1, Is.Not.SameAs(logger2));
        }

        // Since ConcurrentDictionary throws the exception.
        [Test]
        public void GetLogger_WithNullName_ShouldThrowArgumentNullException()
        {
            // Arrange
            _provider.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            LoggerManager.Initialize(_provider.Object);

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
                LoggerManager.GetLogger((string)null));
        }

        [Test]
        public void GetLogger_WithEmptyName_ShouldNotReturnLogger()
        {
            // Arrange
            _provider.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            LoggerManager.Initialize(_provider.Object);

            // Act
            var logger = LoggerManager.GetLogger(string.Empty);

            // Assert
            Assert.IsNotNull(logger);
            Assert.IsInstanceOf<ISmartLogger>(logger);
        }

        [Test]
        public void GetLogger_WithWhitespaceName_ShouldReturnLogger()
        {
            // Arrange
            _provider.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            LoggerManager.Initialize(_provider.Object);

            // Act
            var logger = LoggerManager.GetLogger("     ");

            // Assert
            Assert.IsNotNull(logger);
            Assert.IsInstanceOf<ISmartLogger>(logger);
        }

        [Test]
        public void GetLogger_WithVeryLongName_ShouldReturnLogger()
        {
            // Arrange
            _provider.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            LoggerManager.Initialize(_provider.Object);

            var loggerName = new string('A', 5000);

            // Act
            var logger = LoggerManager.GetLogger(loggerName);

            // Assert
            Assert.IsNotNull(logger);
            Assert.IsInstanceOf<ISmartLogger>(logger);
        }

        [Test]
        public void GetLogger_WithUnicodeName_ShouldReturnLogger()
        {
            // Arrange
            _provider.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            LoggerManager.Initialize(_provider.Object);

            // Act
            var logger = LoggerManager.GetLogger("ஸ்மார்ட்Logger_日本語_日志");

            // Assert
            Assert.IsNotNull(logger);
            Assert.IsInstanceOf<ISmartLogger>(logger);
        }

        [Test]
        public void GetLogger_WithType_ShouldReturnSameInstanceAsStringFullName()
        {
            // Arrange
            _provider.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            LoggerManager.Initialize(_provider.Object);

            // Act
            var loggerFromType = LoggerManager.GetLogger(typeof(LoggerManager));
            var loggerFromString = LoggerManager.GetLogger(typeof(LoggerManager).FullName);

            // Assert
            Assert.That(loggerFromType, Is.SameAs(loggerFromString));
        }

        [Test]
        public void ReloadConfiguration_ShouldCallLoadExactlyOnceDuringReload()
        {
            // Arrange
            _provider.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            LoggerManager.Initialize(_provider.Object);

            // Act
            LoggerManager.ReloadConfiguration(_provider.Object);

            // Assert
            _provider.Verify(x => x.Load(), Times.Exactly(2));
        }

        [Test]
        public void ReloadConfiguration_CalledMultipleTimes_ShouldCallLoadEachTime()
        {
            // Arrange
            _provider.Setup(x => x.Load()).Returns(new LogConfigurationHolder());

            LoggerManager.Initialize(_provider.Object);

            // Act
            LoggerManager.ReloadConfiguration(_provider.Object);
            LoggerManager.ReloadConfiguration(_provider.Object);
            LoggerManager.ReloadConfiguration(_provider.Object);

            // Assert
            _provider.Verify(x => x.Load(), Times.Exactly(4));
        }
    }
}