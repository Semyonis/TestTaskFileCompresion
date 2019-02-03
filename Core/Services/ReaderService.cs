using System.IO;

using Core.Common;
using Core.Instances;

namespace Core.Services
{
    public sealed class ReaderService : BaseService
    {
        public ReaderService(StreamResultQueue queue, SystemSettingMonitor monitor)
            : base(queue, monitor) { }

        public bool InputStreamIsSliced
        {
            get { return queue.IsInputStreamSliced; }
        }

        public void Put(StreamResult result) { queue.Put(result); }

        public void SetInputStreamIsSliced() { queue.SetInputStreamIsSliced(); }

        public void IncrementReadCount() { queue.IncrementReadCount(); }

        public int GetProcessorCount() { return monitor.GetProcessorCount(); }

        public Stream GetNewStream(int length) { return monitor.GetNewStream(length); }
    }
}