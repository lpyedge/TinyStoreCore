using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace TinyStore.Site.Controllers
{
    [Route("{controller}/{action}")]
    public class AdminController : Controller
    {
        [Route("/admin")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }
        public IActionResult Admin_Config()
        {
            return View();
        }
        
        public IActionResult Admin_List()
        {
            return View();
        }

        public IActionResult User_Store()
        {
            return View();
        }
        
        public IActionResult User_Finance()
        {
            return View();
        }
        
        public IActionResult User_Order()
        {
            return View();
        }
        public IActionResult User_Stat()
        {
            return View();
        }
        
        public IActionResult Supply_List()
        {
            return View();
        }
        
        public IActionResult Supply_Stock()
        {
            return View();
        }
    }
}
