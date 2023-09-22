using Microsoft.AspNetCore.Mvc;
using Microsoft.CodeAnalysis.FlowAnalysis.DataFlow;

namespace CAT.Areas.BackOffice.Controllers
{
    [Area("BackOffice")]
    public class MonitoringController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
