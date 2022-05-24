using System;
using System.Collections.Generic;
using System.Linq;
using LPayments;
using Microsoft.AspNetCore.Mvc;

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
                var order = BLL.OrderBLL.QueryModelById(orderId);
                if(order != null)
                {
                    var store = BLL.StoreBLL.QueryModelByStoreId(order.StoreId);
                    if (store != null)
                    {
                        ViewBag.Store = store;
                        ViewBag.Order = order;

                        if (!order.IsPay)
                        {
                            var payOrderId = Global.Generator.DateId(1);
                            order.PayOrderId = payOrderId;
                            BLL.OrderBLL.Update(order);
                            
                            var payTickets = new List<LPayments.PayTicket>();
                            var index = 0;
                            foreach (var payment in store.PaymentList.Where(p=>p.IsEnable))
                            {
                                LPayments.PayTicket payticket;
                                if (payment.IsSystem)
                                {
                                    var pay = SiteContext.Payment.GetPayment(payment.Name);
                                    //发起请求payOrderId会生成 _xx 后缀，防止同一支付平台不允许订单重复
                                    payticket = pay.Pay(payOrderId+"_"+index.ToString("00"), order.PayAmount, ECurrency.CNY, order.Name,
                                        Utils.RequestInfo._ClientIP(Request),
                                        "https://" + SiteContext.Config.SiteDomain + Url.ActionLink("Order","Home",new {orderid=order.OrderId}),
                                        "https://" + SiteContext.Config.SiteDomain + Url.ActionLink("PayNotify","ApiHome",new {payname=payment.Name}));

                                   //todo 支付test
                                    //  if (payticket.DataFormat == EPayDataFormat.Form)
                                    //  {
                                    //      payticket.DataFormat = EPayDataFormat.Url;
                                    //      var uri = payticket.Uri.IndexOf('?') > -1 ? payticket.Uri +"&"+ Utils.HttpWebUtility.BuildQueryString(payticket.Datas) : payticket.Uri + "?" + Utils.HttpWebUtility.BuildQueryString(payticket.Datas) ;
                                    //      payticket.Uri = uri;
                                    //  }
                                    // payticket.Token = (pay as IPayChannel).Platform.ToString().ToLowerInvariant();
                                    switch (payment.Name)
                                    {
                                        case "Alipay|AliPay|H5":
                                            payticket.Name = payticket.Name.ToLowerInvariant();
                                            payticket.Message = "支付宝支付" ;
                                            break;
                                        case "Alipay|AliPay|QRcode":
                                            payticket.Name = payticket.Name.ToLowerInvariant();
                                            payticket.Message = "支付宝二维码" ;
                                            break;
                                        case "Alipay|AliPay|PC":
                                            payticket.Name = payticket.Name.ToLowerInvariant();
                                            payticket.Message = "支付宝支付" ;
                                            break;
                                        case "WeChat|WeChat|H5":
                                            payticket.Name = payticket.Name.ToLowerInvariant();
                                            payticket.Message = "微信支付" ;
                                            break;
                                        case "WeChat|WeChat|QRcode":
                                            payticket.Name = payticket.Name.ToLowerInvariant();
                                            payticket.Message = "微信二维码" ;
                                            break;
                                        default:
                                            break;
                                    }
                                    payTickets.Add(payticket);
                                }
                                else
                                {
                                    switch (payment.BankType)
                                    {
                                        case EBankType.支付宝:
                                        {
                                            payticket = new LPayments.PayTicket()
                                            {
                                                Name = "alipay",
                                                DataFormat = EPayDataFormat.QrCode,
                                                DataContent = payment.QRCode,
                                                Message = "支付宝扫码转账",
                                            };
                                    
                                            payTickets.Add(payticket);
                                        }
                                            break;
                                        case EBankType.微信:
                                        { 
                                            payticket = new LPayments.PayTicket()
                                            {
                                                Name = "wechat",
                                                DataFormat = EPayDataFormat.QrCode,
                                                DataContent = payment.QRCode,
                                                Message = "微信扫码转账",
                                            };
                                    
                                            payTickets.Add(payticket);
                                        }
                                            break;
                                        case EBankType.银联:
                                        {
                                            //https://blog.csdn.net/gsls200808/article/details/89490358
                                            
                                            //https://qr.95516.com/00010000/01116734936470044423094034227630
                                            //https://qr.95516.com/00010000/01126270004886947443855476629280
                                            //https://qr.95516.com/00010002/01012166439217005044479417630044
                                            payticket = new LPayments.PayTicket()
                                            {
                                                Name = "unionpay",
                                                DataFormat = EPayDataFormat.QrCode,
                                                DataContent = payment.QRCode,
                                                Message = "银联扫码转账(云闪付,银行App)",
                                            };
                                    
                                            payTickets.Add(payticket);
                                        }
                                            break;
                                        case EBankType.工商银行:
                                        case EBankType.农业银行:
                                        case EBankType.建设银行:
                                        case EBankType.中国银行:
                                        case EBankType.交通银行:
                                        case EBankType.邮储银行:
                                        {
                                            payticket = new LPayments.PayTicket()
                                            {
                                                Name = "alipay",
                                                DataFormat = EPayDataFormat.QrCode,
                                                DataContent = SiteContext.Payment.TransferToBank(payment, order.PayAmount),
                                                Message = "支付宝扫码转账",
                                            };
                                    
                                            payTickets.Add(payticket);
                                        }
                                            break;
                                        default:
                                        { 
                                            payticket = new LPayments.PayTicket()
                                            {
                                                Name = payment.Name.ToLowerInvariant(),
                                                DataFormat = EPayDataFormat.QrCode,
                                                DataContent = payment.QRCode,
                                                Message = payment.Memo,
                                            };
                                    
                                            payTickets.Add(payticket);
                                        }
                                            break;
                                    }
                                }

                                index++;
                            }

                            ViewBag.PayTickets = payTickets;
                        }
                        
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
            return new EmptyResult();
        }

        [HttpGet("/s/{uniqueId:required}")]
        public IActionResult Store(string uniqueId)
        {
            if (!string.IsNullOrWhiteSpace(uniqueId))
            {
                var store = BLL.StoreBLL.QueryModelByUniqueId(uniqueId);
                if (store != null)                {
                    if (store.BlockList.Count > 0)
                    {
                        var region = SiteContext.IP2Region.Search(Utils.RequestInfo._ClientIP(Request).ToString());
                        if (store.BlockList.Any(p => region.Contains(p, StringComparison.OrdinalIgnoreCase)))
                        {
                            return new NotFoundResult();
                        }
                    }
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
            return new EmptyResult();
        }
    }
}
