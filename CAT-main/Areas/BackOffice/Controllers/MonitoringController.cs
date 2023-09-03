using Microsoft.AspNetCore.Mvc;

namespace CAT.Areas.BackOffice.Controllers
{
    public class MonitoringController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
