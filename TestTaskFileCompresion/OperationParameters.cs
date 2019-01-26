using System;
using System.IO;
using System.IO.Compression;

namespace TestTaskFileCompression
{
    public struct OperationParameters
    {
        private readonly CompressionMode operationType;

        private readonly Stream inStream;
        private readonly Stream outStream;
        
        private readonly int partIndex;

        private readonly byte[] buffer;

        public OperationParameters(CompressionMode operationType, Stream inStream, Stream outStream, int partIndex)
        {
            this.operationType = operationType;

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
            }
            catch (Exception e)
            {
                var errorMessage = "Unhandled exception in operation worker: " + e.Message;
                Console.WriteLine(e);
            }
        }

        private void StartOperation()
        {
            if (operationType == CompressionMode.Compress)
            {
                Compress();
            }
            else
            {
                Decompress();
            }
        }

        private void Compress()
        {
            inStream.CompressToStream(outStream, buffer, 0, buffer.Length);

            ScheduledWriter.Instance.SetNewResult(new ZipResult(partIndex, outStream));
        }

        private void Decompress()
        {
            inStream.DecompressToStream(outStream, buffer, 0, buffer.Length);

            ScheduledWriter.Instance.SetNewResult(new ZipResult(partIndex, outStream));
        }
    }
}