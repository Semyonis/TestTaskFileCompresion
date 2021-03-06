﻿using System.IO;

using Core.Common;
using Core.Services;

namespace Core.Readers
{
    public sealed class CompressionReader : BaseReader
    {
        public CompressionReader(ReaderService service, Stream inStream, Stream outStream, int partIndex)
            : base(service, inStream, outStream, partIndex) { }

        protected override void StartOperation() { Compress(); }

        private void Compress()
        {
            var buffer = new byte[inStream.Length];
            inStream.CompressToStream(outStream, buffer, 0, buffer.Length);
        }
    }
}