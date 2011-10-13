using System;

namespace WebBackgrounder.EntityFramework.Entities {
    public class JobWorker {
        public int Id { get; set; }
        public string Name { get; set; }
        public string WorkerId { get; set; }
        public DateTime Started { get; set; }
        public DateTime? Completed { get; set; }
        public string ExceptionInfo { get; set; }
    }
}
