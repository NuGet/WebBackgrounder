using System.Linq;
using System.Web.Mvc;
using WebBackgrounder.EntityFramework.Entities;

namespace WebBackgrounder.DemoWeb.Controllers {
    public class HomeController : Controller {
        private WorkItemsContext db = new WorkItemsContext();

        public ActionResult Index() {
            var workers = (from w in db.WorkItems
                           orderby w.Started descending
                           select w).Take(30);
            return View(workers);
        }
    }
}
