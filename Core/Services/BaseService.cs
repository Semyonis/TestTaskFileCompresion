using System;

using Core.Instances;
using Core.Tokens;

namespace Core.Services
{
    public abstract class BaseService
    {
        protected readonly StreamResultQueue queue;
        protected readonly SystemSettingMonitor monitor;

        protected BaseService(StreamResultQueue queue, SystemSettingMonitor monitor)
        {
            this.queue = queue;
            this.monitor = monitor;
        }

        public CancellationToken Token
        {
            get
            {
                return monitor.Token;
            }
        } 

        public void HandleException(Exception exception, string additionalInfo)
        {
            monitor.HandleError(exception, additionalInfo);
        }
    }
}