using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using CAT.Data;
using CAT.Models.Entities.Main;

namespace CAT.Areas.BackOffice.Controllers
{
    [Area("BackOffice")]
    public class TestController : Controller
    {
        private readonly MainDbContext _context;

        public TestController(MainDbContext context)
        {
            _context = context;
        }
    }
}
