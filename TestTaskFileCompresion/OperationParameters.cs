using System;
using System.IO;

namespace TestTaskFileCompression
{
    public abstract class OperationParameters
    {
        private readonly int partIndex;

        protected readonly Stream inStream;
        protected readonly Stream outStream;

        protected readonly byte[] buffer;

        protected OperationParameters(Stream inStream, Stream outStream, int partIndex)
        {
            this.inStream = inStream;
            this.outStream = outStream;

            this.partIndex = partIndex;

            buffer = new byte[inStream.Length];
        }

        public void StartWorker()
        {
            try
            {
                StartOperation();
                ScheduledWriter.Instance.SetNewResult(new ZipResult(partIndex, outStream));
            }
            catch (Exception e)
            {
                var errorMessage = "Exception in operation worker: " + e.Message;
                Console.WriteLine(errorMessage);
            }
        }

        protected abstract void StartOperation();
    }
}