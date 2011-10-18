using System.Data.Entity;

namespace WebBackgrounder
{
    public class WorkItemsContext : DbContext, IWorkItemsContext
    {
        public IDbSet<WorkItem> WorkItems { get; set; }
    }
}
