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
            var writerThread = new Thread(ScheduledWriter.Instance.StartWriter);
            writerThread.Start();

            using (var inFileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                var readTotal = 0l;

                int partIndex = 0;
                while (true)
                {
                    semaphore.WaitOne();

                    ScheduledWriter.Instance.IncrementPartCount();

                    var inPartStream = new MemoryStream();

                    var count = (int) (inFileStream.Length / procCount);
                    var buffer = new byte[count];

                    var readCount = inFileStream.CopyTo(inPartStream, buffer, 0, count);

                    inPartStream.Seek(0, SeekOrigin.Begin);

                    var outStream = new MemoryStream();
                    var parameters = new OperationParameters(inPartStream, outStream, operationType, partIndex);
                    var start = new ThreadStart(parameters.CallOperation);

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