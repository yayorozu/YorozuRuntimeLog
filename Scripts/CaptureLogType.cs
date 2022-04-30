using System;

namespace Yorozu
{
    [Flags]
    internal enum CaptureLogType
    {
        Log = 1 << 1,
        Warning = 1 << 2,
        Error = 1 << 3,
        Assert = 1 << 4,
        Exception = 1 << 5,
        All = Log | Warning | Error | Assert | Exception,
        ErrorAll = Error | Assert | Exception,
    }
}