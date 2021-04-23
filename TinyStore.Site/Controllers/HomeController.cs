using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace TinyStore.Site.Controllers
{
    public class HomeController : Controller
    {

        [Route("/")]
        public IActionResult Index()
        {
            ViewBag.StoerList = BLL.StoreBLL.QueryHotList(6);
            return View(); 
            //返回Views目录下当前Controller名称目录下的当前Action名称的模板文件
            //return View("xxx/yyy");返回Views目录下当前Controller名称目录下xxx目录下的yyy模板文件
            //return View("default/index");

        }


        [Route("/stores")]
        public IActionResult Stores()
        {
            ViewBag.StoerList = BLL.StoreBLL.QueryListMini();
            return View();
        }

        [Route("/o/{orderid}")]
        public IActionResult Order(string orderid)
        {
            if (!string.IsNullOrWhiteSpace(orderid))
            {
                var order = BLL.OrderBLL.QueryModelByOrderId(orderid);
                if(order != null)
                {
                    var store = BLL.StoreBLL.QueryModelByStoreId(order.StoreId);
                    if (store != null)
                    {
                        ViewBag.Store = store;
                        ViewBag.Order = order;
                        if (store.Template == EStoreTemplate.模板一)
                        {
                            return View("T1/Order");
                        }
                        else if (store.Template == EStoreTemplate.模板二)
                        {
                            return View("T2/Order");
                        }
                        else if (store.Template == EStoreTemplate.模板三)
                        {
                            return View("T3/Order");
                        }
                        else if (store.Template == EStoreTemplate.模板四)
                        {
                            return View("T4/Order");
                        }
                        else if (store.Template == EStoreTemplate.模板五)
                        {
                            return View("T5/Order");
                        }
                        else
                        {
                            return View("T1/Order");
                        }
                    }
                }
            }
            return new RedirectResult("/");
        }

        [Route("/s/{uniqueid}")]
        public IActionResult Store(string uniqueid)
        {
            if (!string.IsNullOrWhiteSpace(uniqueid))
            {
                var store = BLL.StoreBLL.QueryModelByUniqueId(uniqueid);
                if (store != null)                {
                    
                    ViewBag.Store = store;
                     var productlist = BLL.ProductBLL.QueryListByStoreIdShow(store.StoreId);
                    ViewBag.ProductList = productlist;
                    
                    if (store.Template == EStoreTemplate.模板一)
                    {
                        return View("T1/Store");
                    }
                    else if (store.Template == EStoreTemplate.模板二)
                    {
                        return View("T2/Store");
                    }
                    else if (store.Template == EStoreTemplate.模板三)
                    {
                        return View("T3/Store");
                    }
                    else if (store.Template == EStoreTemplate.模板四)
                    {
                        return View("T4/Store");
                    }
                    else if (store.Template == EStoreTemplate.模板五)
                    {
                        return View("T5/Store");
                    }
                    else
                    {
                        return View("T1/Store");
                    }
                }
            }
            return new RedirectResult("/");
        }
    }
}
