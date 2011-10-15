using System;
using System.Threading.Tasks;

namespace WebBackgrounder
{
    /// <summary>
    /// Represents the environment that is hosting the task manager. 
    /// Typically a web application such as ASP.NET.
    /// </summary>
    public interface IJobHost
    {
        void DoWork(Task work);
    }
}
