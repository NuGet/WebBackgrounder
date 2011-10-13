using System;
using WebBackgrounder.EntityFramework.Entities;

namespace WebBackgrounder.EntityFramework
{
    public class JobUnitOfWork
    {
        readonly EntityWorkItemRepository _repository;
        readonly WorkItem _workItem;

        public JobUnitOfWork(EntityWorkItemRepository repository, WorkItem workItem)
        {
            _workItem = workItem;
            _repository = repository;
        }

        public void Complete()
        {
            _repository.SetWorkItemComplete(_workItem.Id);
        }

        public void Fail(Exception exception)
        {
            _repository.SetWorkItemFailed(_workItem.Id, exception);
        }
    }
}
