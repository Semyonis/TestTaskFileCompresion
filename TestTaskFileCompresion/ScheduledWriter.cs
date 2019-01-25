using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

namespace TestTaskFileCompression
{
    public sealed class ScheduledWriter
    {
        private static volatile object instanceMutex = new object();

        private static ScheduledWriter instance;

        private readonly List<ZipResult> queue;

        private bool isStreamSliceFinished;

        private int totalPartCount;

        private string outputFilePath;
        private byte[] buffer;

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

        public void StartWriter() { StartWriter(1000); }

        public void StartWriter(int millisecondsToSleep)
        {
            var outFileStream = File.Create(outputFilePath);

            var wrotePartCount = 0;
            while (!isStreamSliceFinished && totalPartCount > 0 || wrotePartCount < totalPartCount)
            {
                var isNextPartExist = queue
                    .Any(item => item.PartIndex == wrotePartCount);

                if (isNextPartExist)
                {
                    var nextPart = queue
                        .First(item => item.PartIndex == wrotePartCount);
                    
                    buffer = new byte[nextPart.InputStream.Length];
                    nextPart.InputStream.Seek(0, SeekOrigin.Begin);
                    nextPart.InputStream.CopyTo(outFileStream, buffer, 0, buffer.Length);
                    buffer = null;

                    wrotePartCount++;
                }
                else
                {
                    Thread.Sleep(millisecondsToSleep);
                }
            }

            outFileStream.Close();
        }

        public void SetNewResult(ZipResult result) { queue.Add(result); }

        public void SetOutputFile(string outFile) { outputFilePath = outFile; }

        public void SetIsStreamSliced() { isStreamSliceFinished = true; }

        public void IncrementPartCount() { totalPartCount++; }
    }
}