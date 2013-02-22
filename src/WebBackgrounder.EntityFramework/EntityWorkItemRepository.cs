using System;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Linq;
using System.Transactions;

namespace WebBackgrounder
{
    public class EntityWorkItemRepository : IWorkItemRepository, IDisposable
    {
        Func<IWorkItemsContext> _contextThunk;
        IWorkItemsContext _context;

        public EntityWorkItemRepository(Func<IWorkItemsContext> contextThunk)
        {
            if (contextThunk == null)
            {
                throw new ArgumentNullException("contextThunk");
            }

            _contextThunk = contextThunk;
            EnsureContext();
        }

        public void RunInTransaction(Action query)
        {
            // For some reason, I get different behavior when I use this
            // instead of _context.Database.Connection. This works, that doesn't. :(
            ((IObjectContextAdapter)_context).ObjectContext.Connection.Open();

            try
            {
                using (var transaction = new TransactionScope())
                {
                    query();
                    transaction.Complete();
                }
            }
            finally
            {
                // REVIEW: Make sure this is really needed. I kept running into 
                // exceptions when I didn't do this, but I may be doing it wrong. -Phil 10/17/2011
                _context.Dispose();
                EnsureContext();
            }
        }

        public IWorkItem GetLastWorkItem(IJob job)
        {
            return (from w in _context.WorkItems
                    where w.JobName == job.Name
                    orderby w.Started descending
                    select w).FirstOrDefault();
        }

        public long CreateWorkItem(string workerId, IJob job)
        {
            var workItem = new WorkItem
            {
                JobName = job.Name,
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

        public void Dispose()
        {
            Debug.Assert(_context != null);
            _context.Dispose();
        }

        private void EnsureContext()
        {
            var tempContext = _contextThunk();
            if (tempContext == null)
            {
                throw new InvalidOperationException("Context thunk must never return null");
            }

            _context = tempContext;
        }

        private WorkItem GetWorkItem(long workerId)
        {
            return _context.WorkItems.Find(workerId);
        }
    }
}