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

        private readonly List<StreamResult> queue = new List<StreamResult>();

        private int totalReadCount;
        private int totalWriteCount;

        public bool IsInputStreamSliced { get; private set; }

        public bool IsWritingNotEnded
        {
            get { return totalWriteCount < totalReadCount || !IsInputStreamSliced; }
        }

        public StreamResult GetNextPart()
        {
            // Checking part before wait
            CheckNextPart();

            // Exist 2 situations:
            // - next part already in queue (check part before wait)
            // - next part still is not putted in queue (check part when put it in queue)
            eventWaitHandle.WaitOne();

            StreamResult result;

            lock (mutex)
            {
                result = queue.FirstOrDefault(item => item.PartIndex == totalWriteCount);
            }

            return result;
        }

        public void Put(StreamResult result)
        {
            lock (mutex)
            {
                queue.Add(result);
            }

            // Checking part when put it in queue
            CheckNextPart();
        }

        public void Remove(StreamResult nextPart)
        {
            lock (mutex)
            {
                queue.Remove(nextPart);
            }
        }

        private void CheckNextPart()
        {
            lock (mutex)
            {
                if (queue.Any(item => item.PartIndex == totalWriteCount))
                {
                    eventWaitHandle.Set();
                }
            }
        }

        public void SetInputStreamIsSliced() { IsInputStreamSliced = true; }

        public void IncrementReadCount() { totalReadCount++; }

        public void IncrementWriteCount() { totalWriteCount++; }

    }
}