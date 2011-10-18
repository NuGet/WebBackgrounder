using System.Linq;
using System.Web.Mvc;

namespace WebBackgrounder.DemoWeb.Controllers
{
    public class HomeController : Controller
    {
        private IWorkItemsContext db = new WorkItemsContext();

        public ActionResult Index()
        {
            var workers = (from w in db.WorkItems
                           orderby w.Started descending
                           select w).Take(30);
            return View(workers);
        }
    }
}
