using System.IO;

namespace TestTaskFileCompression
{
    public struct ZipResult
    {
        private readonly int index;
        private readonly Stream outStream;

        public ZipResult(int partIndex, Stream outStream)
        {
            index = partIndex;
            this.outStream = outStream;
        }

        public int PartIndex
        {
            get { return index; }
        }

        public Stream ResultStream
        {
            get { return outStream; }
        }
    }
}