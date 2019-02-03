using System;
using System.IO;
using System.Threading;

using Core.Common;
using Core.Services;

namespace Core.Writers
{
    public abstract class BaseWriterLogic
    {
        private WriterService service;

        private Stream outFileStream;

        public void Call(WriterService writerService, string outputFilePath)
        {
            service = writerService;

            outFileStream = File.Create(outputFilePath);

            new Thread(WriterWorkerStart).Start();
        }

        private void WriterWorkerStart()
        {
            try
            {

                SignOutStream(outFileStream);

                while (service.IsNotEnded)
                {
                    if (service.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    var nextPart = service.GetNextPart();

                    var resultStream = nextPart.ResultStream;

                    InsertPartStreamInfo(outFileStream, (int) resultStream.Length);

                    resultStream.Seek(0, SeekOrigin.Begin);
                    var buffer = new byte[resultStream.Length];
                    resultStream.CopyTo(outFileStream, buffer, 0, buffer.Length);

                    resultStream.Close();

                    service.Remove(nextPart);

                    service.IncrementWriteCount();
                }

                outFileStream.Close();

                service.Clear();
            }
            catch (Exception e)
            {
                var errorMessage = "Exception in writer worker: " + e.Message;

                service.HandleException(e, errorMessage);
            }
        }

        protected abstract void SignOutStream(Stream outFileStream);

        protected abstract void InsertPartStreamInfo(Stream outFileStream, object info);
    }
}