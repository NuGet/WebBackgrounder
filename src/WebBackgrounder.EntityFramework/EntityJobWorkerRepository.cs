using System;
using System.Collections.Generic;
using System.Linq;
using WebBackgrounder.EntityFramework.Entities;

namespace WebBackgrounder.EntityFramework {
    public class EntityJobWorkerRepository {
        readonly JobsContext _context;
        readonly IEnumerable<JobWorker> _workers;
        readonly string _jobName;

        public EntityJobWorkerRepository(JobsContext context, string jobName) {
            _context = context;
            _jobName = jobName;
            _workers = from worker in _context.JobWorkers
                    where worker.Name == jobName
                        && worker.Status < (int)JobStatus.Complete
                    select worker;
        }

        public bool AnyActiveWorkers() {
            return _workers.Any();
        }

        public JobUnitOfWork ReserveWorker(Guid workerId) {
            var worker = new JobWorker { Name = _jobName, WorkerId = workerId, Status = (int)JobStatus.Ready };
            _context.JobWorkers.Add(worker);
            _context.SaveChanges();

            var currentWorker = GetCurrentWorker();
            return currentWorker.WorkerId == workerId ? new JobUnitOfWork(this, currentWorker) : null;
        }

        // Retrieves the oldest (aka first inserted) job with the same job name.
        JobWorker GetCurrentWorker() {
            // REVIEW: Make absolutely sure this queries the database and doesn't look in some context cache.
            var currentWorker = (from worker in _context.JobWorkers
                          where worker.Name == _jobName
                                && worker.Status == (int)JobStatus.Ready
                          orderby worker.Id ascending
                          select worker).FirstOrDefault();

            if (currentWorker == null) {
                throw new InvalidOperationException("Could not find a job to handle this worker, even though we should have just created one.");
            }

            return currentWorker;
        }

        public void SetWorkerStarted(JobWorker worker) {
            worker.Status = (int)JobStatus.Started;
            _context.SaveChanges();
        }

        public void SetWorkerComplete(JobWorker worker) {
            worker.Status = (int)JobStatus.Started;
            _context.SaveChanges();
        }

        public void SetWorkerFailed(JobWorker worker) {
            worker.Status = (int)JobStatus.Started;
            _context.SaveChanges();
        }

        public void UpdateIgnoredWorkers(JobWorker currentWorker) {
            var ignoredWorkers = from worker in _workers
                         where worker.Status == (int)JobStatus.Ready
                             && worker.Name == currentWorker.Name
                             && worker.WorkerId != currentWorker.WorkerId
                         select worker;

            foreach (var loser in ignoredWorkers) {
                loser.Status = (int)JobStatus.Ignored;
            }
            _context.SaveChanges();
        }
    }
}
