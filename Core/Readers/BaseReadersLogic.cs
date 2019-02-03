using System;
using System.IO;
using System.Threading;

using Core.Common;
using Core.Services;

namespace Core.Readers
{
    public abstract class BaseReadersLogic
    {
        private static volatile object mutex = new object();

        private readonly ReaderService service;

        protected readonly Stream inputStream;

        private int globalPartIndex;

        protected BaseReadersLogic(ReaderService readerService, string inputFilePath)
        {
            service = readerService;

            inputStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);
        }

        public void Call()
        {
            globalPartIndex = 0;

            var procCount = service.GetProcessorCount();
            for (var index = 0; index < procCount - 1; index++)
            {
                new Thread(ReaderWorkerStart).Start();
            }
        }

        private void ReaderWorkerStart()
        {
            try
            {
                while (true)
                {
                    if (service.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    int localPartIndex;
                    int readByteCount;
                    Stream partOfInputStream;
                    lock (mutex)
                    {
                        if (service.InputStreamIsSliced)
                        {
                            return;
                        }

                        localPartIndex = globalPartIndex;

                        var countToRead = GetCountToRead();

                        partOfInputStream = service.GetNewStream(countToRead);

                        readByteCount = inputStream.CopyTo(partOfInputStream, new byte[countToRead], 0, countToRead);

                        if (readByteCount == 0)
                        {
                            service.SetInputStreamIsSliced();

                            inputStream.Close();

                            return;
                        }

                        globalPartIndex++;
                    }

                    service.IncrementReadCount();

                    partOfInputStream.Seek(0, SeekOrigin.Begin);

                    GetParameters(partOfInputStream, readByteCount, localPartIndex)
                        .StartWorker();
                }
            }
            catch (Exception e)
            {
                var errorMessage = "Exception in writer worker: " + e.Message;

                service.HandleException(e, errorMessage);
            }
        }

        private BaseReader GetParameters(Stream inPartStream, int length, int partIndex)
        {
            var outPartStream = service.GetNewStream(length);

            return GetOperationParameters(service, inPartStream, outPartStream, partIndex);
        }

        protected abstract BaseReader GetOperationParameters(
            ReaderService service,
            Stream inPartStream,
            Stream outPartStream,
            int partIndex);

        protected abstract int GetCountToRead();
    }
}