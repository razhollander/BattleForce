using System;

namespace CoreDomain.Scripts.Services.Logger.Base
{
    public abstract class LoggerBase : ILogger
    {
        protected LoggerBase()
        {
            LogService.InjectLogger(this);
        }

        public abstract void Log(string message);
        public abstract void LogWarning(string message);
        public abstract void LogError(string message);
        public abstract void LogException(Exception exception);
        public abstract void LogTopic(string message, LogTopicType logTopicType = LogTopicType.Temp, string callerFilePath = "", string callerFileName = "");
    }
}