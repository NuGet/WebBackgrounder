using System;
using System.Linq;
using System.Transactions;
using WebBackgrounder.EntityFramework.Entities;

namespace WebBackgrounder.EntityFramework
{
    public class EntityWorkItemRepository
    {
        WorkItemsContext _context;
        readonly string _jobName;

        public EntityWorkItemRepository(string jobName)
        {
            _context = new WorkItemsContext();
            _jobName = jobName;
        }

        public JobUnitOfWork ReserveWork(string workerId)
        {
            WorkItem workItem;
            using (var transaction = new TransactionScope())
            {
                _context.Connection.Open();
                if (AnyActiveWorker)
                {
                    return null;
                }

                workItem = CreateWorker(workerId);
                transaction.Complete();
            }
            _context = new WorkItemsContext();
            return new JobUnitOfWork(this, workItem);
        }

        private bool AnyActiveWorker
        {
            get
            {
                var activeWorker = GetActiveWorkItem();
                if (activeWorker != null)
                {
                    // TODO: Handle work item expiration.
                    return true;
                }
                return false;
            }
        }

        private WorkItem GetActiveWorkItem()
        {
            return (from w in _context.WorkItems
                    where w.JobName == _jobName
                          && w.Completed == null
                    select w).FirstOrDefault();
        }

        private WorkItem CreateWorker(string workerId)
        {
            var workItem = new WorkItem
            {
                JobName = _jobName,
                WorkerId = workerId,
                Started = DateTime.UtcNow,
                Completed = null
            };
            _context.WorkItems.Add(workItem);
            _context.SaveChanges();
            return workItem;
        }

        public void SetWorkItemComplete(int workItemId)
        {
            var workItem = GetWorkItem(workItemId);
            workItem.Completed = DateTime.UtcNow;
            _context.SaveChanges();
        }

        public void SetWorkItemFailed(int workItemId, Exception exception)
        {
            var workItem = GetWorkItem(workItemId);
            workItem.Completed = DateTime.UtcNow;
            workItem.ExceptionInfo = exception.Message + Environment.NewLine + exception.StackTrace;
            _context.SaveChanges();
        }

        private WorkItem GetWorkItem(int workerId)
        {
            return _context.WorkItems.Find(workerId);
        }
    }
}