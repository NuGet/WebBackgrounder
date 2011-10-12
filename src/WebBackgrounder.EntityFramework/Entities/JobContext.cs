using System.Data.Entity;

namespace WebBackgrounder.EntityFramework.Entities {
    public class JobContext : DbContext {
        public DbSet<Job> Jobs { get; set; }
    }
}
