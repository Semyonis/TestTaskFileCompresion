using System.IO;

using TestTaskFileCompression.Common;

namespace TestTaskFileCompression.Readers
{
    public sealed class CompressionReader : BaseReader
    {
        public CompressionReader(Stream inStream, Stream outStream, int partIndex)
            : base(inStream, outStream, partIndex) { }

        protected override void StartOperation() { Compress(); }

        private void Compress()
        {
            var buffer = new byte[inStream.Length];
            inStream.CompressToStream(outStream, buffer, 0, buffer.Length);
        }
    }
}