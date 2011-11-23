using System;

namespace WebBackgrounder
{
    public static class WorkItemExtensions
    {
        public static bool IsActive(this IWorkItem workItem)
        {
            return workItem != null && workItem.Completed == null;
        }

        public static bool IsTimedOut(this IWorkItem workItem, IJob job)
        {
            if (job == null)
            {
                throw new ArgumentNullException("job");
            }
            return workItem != null && job.Timeout != TimeSpan.MaxValue && workItem.Started.Add(job.Timeout) < DateTime.UtcNow;
        }
    }
}
