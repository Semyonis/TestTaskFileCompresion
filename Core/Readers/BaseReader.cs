using System.IO;

using Core.Common;
using Core.Services;

namespace Core.Readers
{
    public abstract class BaseReader
    {
        private readonly int partIndex;
        private readonly ReaderService service;

        protected readonly Stream inStream;
        protected readonly Stream outStream;

        protected BaseReader(ReaderService service, Stream inStream, Stream outStream, int partIndex)
        {
            this.service = service;
            this.inStream = inStream;
            this.outStream = outStream;

            this.partIndex = partIndex;
        }

        public void StartWorker()
        {
            StartOperation();

            inStream.Close();

            service.Put(new StreamResult(partIndex, outStream));
        }

        protected abstract void StartOperation();
    }
}