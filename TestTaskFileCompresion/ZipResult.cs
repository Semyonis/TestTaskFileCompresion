using System.IO;

namespace TestTaskFileCompression
{
    public sealed class ZipResult
    {
        public ZipResult(int partIndex, Stream outStream)
        {
            PartIndex = partIndex;
            ResultStream = outStream;
        }

        public int PartIndex { get; private set; }

        public Stream ResultStream { get; private set; }
    }
}