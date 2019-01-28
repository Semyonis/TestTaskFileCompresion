using System.IO;

using Core.Common;

namespace Core.Readers
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

        protected override int GetReadLength() { return AppConstants.COMPRESS_READ_LENGTH; }
    }
}