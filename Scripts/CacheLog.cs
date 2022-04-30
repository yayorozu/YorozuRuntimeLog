using UnityEngine;

namespace Yorozu
{
    internal class CacheLog
    {
        internal LogType LogType;
        internal string Condition;
        internal string Stacktrace;

        internal CacheLog(LogType logType, string condition, string stacktrace)
        {
            LogType = logType;
            Condition = condition;
            Stacktrace = stacktrace;
        }
    }
}