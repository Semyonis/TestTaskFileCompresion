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
            set { queue.IsInputStreamSliced = value; }
        }

        public void Put(StreamResult result) { queue.Put(result); }

        public int GetProcessorCount() { return monitor.GetProcessorCount(); }

        public Stream GetNewStream(int length) { return monitor.GetNewStream(length); }
    }
}