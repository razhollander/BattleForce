using System;
using CoreDomain.Scripts.Services.Logger.Base;

namespace Core.Scripts.Services.Logger
{
    public class EmptyLogger : LoggerBase
    {
        public override void Log(string message)
        {
        }

        public override void LogWarning(string message)
        {
        }

        public override void LogError(string message)
        {
        }

        public override void LogException(Exception exception)
        {
        }

        public override void LogTopic(string message, LogTopicType debugLogTopic = LogTopicType.Temp, string callerFilePath = "", string callerMemberName = "")
        {
        }
    }
}