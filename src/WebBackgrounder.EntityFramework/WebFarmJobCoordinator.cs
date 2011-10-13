using System;
using WebBackgrounder.EntityFramework.Entities;

namespace WebBackgrounder.EntityFramework
{
    /// <summary>
    /// Uses the database accessed via EF Code First to coordinate jobs in a web farm.
    /// </summary>
    public class WebFarmJobCoordinator : IJobCoordinator
    {
        readonly static string WorkerId = Guid.NewGuid().ToString();
        readonly Func<string, IWorkItemRepository> _repositoryThunk;

        public WebFarmJobCoordinator(Func<string, IWorkItemRepository> repositoryThunk)
        {
            _repositoryThunk = repositoryThunk;
        }

        public void PerformWork(IJob jobWorker)
        {
            // We need a new instance every time we perform work.
            var repository = _repositoryThunk(jobWorker.Name);

            var unitOfWork = ReserveWork(repository, WorkerId);
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

        public JobUnitOfWork ReserveWork(IWorkItemRepository repository, string workerId)
        {
            object workItemId = null;
            
            repository.RunInTransaction(() =>
                {
                    if (repository.AnyActiveWorker)
                    {
                        workItemId = null;
                        return;
                    }
                    workItemId = repository.CreateWorkItem(workerId);
                }
            );

            if (workItemId == null)
            {
                return null;
            }
            return new JobUnitOfWork(repository, workItemId);
        }

    }
}
