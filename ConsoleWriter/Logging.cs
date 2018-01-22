using System;
using System.Collections;
using System.IO;
using System.Text;

namespace ConsoleWriter
{
    public class Writer
    {
        private static bool mDisabled = false;

        public static bool DisabledState
        {
            get
            {
                return mDisabled;
            }
            set
            {
                mDisabled = value;
            }
        }

        public static void WriteLine(string Line)
        {
            if (!mDisabled)
                Console.WriteLine(Line);
        }

        public static void LogException(string logText)
        {
            WriteToFile(@"Logs\exceptions.txt", "<" + DateTime.Now + "> " + logText + "\r\n\r\n");
            WriteLine("Exception has been saved");
        }

        public static void LogDDOS(string logText)
        {
            WriteToFile(@"Logs\ddos.txt", "<" + DateTime.Now + "> " + logText + "\r\n\r\n");
            WriteLine("Exception has been saved");
        }

        public static void LogPacketData(string logText)
        {
            WriteToFile(@"Logs\packetdata.txt", "<" + DateTime.Now + "> " + logText + "\r\n\r\n");
            WriteLine("Exception has been saved");
        }

        public static void LogReport(string FileName, string logText)
        {
            WriteToFile(@"Logs\" + FileName, logText + "\r\n\r\n");
            WriteLine("Logged Report: " + FileName);
        }

        public static void LogCriticalException(string logText)
        {
            WriteToFile(@"Logs\criticalexceptions.txt", "<" + DateTime.Now + "> " + logText + "\r\n\r\n");
            WriteLine("CRITICAL ERROR LOGGED");
        }

        public static void LogCacheError(string logText)
        {
            WriteToFile(@"Logs\cacheerror.txt", "<" + DateTime.Now + "> " + logText + "\r\n\r\n");
            WriteLine("Critical error saved");
        }

        public static void LogThreadException(string Exception, string Threadname)
        {
            WriteToFile(@"Logs\threaderror.txt", "<" + DateTime.Now + "> Error in thread " + Threadname + ": \r\n" + Exception + "\r\n\r\n");
            WriteLine("Error in " + Threadname + " caught");
        }

        public static void LogQueryError(Exception Exception, string query)
        {
            WriteToFile(@"Logs\MySQLerrors.txt", "<" + DateTime.Now + "> Error in query: \r\n" + query + "\r\n" + Exception + "\r\n\r\n");
            WriteLine("Error in query caught");
        }

        public static void LogPacketException(string Packet, string Exception)
        {
            WriteToFile(@"Logs\packeterror.txt", "<" + DateTime.Now + "> Error in packet " + Packet + ": \r\n" + Exception + "\r\n\r\n");
            WriteLine("User disconnection logged");
        }

        public static void HandleException(Exception pException, string pLocation)
        {
            var ExceptionData = new StringBuilder();
            ExceptionData.AppendLine("<" + DateTime.Now + "> Exception logged in " + pLocation + ":");
            ExceptionData.AppendLine(pException.ToString());
            if (pException.InnerException != null)
            {
                ExceptionData.AppendLine("Inner exception:");
                ExceptionData.AppendLine(pException.InnerException.ToString());
            }
            if (pException.HelpLink != null)
            {
                ExceptionData.AppendLine("Help link:");
                ExceptionData.AppendLine(pException.HelpLink);
            }
            if (pException.Source != null)
            {
                ExceptionData.AppendLine("Source:");
                ExceptionData.AppendLine(pException.Source);
            }
            if (pException.Data != null)
            {
                ExceptionData.AppendLine("Data:");
                foreach (DictionaryEntry Entry in pException.Data)
                {
                    ExceptionData.AppendLine("  Key: " + Entry.Key + "Value: " + Entry.Value);
                }
            }
            if (pException.Message != null)
            {
                ExceptionData.AppendLine("Message:");
                ExceptionData.AppendLine(pException.Message);
            }
            if (pException.StackTrace != null)
            {
                ExceptionData.AppendLine("Stack trace:");
                ExceptionData.AppendLine(pException.StackTrace);
            }
            ExceptionData.AppendLine();
            ExceptionData.AppendLine();
            LogException(ExceptionData.ToString());
        }

        public static void DisablePrimaryWriting(bool ClearConsole)
        {
            mDisabled = true;
            if (ClearConsole)
                Console.Clear();
        }

        public static void LogShutdown(StringBuilder builder)
        {
            WriteToFile(@"Logs\shutdownlog.txt", "<" + DateTime.Now + "> " + builder.ToString());
        }

        private static void WriteToFile(string path, string content)
        {
            try
            {
                var errWriter = new FileStream(path, FileMode.Append, FileAccess.Write);
                var Msg = ASCIIEncoding.ASCII.GetBytes(Environment.NewLine + content);
                errWriter.Write(Msg, 0, Msg.Length);
                errWriter.Dispose();
            }
            catch (Exception e)
            {
                WriteLine("Could not write to file: " + e + ":" + content);
            }
        }

        private static void WriteCallback(IAsyncResult callback)
        {
            var stream = (FileStream)callback.AsyncState;
            stream.EndWrite(callback);
            stream.Dispose();
        }

        public static void Main(string[] argg)
        {

        }
    }
}
