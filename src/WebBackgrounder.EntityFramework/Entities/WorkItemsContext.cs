using System.Data.Common;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;

namespace WebBackgrounder.EntityFramework.Entities
{
    public class WorkItemsContext : DbContext
    {
        public IDbSet<WorkItem> WorkItems { get; set; }

        public DbConnection Connection
        {
            get
            {
                // For some reason, I get different behavior when I use this
                // instead of Database.Connection. This works, that doesn't. :(
                return ((IObjectContextAdapter)this).ObjectContext.Connection;
            }
        }
    }
}
