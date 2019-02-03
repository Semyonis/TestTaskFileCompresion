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

        private int globalPartIndex;

        private ReaderService service;

        private Stream inputStream;

        public void Call(ReaderService readerService, string inputFilePath)
        {
            globalPartIndex = 0;

            service = readerService;

            inputStream = new FileStream(inputFilePath, FileMode.Open, FileAccess.Read, FileShare.Read);

            SeekStart(inputStream);

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

                        var countToRead = GetCountToRead(inputStream);

                        partOfInputStream = service.GetNewStream(countToRead);

                        readByteCount = inputStream
                            .CopyTo(partOfInputStream, new byte[countToRead], 0, countToRead);

                        if (readByteCount == 0)
                        {
                            service.InputStreamIsSliced = true;

                            inputStream.Close();

                            return;
                        }

                        service.IncrementReadPartCount();

                        localPartIndex = globalPartIndex;

                        globalPartIndex++;
                    }

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

        protected abstract int GetCountToRead(Stream stream);

        protected abstract void SeekStart(Stream stream);
    }
}