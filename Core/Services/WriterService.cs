using Core.Common;
using Core.Instances;

namespace Core.Services
{
    public sealed class WriterService : BaseService
    {
        public WriterService(StreamResultQueue queue, SystemSettingMonitor monitor)
            : base(queue, monitor) { }

        public bool IsNotEnded
        {
            get { return queue.IsWritingNotEnded; }
        }

        public StreamResult GetNextPart() { return queue.GetNextPart(); }

        public void Clear() { monitor.Clear(); }
    }
}