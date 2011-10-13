using System;

namespace WebBackgrounder.EntityFramework
{
    /// <summary>
    /// Uses the database accessed via EF Code First to coordinate jobs in a web farm.
    /// </summary>
    public class WebFarmJobCoordinator : IJobCoordinator
    {
        readonly static string WorkerId = Guid.NewGuid().ToString();

        public void PerformWork(IJob jobWorker)
        {
            // We need a new instance every time we perform work.
            var repository = new EntityWorkItemRepository(jobWorker.Name);

            var unitOfWork = repository.ReserveWork(WorkerId);
            if (unitOfWork == null)
            {
                return;
            }

            // We need to wait because we're holding a lock while this 
            // work gets done to ensure the app domain doesn't kill it.
            try
            {
                jobWorker.Execute();
                unitOfWork.Complete();
            }
            catch (Exception exception)
            {
                unitOfWork.Fail(exception);
            }
        }
    }
}
