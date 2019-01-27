﻿using System.IO;

namespace TestTaskFileCompression.Writers
{
    public sealed class DecompressWriterLogic : BaseWriterLogic
    {
        public DecompressWriterLogic(string outputFilePath) : base(outputFilePath) { }

        protected override void InsertPartStreamInfo(Stream outFileStream, object info) { }

        protected override void SignOutStream(Stream outFileStream) { }
    }
}