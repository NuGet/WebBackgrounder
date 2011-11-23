using System;
using System.Threading.Tasks;

namespace WebBackgrounder
{
    public interface IJob
    {
        /// <summary>
        /// Identifies the type of job. For example, "UpdateStats"
        /// </summary>
        string Name { get; }

        Task Execute();

        TimeSpan Interval { get; }

        TimeSpan Timeout { get; }
    }
}
