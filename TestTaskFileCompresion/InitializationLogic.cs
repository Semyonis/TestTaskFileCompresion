using System.IO.Compression;

using TestTaskFileCompression.Instances;
using TestTaskFileCompression.Readers;
using TestTaskFileCompression.Writers;

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
            BaseReadersLogic logic;
            if (operationType == CompressionMode.Compress)
            {
                logic = new CompressReadersLogic(inputFilePath);
            }
            else
            {
                logic = new DecompressReadersLogic(inputFilePath);
            }

            IntegrateReadersDependencies(logic);

            logic.Call();
        }

        private static void WriterInitialization(CompressionMode operationType, string outputFilePath)
        {
            BaseWriterLogic logic;
            if (operationType == CompressionMode.Compress)
            {
                logic = new CompressWriterLogic(outputFilePath);
            }
            else
            {
                logic = new DecompressWriterLogic(outputFilePath);
            }

            IntegrateWriterDependencies(logic);

            logic.Call();
        }

        private static void IntegrateReadersDependencies(BaseReadersLogic logic)
        {
            logic.SetIsStreamSliced = StreamResultQueue.Instance.SetIsStreamSliced;
            logic.IncrementPartCount = StreamResultQueue.Instance.IncrementPartCount;

            logic.Put = StreamResultQueue.Instance.Put;

            logic.GetProcessorCount = SystemSettingMonitor.Instance.GetProcessorCount;
            logic.GetNewStream = SystemSettingMonitor.Instance.GetNewStream;
        }

        private static void IntegrateWriterDependencies(BaseWriterLogic logic)
        {
            logic.Clear = SystemSettingMonitor.Instance.Clear;

            logic.Remove = StreamResultQueue.Instance.Remove;

            logic.GetQueue = StreamResultQueue.Instance.GetQueue;
            logic.IsNotEnded = StreamResultQueue.Instance.IsNotEnded;
        }
    }
}