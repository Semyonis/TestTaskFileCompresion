using System.Collections.Generic;
using System.Linq;

using Core.Common;

namespace Core.Instances
{
    public sealed class StreamResultQueue
    {
        private static volatile object mutex = new object();

        private readonly List<StreamResult> queue = new List<StreamResult>();

        private bool isStreamSliceFinished;

        private int totalPartCount;

        public StreamResult GetPartById(int index)
        {
            StreamResult result;

            lock (mutex)
            {
                result = queue.FirstOrDefault(item => item.PartIndex == index);
            }

            return result;
        }

        public void Put(StreamResult result)
        {
            lock (mutex)
            {
                queue.Add(result);
            }
        }

        public void Remove(StreamResult nextPart)
        {
            lock (mutex)
            {
                queue.Remove(nextPart);
            }
        }

        public void SetInputStreamIsSliced() { isStreamSliceFinished = true; }

        public void IncrementPartCount() { totalPartCount++; }

        public bool IsNotEnded(int wrotePartCount) { return wrotePartCount < totalPartCount || !isStreamSliceFinished; }
    }
}