using System;
using System.Linq;
using WebBackgrounder.EntityFramework.Entities;

namespace WebBackgrounder.EntityFramework {
    public class EntityJobRepository : IJobRepository {
        readonly JobContext _context;
        public EntityJobRepository(JobContext context) {
            _context = context;
        }

        public bool PendingJobsExist(string jobName) {
            var pending = from job in _context.Jobs
                          where job.JobName == jobName
                            && job.Status != (int)JobStatus.Done
                            && job.Status != (int)JobStatus.Ignored
                          select job;
            return pending.Any();
        }

        public void CreateJobRequest(string jobName, Guid workerId) {
            var job = new Job { JobName = jobName, WorkerId = workerId, Status = (int)JobStatus.Ready };
            _context.Jobs.Add(job);
            _context.SaveChanges();
        }

        public Guid GetWorkerIdForJob(string jobName) {
            // Look for the oldest ready job.
            var winner = (from job in _context.Jobs
                          where job.JobName == jobName
                                && (job.Status == (int)JobStatus.Ready
                                || job.Status == (int)JobStatus.Started)
                          orderby job.Id ascending
                          select job).FirstOrDefault();

            if (winner == null) {
                return Guid.Empty;
            }

            return winner.WorkerId;
        }

        public void CompleteJob(string jobName, Guid workerId) {
            var losers = from job in _context.Jobs
                         where job.JobName == jobName
                             && job.Status == (int)JobStatus.Ready
                             && job.WorkerId != workerId
                         select job;

            foreach (var loser in losers) {
                loser.Status = (int)JobStatus.Ignored;
            }
            _context.SaveChanges();

            var winner = (from job in _context.Jobs
                          where job.JobName == jobName
                              && job.Status == (int)JobStatus.Ready
                              && job.WorkerId == workerId
                          select job).FirstOrDefault();
            if (winner == null) {
                throw new InvalidOperationException("No job ready for worker " + workerId);
            }

            winner.Status = (int)JobStatus.Done;
            _context.SaveChanges();
        }

        public void StartWork(string jobName, Guid workerId) {
            var winner = (from job in _context.Jobs
                          where job.JobName == jobName
                              && job.Status == (int)JobStatus.Ready
                              && job.WorkerId == workerId
                          select job).FirstOrDefault();
            if (winner == null) {
                throw new InvalidOperationException("No job ready for worker " + workerId);
            }

            winner.Status = (int)JobStatus.Started;
            _context.SaveChanges();
        }
    }
}
