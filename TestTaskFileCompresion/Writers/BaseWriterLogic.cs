using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;

using TestTaskFileCompression.Common;

namespace TestTaskFileCompression.Writers
{
    public abstract class BaseWriterLogic
    {
        public Action Clear;

        public Action<StreamResult> Remove;

        public Func<int, bool> IsNotEnded;
        public Func<List<StreamResult>> GetQueue;

        private readonly string outputFilePath;
        
        protected BaseWriterLogic(string outputFilePath)
        {
            this.outputFilePath = outputFilePath;
        }

        public void Call()
        {
            try
            {
                new Thread(StartWriterWorker).Start(AppConstants.SLEEP_TIMEOUT);
            }
            catch (Exception e)
            {
                var errorMessage = "Exception in writer worker: " + e.Message;
                Console.WriteLine(errorMessage);
            }
            finally
            {
                //TODO: try again
            }
        }

        private void StartWriterWorker(object obj)
        {
            var millisecondsToSleep = (int)obj;

            var outFileStream = File.Create(outputFilePath);
           
            SignOutStream(outFileStream);

            var wrotePartCount = 0;
            while (IsNotEnded(wrotePartCount))
            {
                var tempQueue = GetQueue();

                var isNextPartExist = tempQueue
                    .Any(item => item.PartIndex == wrotePartCount);

                if (isNextPartExist)
                {
                    var nextPart = tempQueue
                        .First(item => item.PartIndex == wrotePartCount);

                    var resultStream = nextPart.ResultStream;

                    InsertPartStreamInfo(outFileStream, (int)resultStream.Length);

                    resultStream.Seek(0, SeekOrigin.Begin);
                    var buffer = new byte[resultStream.Length];
                    resultStream.CopyTo(outFileStream, buffer, 0, buffer.Length);

                    resultStream.Close();

                    Remove(nextPart);

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

        protected abstract void SignOutStream(Stream outFileStream);

        protected abstract void InsertPartStreamInfo(Stream outFileStream, object info);
    }
}