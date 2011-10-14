using System;

namespace WebBackgrounder
{
    public interface IWorkItemRepository
    {
        void RunInTransaction(Action query);
        bool AnyActiveWorker { get; }
        long CreateWorkItem(string workerId);
        void SetWorkItemCompleted(long workItemId);
        void SetWorkItemFailed(long workItemId, Exception exception);
    }
}
