using System;
using System.Transactions;

namespace WebBackgrounder.EntityFramework {
    /// <summary>
    /// Uses the database accessed via EF Code First to coordinate jobs in a web farm.
    /// </summary>
    public class WebFarmJobCoordinator : IJobCoordinator {
        readonly IJobRepository _repository;

        public WebFarmJobCoordinator(IJobRepository repository) {
            _repository = repository;
        }

        public bool CanDoWork(string jobName, Guid workerId) {
            bool canDoWork = false;
            using (var transaction = new TransactionScope()) {
                if (!_repository.PendingJobsExist(jobName)) {
                    _repository.CreateJobRequest(jobName, workerId);
                    canDoWork = _repository.GetWorkerIdForJob(jobName) == workerId;
                }
                transaction.Complete();
            }
            return canDoWork;
        }

        public void Done(string jobName, Guid workerId) {
            _repository.CompleteJob(jobName, workerId);
        }

        public IDisposable StartWork(string jobName, Guid workerId) {
            using (var transaction = new TransactionScope()) {
                _repository.StartWork(jobName, workerId);
            }
            return new WorkScope(this, jobName, workerId);
        }

        private class WorkScope : IDisposable {
            IJobCoordinator _coordinator;
            string _jobName;
            Guid _workerId;

            public WorkScope(IJobCoordinator coordinator, string jobName, Guid workerId) {
                _coordinator = coordinator;
                _jobName = jobName;
                _workerId = workerId;
            }

            public void Dispose() {
                _coordinator.Done(_jobName, _workerId);
            }
        }
    }
}
