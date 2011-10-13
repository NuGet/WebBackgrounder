using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace WebBackgrounder.EntityFramework.Entities
{
    public class WorkItemsContext : DbContext
    {
        public DbSet<WorkItem> WorkItems { get; set; }

        public DbConnection Connection
        {
            get
            {
                return ((IObjectContextAdapter)this).ObjectContext.Connection;
            }
        }
    }
}
