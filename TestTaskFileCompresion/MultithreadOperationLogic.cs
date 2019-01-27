using System;
using System.IO;
using System.Threading;

namespace TestTaskFileCompression
{
    public abstract class MultithreadOperationLogic
    {
        public Action SetIsStreamSliced;
        public Action IncrementPartCount;

        public Action<ZipResult> SetNewResult;

        public Func<int, Stream> GetNewStream;
        public Func<int> GetProcessorCount;

        private Semaphore semaphore;

        protected readonly Stream inFileStream;

        protected MultithreadOperationLogic(string inputFilePath)
        {
            inFileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

        }

        public void Call()
        {
            var procCount = GetProcessorCount();
            semaphore = new Semaphore(procCount, procCount);

            var partIndex = 0;

            while (true)
            {
                semaphore.WaitOne();

                var readCount = ReadInputAndOperate(inFileStream, partIndex);
                if (readCount == 0)
                {
                    SetIsStreamSliced();

                    inFileStream.Close();
                    return;
                }

                partIndex++;

                IncrementPartCount();

                semaphore.Release();
            }
        }

        private int ReadInputAndOperate(Stream inStream, int partIndex)
        {
            var length = GetReadLength();
            var buffer = new byte[length];

            var inPartStream = GetNewStream(length);
            var outPartStream = GetNewStream(length);
            
            var readCount = inStream.CopyTo(inPartStream, buffer, 0, length);

            inPartStream.Seek(0, SeekOrigin.Begin);
            
            var parameters = GetOperationParameters(inPartStream, outPartStream, partIndex);

            parameters.SetNewResult = SetNewResult;
            
            StartThreadWorker(parameters);

            return readCount;
        }

        private static void StartThreadWorker(OperationParameters parameters)
        {
            new Thread(parameters.StartWorker).Start();
        }

        protected abstract OperationParameters GetOperationParameters(
            Stream inPartStream,
            Stream outPartStream,
            int partIndex);

        protected abstract int GetReadLength();
    }
}