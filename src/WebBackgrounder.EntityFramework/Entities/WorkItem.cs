using System;

namespace WebBackgrounder.EntityFramework.Entities {
    public class WorkItem {
        public int Id { get; set; }
        public string JobName { get; set; }
        public string WorkerId { get; set; }
        public DateTime Started { get; set; }
        public DateTime? Completed { get; set; }
        public string ExceptionInfo { get; set; }
    }
}
