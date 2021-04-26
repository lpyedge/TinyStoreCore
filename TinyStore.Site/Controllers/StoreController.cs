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
    public class StoreController : Controller
    {
        public IActionResult Index(string storeid)
        {
            ViewBag.StoreId = storeid;
            
            return View();
        }

        public IActionResult Login()
        {
            return View();
        }

        public IActionResult Basicinfo()
        {
            return View();
        }

        public IActionResult Benifit()
        {
            return View();
        }
        public IActionResult Order(string sid, string productid)
        {
            if(!string.IsNullOrWhiteSpace(sid))
            {
                var supplier = BLL.SupplyBLL.QueryModelById(sid);
                if(supplier != null)
                {
                    ViewBag.Supplier = supplier;
                    return View();
                }
            }
            if (!string.IsNullOrWhiteSpace(productid))
            {
                var prodcut = BLL.ProductBLL.QueryModelByProductIdAndStoreId(productid,sid);
                if (prodcut != null)
                {
                    ViewBag.Product = prodcut;
                    return View();
                }
            }
            return View();
        }

        [Route("/store/ordercloserecord/{sid}")]
        public IActionResult Ordercloserecord(string sid)
        {
            return View();
        }
        public IActionResult Product()
        {
            return View();
        }
        public IActionResult Pwdmodify()
        {
            return View();
        }
        public IActionResult Register()
        {
            return View();
        }
        public IActionResult Stock()
        {
            return View();
        }
        public IActionResult Stockrecycle()
        {
            return View();
        }
        public IActionResult Store()
        {
            return View();
        }
        public IActionResult Supplier()
        {
            return View();
        }
        public IActionResult Userlog()
        {
            return View();
        }
        public IActionResult Withdraw()
        {
            return View();
        }
    }
}
