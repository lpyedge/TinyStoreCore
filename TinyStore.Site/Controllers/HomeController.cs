using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using LPayments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;

namespace TinyStore.Site.Controllers
{
    public class HomeController : Controller
    {

        [HttpGet("/")]
        public IActionResult Index()
        {
            ViewBag.StoreList = BLL.StoreBLL.QueryHotList(6);
            return View(); 
            //返回Views目录下当前Controller名称目录下的当前Action名称的模板文件
            //return View("xxx/yyy");返回Views目录下当前Controller名称目录下xxx目录下的yyy模板文件
            //return View("default/index");

        }


        [HttpGet("/stores")]
        public IActionResult Stores()
        {
            ViewBag.StoerList = BLL.StoreBLL.QueryListMini();
            return View();
        }

        [HttpGet("/o/{orderId:required}")]
        public IActionResult Order(string orderId)
        {
            if (!string.IsNullOrWhiteSpace(orderId))
            {
                var order = BLL.OrderBLL.QueryModelByOrderId(orderId);
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

        [HttpGet("/s/{uniqueId:required}")]
        public IActionResult Store(string uniqueId)
        {
            if (!string.IsNullOrWhiteSpace(uniqueId))
            {
                var store = BLL.StoreBLL.QueryModelByUniqueId(uniqueId);
                if (store != null)                {
                    
                    ViewBag.Store = store;
                     var productlist = BLL.ProductBLL.QueryListByStoreShow(store.StoreId);
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
        
        
        [HttpGet("/pay/{orderId:required}")]
        public IActionResult Pay(string orderId)
        {
            if (!string.IsNullOrWhiteSpace(orderId))
            {
                var order = BLL.OrderBLL.QueryModelByOrderId(orderId);
                if(order != null)
                {
                    var store = BLL.StoreBLL.QueryModelByStoreId(order.StoreId);
                    if (store != null)
                    {
                        ViewBag.Store = store;
                        ViewBag.Order = order;

                        var PayTickets = new List<LPayments.PayTicket>();
                        foreach (var payment in store.PaymentList.Where(p=>p.IsEnable))
                        {
                            if (payment.IsSystem)
                            {
                                var pay = SiteContext.Payment.GetPayment(payment.Name);
                                var payticket = pay.Pay(order.OrderId, order.Amount, ECurrency.CNY, order.Name,
                                    Utils.RequestInfo._ClientIP(Request),
                                    "https://" + SiteContext.Config.SiteDomain + "/o/" + order.OrderId,
                                    "https://" + SiteContext.Config.SiteDomain + "/PayNotify/" + payment.Name);
                                
                                payticket.Token = (pay as IPayChannel).Platform.ToString().ToLowerInvariant();
                                    
                                PayTickets.Add(payticket);
                            }
                            else
                            {
                                switch (payment.BankType)
                                {
                                    case EBankType.支付宝:
                                    {
                                        PayTickets.Add(new LPayments.PayTicket()
                                        {
                                            Action = EAction.QrCode,
                                            Uri = payment.QRCode,
                                            Datas = new Dictionary<string, string>(),
                                            Success = true,
                                            Message = "支付宝扫码转账",
                                            Token = "alipay"
                                        });
                                    }
                                        break;
                                    case EBankType.微信:
                                    { 
                                        PayTickets.Add(new LPayments.PayTicket()
                                        {
                                            Action = EAction.QrCode,
                                            Uri = payment.QRCode,
                                            Datas = new Dictionary<string, string>(),
                                            Success = true,
                                            Message = "微信扫码转账",
                                            Token = "wechat"
                                        });
                                    }
                                        break;
                                    case EBankType.银联:
                                    {
                                        //https://blog.csdn.net/gsls200808/article/details/89490358
                                        
                                        //https://qr.95516.com/00010000/01116734936470044423094034227630
                                        //https://qr.95516.com/00010000/01126270004886947443855476629280
                                        //https://qr.95516.com/00010002/01012166439217005044479417630044
                                        PayTickets.Add(new LPayments.PayTicket()
                                        {
                                            Action = EAction.QrCode,
                                            Uri = payment.QRCode,
                                            Datas = new Dictionary<string, string>(),
                                            Success = true,
                                            Message = "银联扫码转账(云闪付,银行App)",
                                            Token = "unionpay"
                                        });
                                    }
                                        break;
                                    case EBankType.工商银行:
                                    case EBankType.农业银行:
                                    case EBankType.建设银行:
                                    case EBankType.中国银行:
                                    case EBankType.交通银行:
                                    case EBankType.邮储银行:
                                    {
                                        PayTickets.Add(new LPayments.PayTicket()
                                        {
                                            Action = EAction.QrCode,
                                            Uri = SiteContext.Payment.TransferToBank(payment, order.Amount),
                                            Datas = new Dictionary<string, string>(),
                                            Success = true,
                                            Message = "支付宝扫码转账",
                                            Token = "alipay"
                                        });
                                    }
                                        break;
                                    default:
                                    { 
                                        PayTickets.Add(new LPayments.PayTicket()
                                        {
                                            Action = EAction.QrCode,
                                            Uri = payment.QRCode,
                                            Datas = new Dictionary<string, string>(),
                                            Success = true,
                                            Message = payment.Memo,
                                            Token = ""
                                        });
                                    }
                                        break;
                                }
                                
                            }
                        }

                        ViewBag.PayTickets = PayTickets;
                        
                        return View();
                    }
                }
            }
            return new RedirectResult("/");
        }
    }
}
