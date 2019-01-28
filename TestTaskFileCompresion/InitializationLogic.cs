using System;
using System.IO.Compression;

using Core.Instances;
using Core.Readers;
using Core.Writers;

namespace TestTaskFileCompression
{
    public static class InitializationLogic
    {
        public static void InitializeWorkers(CompressionMode operationType, string inputFilePath, string outputFilePath)
        {
            var queue = new StreamResultQueue();

            WriterInitialization(operationType, outputFilePath, queue);

            ReadersInitialization(operationType, inputFilePath, queue);

            IntegrateStaticClassesDependencies();
        }

        public static void HandleException(Exception e, string info)
        {
            //TODO: logging
            Console.WriteLine(info);
        }
        
        private static void ReadersInitialization(CompressionMode operationType, string inputFilePath, StreamResultQueue queue)
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

            IntegrateReadersDependencies(logic, queue);

            logic.Call();
        }

        private static void WriterInitialization(CompressionMode operationType, string outputFilePath, StreamResultQueue queue)
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

            IntegrateWriterDependencies(logic, queue);

            logic.Call();
        }

        private static void IntegrateReadersDependencies(BaseReadersLogic logic, StreamResultQueue queue)
        {
            logic.HandleException = HandleException;
            logic.SetInputStreamIsSliced = queue.SetInputStreamIsSliced;
            logic.IncrementPartCount = queue.IncrementPartCount;

            logic.Put = queue.Put;

            logic.GetProcessorCount = SystemSettingMonitor.Instance.GetProcessorCount;
            logic.GetNewStream = SystemSettingMonitor.Instance.GetNewStream;
        }

        private static void IntegrateWriterDependencies(BaseWriterLogic logic, StreamResultQueue queue)
        {
            logic.HandleException = HandleException;
            logic.Clear = SystemSettingMonitor.Instance.Clear;

            logic.Remove = queue.Remove;

            logic.GetPartById = queue.GetPartById;
            logic.IsNotEnded = queue.IsNotEnded;
        }

        private static void IntegrateStaticClassesDependencies()
        {
            SystemSettingMonitor.Instance.HandleException = HandleException;
        }
    }
}