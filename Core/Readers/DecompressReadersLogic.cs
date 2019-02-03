using System;
using System.IO;

using Core.Services;

namespace Core.Readers
{
    public sealed class DecompressReadersLogic : BaseReadersLogic
    {
        protected override BaseReader GetOperationParameters(
            ReaderService service, 
            Stream inPartStream,
            Stream outPartStream,
            int partIndex)
        {
            return new DecompressionReader(service, inPartStream, outPartStream, partIndex);
        }

        protected override int GetCountToRead(Stream stream)
        {
            var array = new byte[4];
            stream.Read(array, 0, 4);
            return BitConverter.ToInt32(array, 0);
        }

        protected override void SeekStart(Stream stream) { stream.Seek(4, SeekOrigin.Begin); }
    }
}