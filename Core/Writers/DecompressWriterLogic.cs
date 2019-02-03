using System.IO;

namespace Core.Writers
{
    public sealed class DecompressWriterLogic : BaseWriterLogic
    {
        protected override void InsertPartStreamInfo(Stream outFileStream, object info) { }

        protected override void SignOutStream(Stream outFileStream) { }
    }
}