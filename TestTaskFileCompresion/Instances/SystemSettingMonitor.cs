using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;

using TestTaskFileCompression.Common;

namespace TestTaskFileCompression.Instances
{
    public sealed class SystemSettingMonitor
    {
        private static volatile object mutex = new object();

        private static SystemSettingMonitor instance;

        private readonly PerformanceCounter cpuUsage;
        private readonly PerformanceCounter memUsage;

        private readonly List<string> tempFileList = new List<string>();
        private readonly int processorCount;

        private SystemSettingMonitor()
        {
            cpuUsage = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            memUsage = new PerformanceCounter("Memory", "Available MBytes");

            processorCount = Environment.ProcessorCount;
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
                var tempDirectoryPath = Path.GetTempPath();
                var directoryRoot = Directory.GetDirectoryRoot(tempDirectoryPath);
                var driveInfo = DriveInfo.GetDrives().First(drive => drive.RootDirectory.Name == directoryRoot);
                if (driveInfo.AvailableFreeSpace < length)
                {
                    tempDirectoryPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "temp");
                    directoryRoot = Directory.GetDirectoryRoot(tempDirectoryPath);
                    driveInfo = DriveInfo.GetDrives().First(drive => drive.RootDirectory.Name == directoryRoot);
                    if (driveInfo.AvailableFreeSpace > length)
                    {
                        isAppDirectory = true;
                        if (!Directory.Exists(tempDirectoryPath))
                        {
                            Directory.CreateDirectory(tempDirectoryPath);
                        }
                    }
                    else
                    {
                        Thread.Sleep(AppConstants.SLEEP_TIMEOUT);
                        continue;
                    }
                }

                var randomFileName = Path.GetRandomFileName();
                while (File.Exists(Path.Combine(tempDirectoryPath, randomFileName)))
                {
                    randomFileName = Path.GetRandomFileName();
                }

                var newFile = Path.Combine(tempDirectoryPath, randomFileName);
                if (isAppDirectory)
                {
                    tempFileList.Add(newFile);
                }

                return new FileStream(newFile, FileMode.CreateNew, FileAccess.ReadWrite);
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
                    var message = "Cannot delete file. " + e.Message;
                    Console.WriteLine(message);
                }
            }
        }
    }
}