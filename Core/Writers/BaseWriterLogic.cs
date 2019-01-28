using System;
using System.IO;
using System.Threading;

using Core.Common;

namespace Core.Writers
{
    public abstract class BaseWriterLogic
    {
        public Action Clear;

        public Action<StreamResult> Remove;

        public Action<Exception, string> HandleException;

        public Func<int, bool> IsNotEnded;

        public Func<int, StreamResult> GetPartById;

        private readonly string outputFilePath;

        protected BaseWriterLogic(string outputFilePath) { this.outputFilePath = outputFilePath; }

        public void Call() { new Thread(WriterWorkerStart).Start(); }

        private void WriterWorkerStart()
        {
            try
            {
                var outFileStream = File.Create(outputFilePath);

                SignOutStream(outFileStream);

                var wrotePartCount = 0;

                var isNotEnded = false;
                var notEnded = IsNotEnded;
                if (notEnded != null)
                {
                    isNotEnded = notEnded(wrotePartCount);
                }

                while (isNotEnded)
                {
                    StreamResult nextPart = null;

                    var getById = GetPartById;
                    if (getById != null)
                    {
                        nextPart = getById(wrotePartCount);
                    }

                    if(nextPart !=null)
                    { 
                        var resultStream = nextPart.ResultStream;

                        InsertPartStreamInfo(outFileStream, (int) resultStream.Length);

                        resultStream.Seek(0, SeekOrigin.Begin);
                        var buffer = new byte[resultStream.Length];
                        resultStream.CopyTo(outFileStream, buffer, 0, buffer.Length);

                        resultStream.Close();

                        var remove = Remove;
                        if (remove != null)
                        {
                            remove(nextPart);
                        }

                        wrotePartCount++;
                    }
                    else
                    {
                        Thread.Sleep(AppConstants.SLEEP_TIMEOUT);
                    }

                    notEnded = IsNotEnded;
                    if (notEnded != null)
                    {
                        isNotEnded = notEnded(wrotePartCount);
                    }
                }

                outFileStream.Close();

                var clear = Clear;
                if (clear != null)
                {
                    clear();
                }
            }
            catch (Exception e)
            {
                var errorMessage = "Exception in writer worker: " + e.Message;

                var handle = HandleException;
                if (handle != null)
                {
                    handle(e, errorMessage);
                }
            }
            finally
            {
                //TODO: try write again
            }
        }

        protected abstract void SignOutStream(Stream outFileStream);

        protected abstract void InsertPartStreamInfo(Stream outFileStream, object info);
    }
}