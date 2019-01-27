using System.IO;

using TestTaskFileCompression.Common;

namespace TestTaskFileCompression.Readers
{
    public sealed class CompressReadersLogic : BaseReadersLogic
    {
        public CompressReadersLogic(string inputFilePath) : base(inputFilePath) { }

        protected override BaseReader GetOperationParameters(Stream inPartStream,
            Stream outPartStream,
            int partIndex)
        {
            return new CompressionReader(inPartStream, outPartStream, partIndex);
        }

        protected override int GetReadLength() { return AppConstants.BYTE_IN_MEGABYTE; }
    }
}