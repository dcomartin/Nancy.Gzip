using System;

namespace Nancy.Gzip
{
    public interface ILog
    {
        void Info(string message, Exception ex = null);
        void Debug(string message, Exception ex = null);
        void Warn(string message, Exception ex = null);
        void Error(string message, Exception ex = null);
    }
}
