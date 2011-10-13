using System;
using System.Linq;
using WebBackgrounder.EntityFramework.Entities;

namespace WebBackgrounder.EntityFramework {
    public class EntityJobWorkerRepository {
        readonly JobsContext _context;
        readonly string _jobName;

        public EntityJobWorkerRepository(JobsContext context, string jobName) {
            _context = context;
            _jobName = jobName;
        }

        public JobUnitOfWork ReserveWork(string workerId) {
            JobWorker worker = null;
            //TODO: using (var transaction = new TransactionScope()) {
            if (AnyActiveWorker) {
                return null;
            }

            worker = CreateWorker(workerId);
            //TODO: transaction.Complete();
            //TODO: }

            return new JobUnitOfWork(this, worker);
        }

        private bool AnyActiveWorker {
            get {
                var activeWorker = GetActiveWorker();
                if (activeWorker != null) {
                    // TODO: Handle job expiration.
                    return true;
                }
                return false;
            }
        }

        private JobWorker GetActiveWorker() {
            return (from w in _context.JobWorkers
                    where w.Name == _jobName
                    && w.Completed == null
                    select w).FirstOrDefault();
        }

        private JobWorker CreateWorker(string workerId) {
            var worker = new JobWorker {
                Name = _jobName,
                WorkerId = workerId,
                Started = DateTime.UtcNow,
                Completed = null
            };
            _context.JobWorkers.Add(worker);
            _context.SaveChanges();
            return worker;
        }

        public void SetWorkerComplete(JobWorker worker) {
            worker.Completed = DateTime.UtcNow;
            _context.SaveChanges();
        }

        public void SetWorkerFailed(JobWorker worker, Exception exception) {
            worker.Completed = DateTime.UtcNow;
            worker.ExceptionInfo = exception.Message + Environment.NewLine + exception.StackTrace;
            _context.SaveChanges();
        }
    }
}
