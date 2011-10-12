using System.Data.Entity;

namespace WebBackgrounder.EntityFramework.Entities {
    public class JobsContext : DbContext {
        public DbSet<JobWorker> JobWorkers { get; set; }
    }
}
