﻿using System;
using System.IO;

namespace TestTaskFileCompression
{
    public sealed class MultithreadDecompressLogic : MultithreadOperationLogic
    {
        public MultithreadDecompressLogic(string inputFilePath)
            : base(inputFilePath)
        {
            Seek(inFileStream);
        }

        protected override OperationParameters GetOperationParameters(Stream inPartStream,
            Stream outPartStream,
            int partIndex)
        {
            return new DecompressionParameters(inPartStream, outPartStream, partIndex);
        }

        protected override int GetReadLength()
        {
            var array = new byte[4];
            inFileStream.Read(array, 0, 4);
            return BitConverter.ToInt32(array, 0);
        }

        private static void Seek(Stream inFileStream) { inFileStream.Seek(4, SeekOrigin.Begin); }
    }
}