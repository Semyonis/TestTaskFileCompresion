using System.IO;

namespace TestTaskFileCompression
{
    public sealed class CompressionParameters : OperationParameters
    {
        public CompressionParameters(Stream inStream, Stream outStream, int partIndex)
            : base(inStream, outStream, partIndex) { }

        protected override void StartOperation() { Compress(); }

        private void Compress()
        {
            var buffer = new byte[inStream.Length];
            inStream.CompressToStream(outStream, buffer, 0, buffer.Length);
        }
    }
}