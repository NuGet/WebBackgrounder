using System.Data.Entity;

namespace WebBackgrounder.EntityFramework.Entities
{
    public class WorkItemsContext : DbContext, IWorkItemsContext
    {
        public IDbSet<WorkItem> WorkItems { get; set; }
    }
}
