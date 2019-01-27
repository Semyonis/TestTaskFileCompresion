using System.Collections.Generic;
using System.Linq;

using TestTaskFileCompression.Common;

namespace TestTaskFileCompression.Instances
{
    public sealed class StreamResultQueue
    {
        private static volatile object instanceMutex = new object();

        private static StreamResultQueue instance;

        private readonly List<StreamResult> queue = new List<StreamResult>();

        private bool isStreamSliceFinished;

        private int totalPartCount;

        private StreamResultQueue() { }

        public static StreamResultQueue Instance
        {
            get
            {
                lock (instanceMutex)
                {
                    return instance ?? ( instance = new StreamResultQueue() );
                }
            }
        }
        
        public void Put(StreamResult result) { queue.Add(result); }

        public void Remove(StreamResult nextPart) { queue.Remove(nextPart); }

        public List<StreamResult> GetQueue() { return queue.ToList(); }

        public void SetIsStreamSliced() { isStreamSliceFinished = true; }

        public void IncrementPartCount() { totalPartCount++; }

        public bool IsNotEnded(int wrotePartCount) { return wrotePartCount < totalPartCount || !isStreamSliceFinished; }
    }
}