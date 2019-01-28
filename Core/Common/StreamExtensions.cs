using System;
using System.IO;
using System.IO.Compression;

namespace Core.Common
{
    public static class StreamExtensions
    {
        public static void DecompressToStream(this Stream inStream,
            Stream outStream,
            byte[] buffer,
            int offset,
            int length)
        {
            using (var decompressionStream = new GZipStream(inStream, CompressionMode.Decompress))
            {
                int read;
                while (( read = decompressionStream.Read(buffer, offset, length) ) > 0)
                {
                    outStream.Write(buffer, offset, read);
                }
            }
        }

        public static void CompressToStream(this Stream inStream,
            Stream outStream,
            byte[] buffer,
            int offset,
            int length)
        {
            using (var compressionStream = new GZipStream(outStream, CompressionMode.Compress, true))
            {
                inStream.CopyTo(compressionStream, buffer, offset, length);
            }
        }

        public static int CopyTo(this Stream fromStream, Stream toStream, byte[] buffer, int offset, int length)
        {
            var readCount = fromStream.Read(buffer, offset, length);

            toStream.Write(buffer, offset, readCount);

            return readCount;
        }

        public static bool IsCompressed(this Stream stream)
        {
            var bytes = new byte[4];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bytes, 0, 4);

            return IsCustomGZipCompressed(bytes);
        }


        private static bool IsCustomGZipCompressed(byte[] bytes) { return BitConverter.ToUInt16(bytes, 0) == AppConstants.FORMAT_START_CHARS; }
    }
}