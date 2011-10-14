using System;

namespace WebBackgrounder
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
            
            // We do a double check here because this is the first query we run and 
            // a database can't be created inside a transaction scope.
            if (repository.AnyActiveWorker)
            {
                return null;
            }

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
