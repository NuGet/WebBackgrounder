using System;
using System.Linq;
using System.Threading.Tasks;
using WebBackgrounder.EntityFramework.Entities;

namespace WebBackgrounder.EntityFramework
{
    public class WorkItemCleanupJob : Job
    {
        readonly WorkItemsContext _context;

        public WorkItemCleanupJob(int maxWorkItemCount, TimeSpan interval, WorkItemsContext context) : base("WorkItem Table Cleanup", interval)
        {
            if (maxWorkItemCount < 2)
            {
                throw new ArgumentException("Need to have at least two work items, one for this job and one for another.", "maxWorkItemCount");
            }
            MaxWorkItemCount = maxWorkItemCount;
            _context = context;
        }

        /// <summary>
        /// When this number is reached, this task will delete the oldest 
        /// work items larger than this number.
        /// </summary>
        public int MaxWorkItemCount
        {
            get; 
            private set;
        }

        public override Task Execute()
        {
            return new Task(() =>
            {
                var count = _context.WorkItems.Count();
                if (count > MaxWorkItemCount)
                {
                    var oldest = (from workItem in _context.WorkItems
                                    orderby workItem.Started descending
                                    select workItem).Skip(MaxWorkItemCount).ToList();

                    if (oldest.Count > 0)
                    {
                        foreach (var workItem in oldest)
                        {
                            _context.WorkItems.Remove(workItem);
                        }
                        _context.SaveChanges();
                    }
                }
            });
        }
    }
}
