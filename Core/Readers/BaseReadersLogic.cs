using System;
using System.IO;
using System.Threading;

using Core.Common;
using Core.Tokens;

namespace Core.Readers
{
    public abstract class BaseReadersLogic
    {
        public Action<Exception, string> HandleException;

        public Action SetInputStreamIsSliced;
        public Action IncrementPartCount;

        public Action<StreamResult> Put;

        public Func<int, Stream> GetNewStream;
        public Func<int> GetProcessorCount;

        private Semaphore semaphore;

        protected readonly Stream inFileStream;

        protected BaseReadersLogic(string inputFilePath)
        {
            inFileStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public CancellationToken Token { get; set; }

        public void Call()
        {
            var procCount = 1;
            var getProcessorCount = GetProcessorCount;
            if (getProcessorCount != null)
            {
                procCount = getProcessorCount();
            }

            semaphore = new Semaphore(procCount, procCount);

            var partIndex = 0;
            while (true)
            {
                if (Token.IsCancellationRequested)
                {
                    return;
                }

                semaphore.WaitOne();

                var length = GetReadLength();

                Stream inPartStream = null;
                var getNewStream = GetNewStream;
                if (getNewStream != null)
                {
                    inPartStream = getNewStream(length);
                }

                var readCount = inFileStream.CopyTo(inPartStream, new byte[length], 0, length);

                if (readCount == 0)
                {
                    var setStreamIsSliced = SetInputStreamIsSliced;
                    if (setStreamIsSliced != null)
                    {
                        setStreamIsSliced();
                    }

                    inFileStream.Close();

                    return;
                }

                inPartStream.Seek(0, SeekOrigin.Begin);

                var readerParameters = GetParameters(inPartStream, readCount, partIndex);

                ReaderThreadStart(readerParameters);

                partIndex++;

                var incrementPartCount = IncrementPartCount;
                if (incrementPartCount != null)
                {
                    incrementPartCount();
                }

                semaphore.Release();
            }
        }

        private BaseReader GetParameters(Stream inPartStream, int length, int partIndex)
        {
            var outPartStream = GetNewStream(length);

            var parameters = GetOperationParameters(inPartStream, outPartStream, partIndex);

            parameters.HandleException = HandleException;
            parameters.PutInQueue = Put;

            return parameters;
        }

        private static void ReaderThreadStart(BaseReader parameters) { new Thread(parameters.StartWorker).Start(); }

        protected abstract BaseReader GetOperationParameters(
            Stream inPartStream,
            Stream outPartStream,
            int partIndex);

        protected abstract int GetReadLength();
    }
}