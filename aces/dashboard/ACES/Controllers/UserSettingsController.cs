using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ACES.Controllers
{
    public class UserSettingsController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}

