using System;
using System.Linq;
using System.Transactions;
using WebBackgrounder.EntityFramework.Entities;

namespace WebBackgrounder.EntityFramework
{
    public class EntityWorkItemRepository : IWorkItemRepository
    {
        WorkItemsContext _context;

        public EntityWorkItemRepository(Func<WorkItemsContext> contextThunk)
        {
            _context = contextThunk();
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

        public bool AnyActiveWorker(string jobName)
        {
            var activeWorker = GetActiveWorkItem(jobName);
            if (activeWorker != null)
            {
                // TODO: Handle work item expiration.
                return true;
            }
            return false;
        }

        private WorkItem GetActiveWorkItem(string jobName)
        {
            return (from w in _context.WorkItems
                    where w.JobName == jobName
                          && w.Completed == null
                    select w).FirstOrDefault();
        }

        public long CreateWorkItem(string workerId, string jobName)
        {
            var workItem = new WorkItem
            {
                JobName = jobName,
                WorkerId = workerId,
                Started = DateTime.UtcNow,
                Completed = null
            };
            _context.WorkItems.Add(workItem);
            _context.SaveChanges();
            return workItem.Id;
        }

        public void SetWorkItemCompleted(long workItemId)
        {
            var workItem = GetWorkItem(workItemId);
            workItem.Completed = DateTime.UtcNow;
            _context.SaveChanges();
        }

        public void SetWorkItemFailed(long workItemId, Exception exception)
        {
            var workItem = GetWorkItem(workItemId);
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