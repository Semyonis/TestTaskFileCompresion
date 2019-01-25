using System.IO;
using System.IO.Compression;

namespace TestTaskFileCompression
{
    public struct OperationParameters
    {
        private readonly Stream inStream;
        private readonly Stream outStream;

        private readonly CompressionMode operationType;
        private readonly int partIndex;
        private readonly byte[] buffer;

        public OperationParameters(Stream inStream, Stream outStream, CompressionMode operationType, int partIndex)
        {
            this.inStream = inStream;
            this.outStream = outStream;
            this.operationType = operationType;
            this.partIndex = partIndex;

            var length = (int) inStream.Length;
            buffer = new byte[length];
        }

        public void CallOperation()
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
            var offset = 0;
            var length = 32 * 1024;
            var buffer = new byte[length];

            inStream.DecompressToStream(outStream, buffer, offset, length);
        }
    }
}