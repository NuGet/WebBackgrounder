using System;

namespace WebBackgrounder.EntityFramework.Entities {
    public class Job {
        public int Id { get; set; }
        public string JobName { get; set; }
        public Guid WorkerId { get; set; }
        public int Status { get; set; }
    }
}
