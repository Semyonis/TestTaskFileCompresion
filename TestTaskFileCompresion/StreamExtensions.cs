using System;
using System.IO;
using System.IO.Compression;

namespace TestTaskFileCompression
{
    public static class StreamExtensions
    {
        private const int ZIP_LEAD_BYTES = 0x04034b50;
        private const ushort GZIP_LEAD_BYTES = 0x8b1f;

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

            toStream.Write(buffer, offset, length);

            return readCount;
        }

        public static bool IsCompressed(this Stream stream)
        {
            var bytes = new byte[4];

            stream.Seek(0, SeekOrigin.Begin);
            stream.Read(bytes, 0, 4);

            return IsZipCompressed(bytes) || IsGZipCompressed(bytes);
        }

        private static bool IsZipCompressed(byte[] bytes) { return BitConverter.ToInt32(bytes, 0) == ZIP_LEAD_BYTES; }

        private static bool IsGZipCompressed(byte[] bytes) { return BitConverter.ToUInt16(bytes, 0) == GZIP_LEAD_BYTES; }
    }
}