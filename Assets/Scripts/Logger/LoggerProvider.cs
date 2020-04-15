using System;

namespace Logger
{
    /// <summary>
    /// A provider of the Logger implementation.
    /// </summary>
    public static class LoggerProvider
    {
        /// <summary>Path of the file that will store all the log data.</summary>
        private const string LogFileDirectory = "Assets/Logs/";

        /// <summary>The concrete Logger implementation.</summary>
        public static ILogger Logger { get; }

        static LoggerProvider()
        {
            Logger = new FileLogger(LogFileDirectory + "network_traffic_" + DateTime.Now.ToString("yyyy_MM_dd_HH_mm_ss") + ".log");
        }
    }
}