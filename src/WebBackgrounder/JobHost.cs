using System;
using System.Threading.Tasks;
using System.Web.Hosting;

namespace WebBackgrounder
{
    public class JobHost : IJobHost, IRegisteredObject
    {
        readonly object _lock = new object();
        bool _shuttingDown;

        public JobHost()
        {
            HostingEnvironment.RegisterObject(this);
        }

        public void Stop(bool immediate)
        {
            lock (_lock)
            {
                _shuttingDown = true;
            }
            HostingEnvironment.UnregisterObject(this);
        }

        public void DoWork(Task work)
        {
            if (work == null)
            {
                throw new ArgumentNullException("work");
            }
            lock (_lock)
            {
                if (_shuttingDown)
                {
                    return;
                }
                work.Start();
                // Need to hold the lock until the task completes.
                // Later on, we should take advantage of the fact that the work is represented 
                // by a task. Instead of locking, we could simply have the Stop method cancel 
                // any pending tasks.
                work.Wait();
            }
        }
    }
}
