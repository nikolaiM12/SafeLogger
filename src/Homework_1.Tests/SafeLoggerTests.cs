using Homework_1.Implementation;

namespace Homework_1.Tests
{
    public class SafeLoggerTests
    {
        private const string Secret = "TOP_SECRET_TOKEN_ABC123";

        [Fact]
        public void VulnerableLogger_ShouldLeakSecret_WhenUserMessageContainsPlaceholder()
        {
            string logPath = CreateLogPath();

            var vulnerableLogger = new VulnerableLogger(logPath);

            vulnerableLogger.LogUserMessageVulnerable("User said: {0}");
            string logContent = File.ReadAllText(logPath);

            Assert.Contains(Secret, logContent);
        }

        [Fact]
        public void SafeLogger_ShouldNotLeakSecret_WhenUserMessageContainsPlaceholder()
        {
            string logPath = CreateLogPath();

            var safeLogger = new SafeLogger(logPath);

            safeLogger.LogUserMessage("User said: {0}");
            string logContent = File.ReadAllText(logPath);

            Assert.DoesNotContain(Secret, logContent);
            Assert.Contains("User said: {{0}}", logContent);
        }

        [Theory]
        [InlineData("Hello\nWorld")]
        [InlineData("Hello\r\nWorld")]
        public void SafeLogger_ShouldEscapeNewLines_AndWriteSingleLine(string input)
        {
            string logPath = CreateLogPath();

            var safeLogger = new SafeLogger(logPath);

            safeLogger.LogUserMessage(input);
            string[] lines = File.ReadAllLines(logPath);

            Assert.Single(lines);
            Assert.Contains("\\n", lines[0]);
        }

        [Fact]
        public void SafeLogger_ShouldEscapeCurlyBraces_AndNotThrow()
        {
            string logPath = CreateLogPath();

            var safeLogger = new SafeLogger(logPath);
            string input = "Data { { } }";

            Exception ex = Record.Exception(() => safeLogger.LogUserMessage(input));
            string logContent = File.ReadAllText(logPath);

            Assert.Null(ex);
            Assert.Contains("{{", logContent);
            Assert.Contains("}}", logContent);
        }

        [Fact]
        public void SafeLogger_ShouldTruncateLongMessages_AndAppendTruncationMarker()
        {
            string logPath = CreateLogPath();

            var safeLogger = new SafeLogger(logPath, maxLength: 50);
            string longInput = new string('A', 100);

            safeLogger.LogUserMessage(longInput);
            string logContent = File.ReadAllText(logPath);

            Assert.Contains("[truncated]", logContent);
            Assert.True(logContent.Length < 200, "The log must be limited.");
        }

        [Fact]
        public void SafeLogger_ShouldWriteToDiagnostics_WhenExceptionOccurs()
        {
            string invalidPath = CreateLogPath("InvalidSubDir");
            Directory.CreateDirectory(invalidPath);

            string diagnosticPath = CreateLogPath("diagnostics.log");

            var safeLogger = new SafeLogger(invalidPath, diagnosticPath);

            Exception ex = Record.Exception(() => safeLogger.LogUserMessage("This will fail to write"));

            Assert.Null(ex);
            Assert.True(File.Exists(diagnosticPath), "Diagnostics log should be created when write fails");

            string diagContent = File.ReadAllText(diagnosticPath);
            Assert.Contains("SafeLogger failure", diagContent);
        }

        private static string CreateLogPath(string fileName = null)
        {
            string logDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "Logs");
            Directory.CreateDirectory(logDir);

            string finalFileName;

            if (!string.IsNullOrWhiteSpace(fileName))
            {
                finalFileName = fileName;
            }
            else
            {
                finalFileName = Guid.NewGuid().ToString() + ".log";
            }

            return Path.Combine(logDir, finalFileName);
        }
    }
}