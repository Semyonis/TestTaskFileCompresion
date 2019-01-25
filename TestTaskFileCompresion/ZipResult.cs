using System.IO;

namespace TestTaskFileCompression
{
    public sealed class ZipResult
    {
        public ZipResult(int partIndex, Stream outStream)
        {
            PartIndex = partIndex;
            InputStream = outStream;
        }

        public int PartIndex { get; private set; }

        public Stream InputStream { get; private set; }
    }
}