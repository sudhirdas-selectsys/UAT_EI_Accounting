using Accounting.Utilities.Internal;

namespace Accounting.Web
{
    public static class FileLogger
    {
        private static readonly object _lock = new object();
        private static DateTime _lastCleanupDate = DateTime.MinValue;

        public static void LogInformation(string message) => WriteLog("Information", message);
        public static void LogError(string message) => WriteLog("Error", message);

        private static void WriteLog(string level, string message)
        {
            try
            {
                DateTime now = DateTime.Now;
                string logsFolder = WebAppReferenceUtility._configuration["FileLogger:LogsFolder"];
                if (!Directory.Exists(logsFolder))
                {
                    Directory.CreateDirectory(logsFolder);
                }

                string logFilePath = Path.Combine(logsFolder, $"{now:ddMMMyyyy}.txt");
                string logMessage = $"{level} : {now:yyyy-MM-dd HH:mm:ss} : {message}{Environment.NewLine}";

                lock (_lock)
                {
                    File.AppendAllText(logFilePath, logMessage);
                    if (_lastCleanupDate.Date != now.Date)
                    {
                        DeleteOldLogs(logsFolder);
                        _lastCleanupDate = now.Date;
                    }
                }
            }
            catch
            {
            }
        }

        private static void DeleteOldLogs(string logsFolder)
        {
            try
            {
                var files = Directory.GetFiles(logsFolder, "*.txt");
                foreach (var file in files)
                {
                    string nameWithoutExt = Path.GetFileNameWithoutExtension(file);
                    if (DateTime.TryParseExact(nameWithoutExt, "ddMMMyyyy", System.Globalization.CultureInfo.InvariantCulture, System.Globalization.DateTimeStyles.None, out DateTime fileDate))
                    {
                        int retentionDays = int.Parse(WebAppReferenceUtility._configuration["FileLogger:RetentionDays"]);
                        if (fileDate.Date < DateTime.Now.Date.AddDays(-retentionDays))
                        {
                            try
                            {
                                File.Delete(file);
                            }
                            catch
                            {
                                // File may be locked or already deleted — skip it
                            }
                        }
                    }
                }
            }
            catch
            {
            }
        }
    }
}
