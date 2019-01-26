using System.IO;

namespace TestTaskFileCompression
{
    public sealed class MultithreadCompressLogic : MultithreadOperationLogic
    {
        public MultithreadCompressLogic(string inputFilePath) : base(inputFilePath) { }

        protected override OperationParameters GetOperationParameters(Stream inPartStream,
            Stream outPartStream,
            int partIndex)
        {
            return new CompressionParameters(inPartStream, outPartStream, partIndex);
        }

        protected override int GetReadLength() { return 1000000; }
    }
}