using System;
using System.IO;

namespace AlphaPOS.VposConnector.Infrastructure.Logging
{
    public class FileLogger
    {
        private readonly object _lock = new object();
        private string _filePath;

        public FileLogger(string filePath)
        {
            _filePath = string.IsNullOrEmpty(filePath) ? "AlphaPOS.VposConnector.log" : filePath;
        }

        public void SetLogFile(string path)
        {
            if (!string.IsNullOrEmpty(path)) _filePath = path;
        }

        public void Info(string fmt, params object[] args)
        {
            Log("INFO", fmt, args);
        }

        public void Error(string fmt, params object[] args)
        {
            Log("ERROR", fmt, args);
        }

        private void Log(string level, string fmt, params object[] args)
        {
            try
            {
                lock (_lock)
                {
                    var line = DateTime.UtcNow.ToString("o") + " " + level + " " + string.Format(fmt, args) + Environment.NewLine;
                    File.AppendAllText(_filePath, line);
                }
            }
            catch { }
        }
    }
}
