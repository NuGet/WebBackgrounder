using System;
using System.Data.Entity;

namespace WebBackgrounder.EntityFramework.Entities
{
    public interface IWorkItemsContext : IDisposable
    {
        IDbSet<WorkItem> WorkItems { get; set; }
        int SaveChanges();
    }
}
