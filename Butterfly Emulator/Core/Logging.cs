using System;
using System.Text;
using ButterStorm;
using ConsoleWriter;

namespace Butterfly.Core
{
    public static class Logging
    {
        internal static bool DisabledState
        {
            get
            {
                return Writer.DisabledState;
            }
            set
            {
                Writer.DisabledState = value;
            }
        }

        internal static void WriteLine(string Line)
        {
            Writer.WriteLine(Line);
        }

        internal static void LogException(string logText)
        {
            if (EmuSettings.LOG_EXCEPTIONS == true)
                Writer.LogException(logText);
        }

        internal static void LogPacketData(string logText)
        {
            if (EmuSettings.LOG_EXCEPTIONS == true)
                Writer.LogPacketData(logText);
        }

        internal static void LogReport(string FileName, string logText)
        {
            Writer.LogReport(FileName, logText);
        }

        internal static void LogCriticalException(string logText)
        {
            if (EmuSettings.LOG_EXCEPTIONS == true)
                Writer.LogCriticalException(logText);
        }

        internal static void LogCacheError(string logText)
        {
            if (EmuSettings.LOG_EXCEPTIONS == true)
                Writer.LogCacheError(logText);
        }

        internal static void LogThreadException(string Exception, string Threadname)
        {
            if (EmuSettings.LOG_EXCEPTIONS == true)
                Writer.LogThreadException(Exception, Threadname);
        }

        public static void LogQueryError(Exception Exception, string query)
        {
            if (EmuSettings.LOG_EXCEPTIONS == true)
                Writer.LogQueryError(Exception, query);
        }

        internal static void LogPacketException(string Packet, string Exception)
        {
            if (EmuSettings.LOG_EXCEPTIONS == true)
                Writer.LogPacketException(Packet, Exception);
        }

        internal static void HandleException(Exception pException, string pLocation)
        {
            Writer.HandleException(pException, pLocation);
        }

        internal static void DisablePrimaryWriting(bool ClearConsole)
        {
            Writer.DisablePrimaryWriting(ClearConsole);
        }

        internal static void LogShutdown(StringBuilder builder)
        {
            Writer.LogShutdown(builder);
        }
    }
}
