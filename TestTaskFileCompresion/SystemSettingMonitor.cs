using System;
using System.Diagnostics;

namespace TestTaskFileCompression
{
    public sealed class SystemSettingMonitor
    {
        private static SystemSettingMonitor instance;

        private static volatile object locker = new object();

        private readonly PerformanceCounter cpuUsage;
        private readonly PerformanceCounter memUsage;

        private SystemSettingMonitor()
        {
            cpuUsage = new PerformanceCounter("Processor", "% Processor Time", "_Total");
            memUsage = new PerformanceCounter("Memory", "Available MBytes");

            ProcessorCount = Environment.ProcessorCount;
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

        public int ProcessorCount { get; private set; }

        public double CpuUsage
        {
            get { return cpuUsage.NextValue(); }
        }

        public double MemUsage
        {
            get { return memUsage.NextValue(); }
        }
    }
}