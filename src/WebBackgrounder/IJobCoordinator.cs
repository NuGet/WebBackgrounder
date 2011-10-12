using System;

namespace WebBackgrounder {
    public interface IJobCoordinator {
        bool CanDoWork(string jobName, Guid workerId);
        IDisposable StartWork(string jobName, Guid workerId);
        void Done(string jobName, Guid workerId);
    }
}
