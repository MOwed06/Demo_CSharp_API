using Microsoft.Extensions.Logging;

namespace BigBooks.UnitTest.Common
{
    public class LogMessage
    {
        public LogLevel LogLevel { get; set; }
        public string Message { get; set; } = string.Empty;
        public DateTime DateTime { get; set; }
    }
}
