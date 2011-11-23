using System;

namespace WebBackgrounder
{
    public interface IWorkItemRepository : IDisposable
    {
        void RunInTransaction(Action query);
        IWorkItem GetLastWorkItem(IJob job);
        long CreateWorkItem(string workerId, IJob job);
        void SetWorkItemCompleted(long workItemId);
        void SetWorkItemFailed(long workItemId, Exception exception);
    }
}
