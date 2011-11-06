using System;

namespace WebBackgrounder
{
    public interface IWorkItemRepository : IDisposable
    {
        void RunInTransaction(Action query);
        IWorkItem GetLastWorkItem(string jobName);
        long CreateWorkItem(string workerId, string jobName);
        void SetWorkItemCompleted(long workItemId);
        void SetWorkItemFailed(long workItemId, Exception exception);
    }
}
