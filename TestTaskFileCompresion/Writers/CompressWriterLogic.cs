using System;
using System.IO;

using TestTaskFileCompression.Common;

namespace TestTaskFileCompression.Writers
{
    public sealed class CompressWriterLogic : BaseWriterLogic
    {
        public CompressWriterLogic(string outputFilePath) : base(outputFilePath) { }

        protected override void InsertPartStreamInfo(Stream outFileStream, object info)
        {
            var length = (int) info;
            WriteIntToStream(outFileStream, length);
        }

        protected override void SignOutStream(Stream outFileStream)
        {
            WriteIntToStream(outFileStream, AppConstants.FORMAT_START_CHARS);
        }

        private static void WriteIntToStream(Stream outFileStream, int length)
        {
            var bytes = BitConverter.GetBytes(length);
            outFileStream.Write(bytes, 0, 4);
        }
    }
}