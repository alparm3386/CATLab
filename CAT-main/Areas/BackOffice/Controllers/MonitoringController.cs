using Microsoft.AspNetCore.Mvc;

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
