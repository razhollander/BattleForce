using System;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace CoreDomain.Scripts.Services.Logger.Base
{
    public static class LogService
    {
        private static ILogger _logger;

        internal static void InjectLogger(ILogger logger)
        {
            _logger = logger;
        }

        [Conditional("INFO_LOGS_ENABLED")]
        public static void Log(string message)
        {
            _logger.Log(message);
        }

        [Conditional("INFO_LOGS_ENABLED")]
        public static void LogWarning(string message)
        {
            _logger.LogWarning(message);
        }

        [Conditional("ERROR_LOGS_ENABLED")]
        public static void LogError(string message)
        {
            _logger.LogError(message);
        }
        
        [Conditional("ERROR_LOGS_ENABLED")]
        public static void LogException(Exception exception)
        {
            _logger.LogException(exception);
        }

        [Conditional("INFO_LOGS_ENABLED")]
        public static void LogTopic(string message, LogTopicType logTopicType = LogTopicType.Temp, [CallerFilePath] string callerFilePath = "", [CallerMemberName] string callerMemberName ="")
        {
            _logger.LogTopic(message, logTopicType, callerFilePath, callerMemberName);
        }
    }
}