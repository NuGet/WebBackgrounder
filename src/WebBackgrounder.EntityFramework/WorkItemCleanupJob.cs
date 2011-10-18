using System;
using System.Linq;
using System.Threading.Tasks;

namespace WebBackgrounder.Jobs
{
    public class WorkItemCleanupJob : Job
    {
        readonly IWorkItemsContext _context;

        public WorkItemCleanupJob(TimeSpan interval, TimeSpan spanToKeepRecords, IWorkItemsContext context)
            : base("WorkItem Table Cleanup", interval)
        {
            if (spanToKeepRecords < TimeSpan.Zero)
            {
                throw new ArgumentException("Need to specify a positive time span.", "spanToKeepRecords");
            }
            SpanToKeepRecords = spanToKeepRecords;
            _context = context;
        }

        /// <summary>
        /// When this number is reached, this task will delete the oldest 
        /// work items larger than this number.
        /// </summary>
        public TimeSpan SpanToKeepRecords
        {
            get;
            private set;
        }

        public override Task Execute()
        {
            return new Task(() =>
            {
                var cutoffDate = DateTime.UtcNow.Subtract(SpanToKeepRecords);
                var oldItems = _context.WorkItems.Where(w => w.Completed != null && w.Completed < cutoffDate);
                if (oldItems.Any())
                {
                    foreach (var workItem in oldItems.ToList())
                    {
                        _context.WorkItems.Remove(workItem);
                    }
                    _context.SaveChanges();
                }
            });
        }
    }
}
