using System;
using System.IO;

namespace Logger
{
    public class FileLogger : ILogger
    {
        private readonly object _fileLock = new object();
        private readonly string _filePath;

        public FileLogger(string filePath)
        {
            _filePath = filePath ?? throw new ArgumentNullException(nameof(filePath));

            var fileDirectory = Path.GetDirectoryName(filePath);

            if (!Directory.Exists(fileDirectory))
            {
                Directory.CreateDirectory(fileDirectory);
            }

            if (!File.Exists(filePath))
            {
                File.Create(filePath).Close();
            }
        }

        public void Log(string text)
        {
            lock (_fileLock)
            {
                File.AppendAllText(_filePath, text);
            }
        }
    }
}