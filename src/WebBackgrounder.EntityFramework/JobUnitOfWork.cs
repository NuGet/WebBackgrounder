using System;
using WebBackgrounder.EntityFramework.Entities;

namespace WebBackgrounder.EntityFramework {
    public class JobUnitOfWork {
        readonly EntityJobWorkerRepository _repository;
        readonly JobWorker _currentJob;

        public JobUnitOfWork(EntityJobWorkerRepository repository, JobWorker job) {
            _currentJob = job;
            _repository = repository;
        }

        public void Complete() {
            _repository.SetWorkerComplete(_currentJob);
        }

        public void Fail(Exception exception) {
            _repository.SetWorkerFailed(_currentJob, exception);
        }
    }
}
