using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;

using Core.Common;
using Core.Tokens;

namespace Core.Instances
{
    public sealed class SystemSettingMonitor
    {
        private readonly PerformanceCounter cpuUsage;
        private readonly PerformanceCounter memUsage;

        private readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource();

        private readonly List<string> tempFileList = new List<string>();

        private readonly string errorLogFile;
        private readonly int processorCount;
        
        public SystemSettingMonitor()
        {
            cpuUsage = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            memUsage = new PerformanceCounter("Memory", "Available MBytes");

            processorCount = Environment.ProcessorCount;

            errorLogFile = "errorLog.txt";
        }

        public CancellationToken Token
        {
            get { return cancelTokenSource.Token; }
        }

        public int GetProcessorCount() { return processorCount; }

        public Stream GetNewStream(int length)
        {
            while (true)
            {
                if ((double) cpuUsage.NextValue() > 90)
                {
                    continue;
                }

                var nextValue = (double) memUsage.NextValue();
                if (length < nextValue * AppConstants.BYTE_IN_MEGABYTE)
                {
                    return new MemoryStream();
                }

                var isAppDirectory = false;
                var directoryPath = Path.GetTempPath();
                var tempDriveInfo = GetDriveInfo(directoryPath);
                if (tempDriveInfo.AvailableFreeSpace < length)
                {
                    directoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
                    var currentDriveInfo = GetDriveInfo(directoryPath);
                    if (currentDriveInfo.AvailableFreeSpace > length)
                    {
                        isAppDirectory = true;
                        if (!Directory.Exists(directoryPath))
                        {
                            Directory.CreateDirectory(directoryPath);
                        }
                    }
                    else
                    {
                        continue;
                    }
                }

                var randomFileName = GetTempFileName(directoryPath);

                if (isAppDirectory)
                {
                    tempFileList.Add(randomFileName);
                }

                return new FileStream(randomFileName, FileMode.CreateNew, FileAccess.ReadWrite);
            }
        }

        public void Clear()
        {
            foreach (var file in tempFileList)
            {
                if (!File.Exists(file))
                {
                    continue;
                }

                try
                {
                    File.Delete(file);
                }
                catch (Exception e)
                {
                    var errorMessage = "Cannot delete file. " + e.Message;

                    HandleError(e, errorMessage);
                }
            }
        }

        public void HandleError(Exception exception, string additionalMessage)
        {
            LogError(exception.Message
                + "\n"
                + exception.StackTrace
                + "\n------------------\n"
                + additionalMessage);

            Cancel();
        }

        private void LogError(string errorMessage)
        {
            lock (errorLogFile)
            {
                if (!File.Exists(errorLogFile))
                {
                    File.Create(errorLogFile);
                }
            
                using (var stream = new FileStream(errorLogFile, FileMode.Append))
                {
                    using (var bw = new BinaryWriter(stream))
                    {
                        bw.Write(errorMessage);
                    }
                }
            }
        }

        private void Cancel()
        {
            cancelTokenSource.Cancel();

            Clear();
        }

        private static DriveInfo GetDriveInfo(string tempDirectoryPath)
        {
            var directoryRoot = Directory.GetDirectoryRoot(tempDirectoryPath);

            return DriveInfo.GetDrives()
                .First(drive => drive.RootDirectory.Name == directoryRoot);
        }

        private static string GetTempFileName(string tempDirectoryPath)
        {
            var randomFileName = GetFileNameInTempDirectory(tempDirectoryPath);

            while (File.Exists(randomFileName))
            {
                randomFileName = GetFileNameInTempDirectory(tempDirectoryPath);
            }

            return randomFileName;
        }

        private static string GetFileNameInTempDirectory(string tempDirectoryPath)
        {
            return Path.Combine(tempDirectoryPath, Path.GetRandomFileName());
        }
    }
}