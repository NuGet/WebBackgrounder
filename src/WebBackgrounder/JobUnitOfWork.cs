using System;

namespace WebBackgrounder
{
    public class JobUnitOfWork
    {
        readonly IWorkItemRepository _repository;
        readonly long _workItemId;

        public JobUnitOfWork(IWorkItemRepository repository, long workItemId)
        {
            _workItemId = workItemId;
            _repository = repository;
        }

        public void Complete()
        {
            _repository.SetWorkItemCompleted(_workItemId);
        }

        public void Fail(Exception exception)
        {
            _repository.SetWorkItemFailed(_workItemId, exception);
        }
    }
}
