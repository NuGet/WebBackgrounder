using System;

namespace WebBackgrounder.EntityFramework.Entities {
    public class JobWorker {
        public int Id { get; set; }
        public string Name { get; set; }
        public Guid WorkerId { get; set; }
        public int Status { get; set; }
    }
}
