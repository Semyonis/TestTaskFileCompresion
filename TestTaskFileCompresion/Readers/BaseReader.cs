using System;
using System.IO;

using TestTaskFileCompression.Common;

namespace TestTaskFileCompression.Readers
{
    public abstract class BaseReader
    {
        public Action<StreamResult> SetNewResult;

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

                SetNewResult(new StreamResult(partIndex, outStream));
            }
            catch (Exception e)
            {
                var errorMessage = "Exception in operation worker: " + e.Message;
                Console.WriteLine(errorMessage);
            }
            finally
            {
                //TODO: try again
            }
        }

        protected abstract void StartOperation();
    }
}