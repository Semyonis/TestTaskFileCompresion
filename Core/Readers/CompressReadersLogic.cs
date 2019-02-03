using System.IO;

using Core.Common;
using Core.Services;

namespace Core.Readers
{
    public sealed class CompressReadersLogic : BaseReadersLogic
    {
        public CompressReadersLogic(ReaderService service, string inputFilePath)
            : base(service, inputFilePath) { }

        protected override BaseReader GetOperationParameters(
            ReaderService service,
            Stream inPartStream,
            Stream outPartStream,
            int partIndex)
        {
            return new CompressionReader(service, inPartStream, outPartStream, partIndex);
        }

        protected override int GetCountToRead() { return AppConstants.COMPRESS_READ_LENGTH; }
    }
}