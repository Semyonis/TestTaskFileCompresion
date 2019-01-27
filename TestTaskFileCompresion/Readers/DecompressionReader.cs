using System.IO;

using TestTaskFileCompression.Common;

namespace TestTaskFileCompression.Readers
{
    public sealed class DecompressionReader : BaseReader
    {
        public DecompressionReader(Stream inStream, Stream outStream, int partIndex)
            : base(inStream, outStream, partIndex) { }

        protected override void StartOperation() { Decompress(); }

        private void Decompress()
        {
            var buffer = new byte[inStream.Length];
            inStream.DecompressToStream(outStream, buffer, 0, buffer.Length);
        }
    }
}