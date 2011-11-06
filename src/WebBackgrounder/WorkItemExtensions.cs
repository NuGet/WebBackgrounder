
namespace WebBackgrounder
{
    public static class WorkItemExtensions
    {
        public static bool IsActive(this IWorkItem workItem)
        {
            return workItem != null && workItem.Completed == null;
        }
    }
}
