using System;
using System.IO;
using System.IO.Compression;
using System.Threading;

namespace TestTaskFileCompression
{
    public struct OperationLogic
    {
        private readonly Semaphore semaphore;

        private readonly string inputFilePath;

        private readonly CompressionMode operationType;
        private readonly int procCount;

        public OperationLogic(CompressionMode operationType, string inputFilePath, string outputFilePath)
        {
            this.operationType = operationType;

            this.inputFilePath = inputFilePath;

            procCount = SystemSettingMonitor.Instance.ProcessorCount;
            semaphore = new Semaphore(procCount, procCount);

            ScheduledWriter.Instance.SetOutputFile(outputFilePath);
        }

        public void Call()
        {
            var writerThread = new Thread(ScheduledWriter.Instance.StartWorker);
            writerThread.Start();

            using (var inFileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var readTotal = 0L;

                var partIndex = 0;

                while (true)
                {
                    semaphore.WaitOne();

                    ScheduledWriter.Instance.IncrementPartCount();
                    ScheduledWriter.Instance.SetOperation(operationType);

                    var inPartStream = new MemoryStream();

                    int count;
                    if (operationType == CompressionMode.Decompress)
                    {
                        var array = new byte[4];
                        inFileStream.Read(array, 0, 4);
                        count = BitConverter.ToInt32(array, 0);
                    }
                    else
                    {
                        count = (int)( inFileStream.Length / procCount );
                    }

                    var buffer = new byte[count];

                    var readCount = inFileStream.CopyTo(inPartStream, buffer, 0, count);

                    inPartStream.Seek(0, SeekOrigin.Begin);

                    var outPartStream = new MemoryStream();

                    var parameters = new OperationParameters(operationType, inPartStream, outPartStream, partIndex);
                    var start = new ThreadStart(parameters.StartWorker);

                    var thread = new Thread(start);

                    thread.Start();

                    thread.Join();

                    readTotal += readCount;

                    partIndex++;

                    semaphore.Release();

                    if (readTotal == inFileStream.Length)
                    {
                        ScheduledWriter.Instance.SetIsStreamSliced();
                        break;
                    }
                }
            }

            writerThread.Join();
        }
    }
}