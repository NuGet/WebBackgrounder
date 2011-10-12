using System;
using WebBackgrounder.EntityFramework.Entities;

namespace WebBackgrounder.EntityFramework {
    /// <summary>
    /// Uses the database accessed via EF Code First to coordinate jobs in a web farm.
    /// </summary>
    public class WebFarmJobCoordinator : IJobCoordinator {
        readonly JobsContext _context;
        readonly Guid _workerId = Guid.NewGuid();

        public WebFarmJobCoordinator(JobsContext context) {
            _context = context;
        }

        public void PerformWork(IJob jobWorker) {
            // We need a new instance every time we perform work.
            var repository = new EntityJobWorkerRepository(_context, jobWorker.Name);
            
            // TODO: If the pending job belongs to this worker, we need to deal with that.
            if (repository.AnyActiveWorkers()) {
                return;
            }

            var unitOfWork = repository.ReserveWorker(_workerId);
            if (unitOfWork == null) {
                return;
            }

            using (unitOfWork) {
                jobWorker.Execute();
                unitOfWork.Complete();
            }
        }
    }
}
