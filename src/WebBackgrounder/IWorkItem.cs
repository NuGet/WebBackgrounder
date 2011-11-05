using System;

namespace WebBackgrounder
{
    public interface IWorkItem
    {
        DateTime Started { get; }
        DateTime? Completed { get; set; }
    }
}
