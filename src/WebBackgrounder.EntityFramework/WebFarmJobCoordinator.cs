using System;
using WebBackgrounder.EntityFramework.Entities;

namespace WebBackgrounder.EntityFramework {
    /// <summary>
    /// Uses the database accessed via EF Code First to coordinate jobs in a web farm.
    /// </summary>
    public class WebFarmJobCoordinator : IJobCoordinator {
        readonly JobsContext _context;
        readonly static string _workerId = Guid.NewGuid().ToString();

        public WebFarmJobCoordinator(JobsContext context) {
            _context = context;
        }

        public void PerformWork(IJob jobWorker) {
            // We need a new instance every time we perform work.
            var repository = new EntityJobWorkerRepository(_context, jobWorker.Name);

            var unitOfWork = repository.ReserveWork(_workerId);
            if (unitOfWork == null) {
                return;
            }

            // We need to wait because we're holding a lock while this 
            // work gets done to ensure the app domain doesn't kill it.
            try {
                jobWorker.Execute();
                unitOfWork.Complete();
            }
            catch (Exception exception) {
                unitOfWork.Fail(exception);
            }
        }
    }
}
