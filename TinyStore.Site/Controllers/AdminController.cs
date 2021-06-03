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

        public IActionResult User_List()
        {
            return View();
        }
        
        //
        // public IActionResult AdminLog()
        // {
        //     return View();
        // }
        // public IActionResult Order()
        // {
        //     return View();
        // }
        // public IActionResult Orderdelete()
        // {
        //     return View();
        // }
        // public IActionResult Productsupply()
        // {
        //     return View();
        // }
        // public IActionResult UserInfo()
        // {
        //     return View();
        // }
        //
        // public IActionResult Userlog(int UserId)
        // {
        //     var user = BLL.UserBLL.QueryModelById(UserId);
        //     ViewBag.User = user;
        //     return View();
        // }
        //
        // public IActionResult Withdraw()
        // {
        //     return View();
        // }
        //
        // public IActionResult Admin()
        // {
        //     return View();
        // }

    }
}
