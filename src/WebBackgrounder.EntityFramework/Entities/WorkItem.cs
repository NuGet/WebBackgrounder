using System;
using System.ComponentModel.DataAnnotations;

namespace WebBackgrounder
{
    public class WorkItem : IWorkItem
    {
        public long Id { get; set; }
        [StringLength(64)]
        public string JobName { get; set; }
        [StringLength(64)]
        public string WorkerId { get; set; }
        public DateTime Started { get; set; }
        public DateTime? Completed { get; set; }
        public string ExceptionInfo { get; set; }
    }
}
