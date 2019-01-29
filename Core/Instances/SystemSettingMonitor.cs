using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

using Core.Common;
using Core.Tokens;

namespace Core.Instances
{
    public sealed class SystemSettingMonitor
    {
        public Action<Exception, string> HandleException;

        private static volatile object mutex = new object();

        private static SystemSettingMonitor instance;

        private readonly PerformanceCounter cpuUsage;
        private readonly PerformanceCounter memUsage;

        private readonly CancellationTokenSource cancelTokenSource = new CancellationTokenSource();
        private readonly List<string> tempFileList = new List<string>();
        private readonly int processorCount;
        private readonly string errorLogFile;


        private SystemSettingMonitor()
        {
            cpuUsage = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            memUsage = new PerformanceCounter("Memory", "Available MBytes");

            processorCount = Environment.ProcessorCount;

            errorLogFile = "errorLog.txt";
        }

        public static SystemSettingMonitor Instance
        {
            get
            {
                lock (mutex)
                {
                    return instance ?? (instance = new SystemSettingMonitor());
                }
            }
        }

        public double CpuUsage
        {
            get { return cpuUsage.NextValue(); }
        }

        public double MemUsage
        {
            get { return memUsage.NextValue(); }
        }

        public int GetProcessorCount() { return processorCount; }

        public CancellationToken Token
        {
            get { return cancelTokenSource.Token; }
        }

        public Stream GetNewStream(int length)
        {
            while (true)
            {
                if (CpuUsage > 90)
                {
                    Thread.Sleep(AppConstants.SLEEP_TIMEOUT);
                    continue;
                }

                var nextValue = MemUsage;
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
                        Thread.Sleep(AppConstants.SLEEP_TIMEOUT);
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

        public void Cancel()
        {
            cancelTokenSource.Cancel();
            Clear();
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

                    var handle = HandleException;
                    if (handle != null)
                    {
                        handle(e, errorMessage);
                    }
                }
            }
        }

        public void LogError(string errorMessage)
        {
            if (!File.Exists(errorLogFile))
            {
                File.Create(errorLogFile);
            }

            File.WriteAllText(errorLogFile, errorMessage);
        }

        private static DriveInfo GetDriveInfo(string tempDirectoryPath)
        {
            var directoryRoot = Directory.GetDirectoryRoot(tempDirectoryPath);
            return DriveInfo.GetDrives().First(drive => drive.RootDirectory.Name == directoryRoot);
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