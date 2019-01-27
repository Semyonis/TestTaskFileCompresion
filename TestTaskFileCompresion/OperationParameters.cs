using System;
using System.IO;

namespace TestTaskFileCompression
{
    public abstract class OperationParameters
    {
        public Action<ZipResult> SetNewResult;

        private readonly int partIndex;

        protected readonly Stream inStream;
        protected readonly Stream outStream;

        protected OperationParameters(Stream inStream, Stream outStream, int partIndex)
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

                SetNewResult(new ZipResult(partIndex, outStream));
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