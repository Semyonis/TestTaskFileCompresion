using System;
using System.IO;

namespace TestTaskFileCompression.Readers
{
    public sealed class DecompressReadersLogic : BaseReadersLogic
    {
        public DecompressReadersLogic(string inputFilePath)
            : base(inputFilePath)
        {
            SeekStart(inFileStream);
        }

        protected override BaseReader GetOperationParameters(Stream inPartStream,
            Stream outPartStream,
            int partIndex)
        {
            return new DecompressionReader(inPartStream, outPartStream, partIndex);
        }

        protected override int GetReadLength()
        {
            var array = new byte[4];
            inFileStream.Read(array, 0, 4);
            return BitConverter.ToInt32(array, 0);
        }

        private static void SeekStart(Stream inFileStream) { inFileStream.Seek(4, SeekOrigin.Begin); }
    }
}