using System.IO;

namespace TestTaskFileCompression
{
    public sealed class DecompressionParameters : OperationParameters
    {
        public DecompressionParameters(Stream inStream, Stream outStream, int partIndex)
            : base(inStream, outStream, partIndex) { }

        protected override void StartOperation() { Decompress(); }

        private void Decompress()
        {
            var buffer = new byte[inStream.Length];
            inStream.DecompressToStream(outStream, buffer, 0, buffer.Length);
        }
    }
}