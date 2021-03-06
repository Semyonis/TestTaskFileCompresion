﻿using System.IO;

namespace Core.Common
{
    public sealed class StreamResult
    {
        public StreamResult(int partIndex, Stream outStream)
        {
            PartIndex = partIndex;
            ResultStream = outStream;
        }

        public int PartIndex { get; private set; }

        public Stream ResultStream { get; private set; }
    }
}