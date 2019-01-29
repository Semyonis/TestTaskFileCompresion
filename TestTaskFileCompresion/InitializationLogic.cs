﻿using System;
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
            SystemSettingMonitor.Instance.LogError(e.Message + "\n" + e.StackTrace + "\n------------------\n" + info);

            SystemSettingMonitor.Instance.Cancel();
        }

        private static void ReadersInitialization(CompressionMode operationType,
            string inputFilePath,
            StreamResultQueue queue)
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

        private static void WriterInitialization(CompressionMode operationType,
            string outputFilePath,
            StreamResultQueue queue)
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

            logic.Put = queue.Put;

            logic.SetInputStreamIsSliced = queue.SetInputStreamIsSliced;
            logic.IncrementPartCount = queue.IncrementPartCount;

            logic.GetProcessorCount = SystemSettingMonitor.Instance.GetProcessorCount;
            logic.GetNewStream = SystemSettingMonitor.Instance.GetNewStream;

            logic.Token = SystemSettingMonitor.Instance.Token;
        }

        private static void IntegrateWriterDependencies(BaseWriterLogic logic, StreamResultQueue queue)
        {
            logic.HandleException = HandleException;

            logic.Remove = queue.Remove;

            logic.GetPartById = queue.GetPartById;
            logic.IsNotEnded = queue.IsNotEnded;

            logic.Clear = SystemSettingMonitor.Instance.Clear;

            logic.Token = SystemSettingMonitor.Instance.Token;
        }

        private static void IntegrateStaticClassesDependencies()
        {
            SystemSettingMonitor.Instance.HandleException = HandleException;
        }
    }
}