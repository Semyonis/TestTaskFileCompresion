using System.IO;

using Core.Common;
using Core.Services;

namespace Core.Readers
{
    public sealed class CompressReadersLogic : BaseReadersLogic
    {
        protected override BaseReader GetOperationParameters(
            ReaderService service,
            Stream inPartStream,
            Stream outPartStream,
            int partIndex)
        {
            return new CompressionReader(service, inPartStream, outPartStream, partIndex);
        }

        protected override int GetCountToRead(Stream stream) { return AppConstants.COMPRESS_READ_LENGTH; }

        protected override void SeekStart(Stream stream) { }
    }
}