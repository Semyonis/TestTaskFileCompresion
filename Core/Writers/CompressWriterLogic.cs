﻿using System;
using System.IO;

using Core.Common;

namespace Core.Writers
{
    public sealed class CompressWriterLogic : BaseWriterLogic
    {
        protected override void InsertPartStreamInfo(Stream outFileStream, object info)
        {
            var length = (int) info;
            WriteIntToStream(outFileStream, length);
        }

        protected override void SignOutputStream(Stream outFileStream)
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