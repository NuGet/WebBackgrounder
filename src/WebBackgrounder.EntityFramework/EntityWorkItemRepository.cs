using System;
using System.Linq;
using System.Transactions;
using WebBackgrounder.EntityFramework.Entities;

namespace WebBackgrounder.EntityFramework
{
    public class EntityWorkItemRepository : IWorkItemRepository
    {
        WorkItemsContext _context;
        readonly string _jobName;

        public EntityWorkItemRepository(string jobName, Func<WorkItemsContext> contextThunk)
        {
            _context = contextThunk();
            _jobName = jobName;
        }

        public void RunInTransaction(Action query)
        {
            using (var transaction = new TransactionScope())
            {
                _context.Connection.Open();
                query();
                transaction.Complete();
            }
            _context = new WorkItemsContext();
        }

        public bool AnyActiveWorker
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

        public object CreateWorkItem(string workerId)
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
            return workItem.Id;
        }

        public void SetWorkItemCompleted(object workItemId)
        {
            var workItem = GetWorkItem((long)workItemId);
            workItem.Completed = DateTime.UtcNow;
            _context.SaveChanges();
        }

        public void SetWorkItemFailed(object workItemId, Exception exception)
        {
            var workItem = GetWorkItem((long)workItemId);
            workItem.Completed = DateTime.UtcNow;
            workItem.ExceptionInfo = exception.Message + Environment.NewLine + exception.StackTrace;
            _context.SaveChanges();
        }

        private WorkItem GetWorkItem(long workerId)
        {
            return _context.WorkItems.Find(workerId);
        }
    }
}