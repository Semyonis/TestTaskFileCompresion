using System.IO.Compression;
using System.Threading;

namespace TestTaskFileCompression
{
    public static class InitializationLogic
    {
        public static void InitializeWorkers(CompressionMode operationType, string inputFilePath, string outputFilePath)
        {
            WriterInitialization(operationType, outputFilePath);

            ReadersInitialization(operationType, inputFilePath);
        }

        private static void ReadersInitialization(CompressionMode operationType, string inputFilePath)
        {
            MultithreadOperationLogic logic;
            if (operationType == CompressionMode.Compress)
            {
                logic = new MultithreadCompressLogic(inputFilePath);
            }
            else
            {
                logic = new MultithreadDecompressLogic(inputFilePath);
            }

            logic.SetIsStreamSliced = ScheduledWriter.Instance.SetIsStreamSliced;
            logic.IncrementPartCount = ScheduledWriter.Instance.IncrementPartCount;

            logic.SetNewResult = ScheduledWriter.Instance.SetNewResult;

            logic.GetProcessorCount = SystemSettingMonitor.Instance.GetProcessorCount;
            logic.GetNewStream = SystemSettingMonitor.Instance.GetNewStream;

            logic.Call();
        }

        private static void WriterInitialization(CompressionMode operationType, string outputFilePath)
        {
            ScheduledWriter.Instance.Initialize(operationType, outputFilePath);

            ScheduledWriter.Instance.Clear = SystemSettingMonitor.Instance.Clear;

            new Thread(ScheduledWriter.Instance.StartWorker).Start();
        }
    }
}