using System;
using WebBackgrounder.EntityFramework.Entities;

namespace WebBackgrounder.EntityFramework {
    public class JobUnitOfWork : IDisposable {
        readonly EntityJobWorkerRepository _repository;
        readonly JobWorker _currentJob;
        bool _finished;

        public JobUnitOfWork(EntityJobWorkerRepository repository, JobWorker job) {
            _currentJob = job;
            _repository = repository;
            _repository.SetWorkerStarted(_currentJob);
        }

        public void Complete() {
            _repository.UpdateIgnoredWorkers(_currentJob);
            _repository.SetWorkerComplete(_currentJob);
            _finished = true;
        }

        public void Dispose() {
            if (!_finished) {
                _repository.SetWorkerFailed(_currentJob);
            }
        }
    }
}
