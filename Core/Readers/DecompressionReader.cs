using System.IO;

using Core.Common;
using Core.Services;

namespace Core.Readers
{
    public sealed class DecompressionReader : BaseReader
    {
        public DecompressionReader(ReaderService service, Stream inStream, Stream outStream, int partIndex)
            : base(service, inStream, outStream, partIndex) { }

        protected override void StartOperation() { Decompress(); }

        private void Decompress()
        {
            var buffer = new byte[inStream.Length];
            inStream.DecompressToStream(outStream, buffer, 0, buffer.Length);
        }
    }
}