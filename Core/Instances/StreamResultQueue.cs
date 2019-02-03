using System.Collections.Generic;
using System.Linq;
using System.Threading;

using Core.Common;

namespace Core.Instances
{
    public sealed class StreamResultQueue
    {
        private static volatile object mutex = new object();

        private readonly EventWaitHandle eventWaitHandle = new EventWaitHandle(false, EventResetMode.AutoReset);

        private volatile List<StreamResult> queue = new List<StreamResult>();

        private int totalReadCount;
        private int totalWriteCount;

        public bool IsInputStreamSliced { get; set; }

        public bool IsWritingNotEnded
        {
            get { return totalWriteCount < totalReadCount || !IsInputStreamSliced; }
        }

        public StreamResult GetNextPart()
        {
            // Checking part before wait
            lock (mutex)
            {
                if (queue.Any(item => item.PartIndex == totalWriteCount))
                {
                    eventWaitHandle.Set();
                }
            }

            // Exist 2 situations:
            // - next part already in queue (check part before wait)
            // - next part still is not putted in queue (check part when put it in queue)
            eventWaitHandle.WaitOne();

            StreamResult result;

            lock (mutex)
            {
                result = queue.First(item => item.PartIndex == totalWriteCount);

                queue.Remove(result);

                totalWriteCount++;
            }

            return result;
        }

        public void Put(StreamResult result)
        {
            lock (mutex)
            {
                queue.Add(result);

                // Checking part when put it in queue
                if (result.PartIndex == totalWriteCount)
                {
                    eventWaitHandle.Set();
                }
            }
        }

        public void IncrementReadPartCount() { totalReadCount++; }
    }
}