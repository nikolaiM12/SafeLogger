using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Homework_1
{
    public class VulnerableLogger
    {
        private readonly string logPath;
        private readonly string secret = "TOP_SECRET_TOKEN_ABC123";
        public VulnerableLogger(string logPath)
        {
            this.logPath = logPath;
        }
        public void LogUserMessageVulnerable(string userMsg)
        {
            string formatted = string.Format(userMsg, secret);
            File.AppendAllText(logPath, DateTime.UtcNow.ToString("o") + " " + formatted + Environment.NewLine);
        }
    }
}
