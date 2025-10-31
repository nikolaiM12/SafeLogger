using Homework_1.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework_1.Implementation
{
    public class SafeLogger : ISafeLogger
    {
        private readonly string _logPath;
        private readonly string _diagnosticPath;
        private readonly int _maxLength;

        private const string DefaultDiagnosticPath = "diagnostics.log";
        private const int DefaultMaxLength = 4096;
        private const string NullPlaceholder = "[null]";
        private const string TruncationMarker = "[truncated]";

        public SafeLogger(string logPath, 
            string diagnosticPath = DefaultDiagnosticPath, 
            int maxLength = DefaultMaxLength)
        {
            _logPath = logPath;
            _diagnosticPath = diagnosticPath;
            _maxLength = maxLength;
        }

        public void LogUserMessage(string userMessage)
        {
            try
            {
                string message = SanitizeInput(userMessage);
                string entry = $"{DateTime.UtcNow:O} [INFO] {message}{Environment.NewLine}";
                File.AppendAllText(_logPath, entry, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                WriteDiagnostic(ex);
            }
        }

        private string SanitizeInput(string input)
        {
            if(input == null)
            {
                return NullPlaceholder;
            }

            string msg = input
                .Replace("\r", "\\r")
                .Replace("\n", "\\n")
                .Replace("{", "{{")
                .Replace("}", "}}");

            if(msg.Length > _maxLength)
            {
                msg = msg.Substring(0, _maxLength) + TruncationMarker;
            }

            return msg;
        }

        private void WriteDiagnostic(Exception ex)
        {
            try
            {
                string diagEntry = $"{DateTime.UtcNow:O} [ERROR] SafeLogger failure: {ex.Message}{Environment.NewLine}";
                File.AppendAllText(_diagnosticPath, diagEntry, Encoding.UTF8);
            }
            catch
            {

            }
        }
    }
}
