using System;
using System.Diagnostics;
using System.IO;

namespace TestTaskFileCompression
{
    public sealed class SystemSettingMonitor
    {
        private static SystemSettingMonitor instance;

        private static volatile object locker = new object();

        private readonly PerformanceCounter cpuUsage;
        private readonly PerformanceCounter memUsage;

        private readonly string tempDirectoryPath;
        private readonly int processorCount;

        private SystemSettingMonitor()
        {
            cpuUsage = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            memUsage = new PerformanceCounter("Memory", "Available MBytes");

            processorCount = Environment.ProcessorCount;

            tempDirectoryPath = Path.GetTempPath();
            Directory.CreateDirectory(tempDirectoryPath);
        }

        public static SystemSettingMonitor Instance
        {
            get
            {
                lock (locker)
                {
                    return instance ?? (instance = new SystemSettingMonitor());
                }
            }
        }

        public int GetProcessorCount()
        { 
            return processorCount;
        }

        public double CpuUsage
        {
            get { return cpuUsage.NextValue(); }
        }

        public double MemUsage
        {
            get { return memUsage.NextValue(); }
        }

        public Stream GetNewStream(int length)
        {
            var nextValue = MemUsage;
            if (length < nextValue * 100000)
            {
                return new MemoryStream();
            }
            else
            {
                var randomFileName = Path.GetRandomFileName();
                while(File.Exists(Path.Combine(tempDirectoryPath, randomFileName)))
                {
                    randomFileName = Path.GetRandomFileName();
                }
                
                return new FileStream(randomFileName, FileMode.CreateNew);
            }
        }

        public void Clear()
        {
            Directory.Delete(tempDirectoryPath, true);
        }
    }
}