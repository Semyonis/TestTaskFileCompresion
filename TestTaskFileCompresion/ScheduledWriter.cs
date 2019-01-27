using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Threading;

namespace TestTaskFileCompression
{
    public sealed class ScheduledWriter
    {
        public Action Clear;

        private static volatile object instanceMutex = new object();

        private static ScheduledWriter instance;

        private readonly List<ZipResult> queue;

        private bool isStreamSliceFinished;

        private int totalPartCount;

        private string outputFilePath;
        private CompressionMode mode;

        private ScheduledWriter()
        {
            queue = new List<ZipResult>();

            isStreamSliceFinished = false;

            totalPartCount = 0;
        }

        public static ScheduledWriter Instance
        {
            get
            {
                lock (instanceMutex)
                {
                    return instance ?? (instance = new ScheduledWriter());
                }
            }
        }

        public void Initialize(CompressionMode operationType, string outputFilePath)
        {
            mode = operationType;
            this.outputFilePath = outputFilePath;
        }

        public void SetNewResult(ZipResult result)
        {
            lock (queue)
            {
                queue.Add(result);
            }
        }

        public void SetIsStreamSliced() { isStreamSliceFinished = true; }

        public void IncrementPartCount() { totalPartCount++; }

        public void StartWorker()
        {
            try
            {
                StartWriterWorker(1000);
            }
            catch (Exception e)
            {
                var errorMessage = "Exception in writer worker: " + e.Message;
                Console.WriteLine(errorMessage);
            }
        }

        private void StartWriterWorker(int millisecondsToSleep)
        {
            var outFileStream = File.Create(outputFilePath);
            if (mode == CompressionMode.Compress)
            {
                var bytes = BitConverter.GetBytes((int) 666);
                outFileStream.Write(bytes, 0, 4);
            }

            var wrotePartCount = 0;
            while (wrotePartCount < totalPartCount || !isStreamSliceFinished)
            {
                bool isNextPartExist;
                var nextPart = new ZipResult(0, null);
                lock (queue)
                {
                    isNextPartExist = queue
                        .Any(item => item.PartIndex == wrotePartCount);

                    if (isNextPartExist)
                    {
                        nextPart = queue
                            .First(item => item.PartIndex == wrotePartCount);
                    }
                }

                if (isNextPartExist)
                {
                    var resultStream = nextPart.ResultStream;

                    if (mode == CompressionMode.Compress)
                    {
                        var bytes = BitConverter.GetBytes((int) resultStream.Length);
                        outFileStream.Write(bytes, 0, 4);
                    }

                    resultStream.Seek(0, SeekOrigin.Begin);
                    var buffer = new byte[resultStream.Length];
                    resultStream.CopyTo(outFileStream, buffer, 0, buffer.Length);

                    resultStream.Close();

                    lock (queue)
                    {
                        queue.Remove(nextPart);
                    }

                    wrotePartCount++;
                }
                else
                {
                    Thread.Sleep(millisecondsToSleep);
                }
            }

            outFileStream.Close();

            Clear();
        }
    }
}