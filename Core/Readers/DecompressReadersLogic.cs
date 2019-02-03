using System;
using System.IO;

using Core.Services;

namespace Core.Readers
{
    public sealed class DecompressReadersLogic : BaseReadersLogic
    {
        public DecompressReadersLogic(ReaderService service, string inputFilePath)
            : base(service, inputFilePath)
        {
            SeekStart(inputStream);
        }

        protected override BaseReader GetOperationParameters(
            ReaderService service, 
            Stream inPartStream,
            Stream outPartStream,
            int partIndex)
        {
            return new DecompressionReader(service, inPartStream, outPartStream, partIndex);
        }

        protected override int GetCountToRead()
        {
            var array = new byte[4];
            inputStream.Read(array, 0, 4);
            return BitConverter.ToInt32(array, 0);
        }

        private static void SeekStart(Stream stream) { stream.Seek(4, SeekOrigin.Begin); }
    }
}