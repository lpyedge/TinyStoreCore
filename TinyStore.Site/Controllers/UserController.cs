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
    public class UserController : Controller
    {
        [Route("/user")]
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult User_Profile()
        {
            return View();
        }
        
        public IActionResult User_Secret()
        {
            return View();
        }
        
        public IActionResult User_Finance()
        {
            return View();
        }
        
        public IActionResult User_Log()
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
        
        
        public IActionResult Store_Info()
        {
            return View();
        }
        
        public IActionResult Store_Payment()
        {
            return View();
        }
        
        public IActionResult Store_Product()
        {
            return View();
        }
        
        public IActionResult Store_Order()
        {
            return View();
        }
        
        public IActionResult Store_Stat()
        {
            return View();
        }
        
        //！----------------------！//
        public IActionResult Register()
        {
            return View();
        }
    }
}
