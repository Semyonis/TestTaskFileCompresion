using System;
using System.IO;

using Core.Common;

namespace Core.Readers
{
    public abstract class BaseReader
    {
        public Action<Exception, string> HandleException;

        public Action<StreamResult> PutInQueue;

        private readonly int partIndex;

        protected readonly Stream inStream;
        protected readonly Stream outStream;

        protected BaseReader(Stream inStream, Stream outStream, int partIndex)
        {
            this.inStream = inStream;
            this.outStream = outStream;

            this.partIndex = partIndex;
        }

        public void StartWorker()
        {
            try
            {
                StartOperation();

                inStream.Close();

                var put = PutInQueue;
                if (put != null)
                {
                    put(new StreamResult(partIndex, outStream));
                }
            }
            catch (Exception e)
            {
                var errorMessage = "Exception in operation worker: " + e.Message;

                var handle = HandleException;
                if (handle != null)
                {
                    handle(e, errorMessage);
                }
            }
            finally
            {
                //TODO: try read again
            }
        }

        protected abstract void StartOperation();
    }
}