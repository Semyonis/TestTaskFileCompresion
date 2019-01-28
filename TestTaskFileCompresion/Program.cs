using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

using Core.Common;

namespace TestTaskFileCompression
{
    static class Program
    {
        private const string COMPRESS_OPERATION = "COMPRESS";
        private const string DECOMPRESS_OPERATION = "DECOMPRESS";

        private const int STANDARD_ARGS_COUNT = 3;

        private static readonly List<string> OperationList = new List<string>
        {
            COMPRESS_OPERATION,
            DECOMPRESS_OPERATION
        };

        static void Main(string[] args)
        {
            if (args.Length != STANDARD_ARGS_COUNT)
            {
                throw new ArgumentException("Incorrect argument count");
            }

            var operationName = args[0].Trim().ToUpper();
            if (!OperationList.Contains(operationName))
            {
                throw new ArgumentException("Incorrect operation name");
            }

            var isCompressOperation = operationName == COMPRESS_OPERATION;
            var operationType = isCompressOperation
                ? CompressionMode.Compress
                : CompressionMode.Decompress;

            var inputFilePath = args[1].Trim();
            if (!File.Exists(inputFilePath))
            {
                throw new ArgumentException("Specified as input file is not existed");
            }

            var outputFilePath = args[2].Trim();
            if (!Directory.Exists(Path.GetDirectoryName(outputFilePath)))
            {
                throw new ArgumentException("Specified as output directory is not existed");
            }

            if (File.Exists(outputFilePath))
            {
                throw new ArgumentException("Specified as output file is already existed");
            }

            var destinationDrive = DriveInfo.GetDrives()
                .Single(drive => drive.RootDirectory.Name == Path.GetPathRoot(outputFilePath));

            using (var inputFileStream = File.OpenRead(inputFilePath))
            {
                if (isCompressOperation && inputFileStream.IsCompressed())
                {
                    throw new Exception("Specified as input file is already compressed");
                }

                if (!isCompressOperation && !inputFileStream.IsCompressed())
                {
                    throw new Exception("Specified as input file is not compressed");
                }

                if (destinationDrive.AvailableFreeSpace < inputFileStream.Length)
                {
                    throw new Exception("Specified as output directory have not enough free space");
                }
            }

            AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;

            InitializationLogic.InitializeWorkers(operationType, inputFilePath, outputFilePath);
        }

        private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            var message = e.ExceptionObject.ToString();

            InitializationLogic.HandleException((Exception) e.ExceptionObject, message);
        }
    }
}