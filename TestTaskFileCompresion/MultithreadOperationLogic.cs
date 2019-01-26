using System.IO;
using System.Threading;

namespace TestTaskFileCompression
{
    public abstract class MultithreadOperationLogic
    {
        private readonly Semaphore semaphore;

        protected readonly Stream inFileStream;

        protected MultithreadOperationLogic(string inputFilePath)
        {
            inFileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            var procCount = SystemSettingMonitor.Instance.ProcessorCount;
            semaphore = new Semaphore(procCount, procCount);
        }

        public void Call()
        {
            var partIndex = 0;

            while (true)
            {
                semaphore.WaitOne();

                var readCount = ReadInputAndOperate(inFileStream, partIndex);
                if (readCount == 0)
                {
                    ScheduledWriter.Instance.SetIsStreamSliced();

                    inFileStream.Close();
                    return;
                }

                partIndex++;

                ScheduledWriter.Instance.IncrementPartCount();

                semaphore.Release();
            }
        }

        private int ReadInputAndOperate(Stream inFileStream, int partIndex)
        {
            var inPartStream = new MemoryStream();

            var length = GetReadLength();
            var buffer = new byte[length];
            var readCount = inFileStream.CopyTo(inPartStream, buffer, 0, length);

            inPartStream.Seek(0, SeekOrigin.Begin);

            var outPartStream = new MemoryStream();

            var parameters = GetOperationParameters(inPartStream, outPartStream, partIndex);

            StartThreadWorker(parameters);

            return readCount;
        }

        private static void StartThreadWorker(OperationParameters parameters)
        {
            var thread = new Thread(parameters.StartWorker);
            thread.Start();
            thread.Join();
        }

        protected abstract OperationParameters GetOperationParameters(
            Stream inPartStream,
            Stream outPartStream,
            int partIndex);

        protected abstract int GetReadLength();
    }
}