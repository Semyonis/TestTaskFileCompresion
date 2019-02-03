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

        private Stream outputStream;

        public void Call(WriterService writerService, string outputFilePath)
        {
            service = writerService;

            outputStream = File.Create(outputFilePath);

            new Thread(WriterWorkerStart).Start();
        }

        private void WriterWorkerStart()
        {
            try
            {
                SignOutputStream(outputStream);

                while (service.IsNotEnded)
                {
                    if (service.Token.IsCancellationRequested)
                    {
                        return;
                    }

                    var nextPart = service.GetNextPart();

                    var resultStream = nextPart.ResultStream;

                    InsertPartStreamInfo(outputStream, (int) resultStream.Length);

                    resultStream.Seek(0, SeekOrigin.Begin);
                    var buffer = new byte[resultStream.Length];
                    resultStream.CopyTo(outputStream, buffer, 0, buffer.Length);

                    resultStream.Close();
                }

                outputStream.Close();

                service.Clear();
            }
            catch (Exception e)
            {
                var errorMessage = "Exception in writer worker: " + e.Message;

                service.HandleException(e, errorMessage);
            }
        }

        protected abstract void SignOutputStream(Stream outFileStream);

        protected abstract void InsertPartStreamInfo(Stream outFileStream, object info);
    }
}