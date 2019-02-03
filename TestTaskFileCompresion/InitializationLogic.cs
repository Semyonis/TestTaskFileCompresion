using System.IO.Compression;

using Core.Instances;
using Core.Readers;
using Core.Services;
using Core.Writers;

namespace TestTaskFileCompression
{
    public class InitializationLogic
    {
        private readonly SystemSettingMonitor monitor;
        private readonly StreamResultQueue queue;

        public InitializationLogic(SystemSettingMonitor monitor)
        {
            this.monitor = monitor;

            queue = new StreamResultQueue();
        }

        public void InitializeWorkers(CompressionMode operationType,
            string inputFilePath,
            string outputFilePath)
        {
            ReadersInitialization(operationType, inputFilePath);

            WriterInitialization(operationType, outputFilePath);
        }

        private void ReadersInitialization(CompressionMode operationType,
            string inputFilePath)
        {
            var service = new ReaderService(queue, monitor);

            BaseReadersLogic logic;
            if (operationType == CompressionMode.Compress)
            {
                logic = new CompressReadersLogic();
            }
            else
            {
                logic = new DecompressReadersLogic();
            }

            logic.Call(service, inputFilePath);
        }

        private void WriterInitialization(CompressionMode operationType,
            string outputFilePath)
        {
            BaseWriterLogic logic;
            if (operationType == CompressionMode.Compress)
            {
                logic = new CompressWriterLogic();
            }
            else
            {
                logic = new DecompressWriterLogic();
            }

            var service = new WriterService(queue, monitor);

            logic.Call(service, outputFilePath);
        }
    }
}