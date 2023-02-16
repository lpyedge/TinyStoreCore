using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Payments;

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
            //https://localhost:5001/o/23KJ6NYXWT6?charset=utf-8&out_trade_no=23KJ6P1E98K_02&method=alipay.trade.page.pay.return&total_amount=10.00&sign=R%2FZ2WkLmK9hz7mwnd1fZChcxX66Mg%2Bf7v8TPLVZI8fFWdY3eeTK84P5DMdXxmyqOvMMFozOuo%2FlBjpVOYP8kPMuTUw2e%2FJkVGiHAQOvbY8AeaIeV8kkEiU4oDaXzUlrL0P%2F4qwp2xgMhtAuvgZhosXrofMRtjmL6wcaS7wAsl%2Fzu9Ub%2Fhqyp7Do3UZsywATAOThC%2F7Zmj5nWdhfIzbkoMsILXqqxZGADG%2BjL2dhVslJIjrb7mebB8ZJEPHWJ4ha%2FkH3j9bKP%2F0XXASGjmNMzPk8RYZotdQXFmKfHmL2%2BblgCzngn%2FS00VS%2F5LHRaGBbIDr%2BrqcQeAoCRT90wlQjrCA%3D%3D&trade_no=2023021522001423421425321129&auth_app_id=2017082508368887&version=1.0&app_id=2017082508368887&sign_type=RSA2&seller_id=2088301211369752&timestamp=2023-02-15+16%3A43%3A53
            // {
            //     "charset": "utf-8",
            //     "out_trade_no": "23KJ6P1E98K_02",
            //     "method": "alipay.trade.page.pay.return",
            //     "total_amount": "10.00",
            //     "sign": "R/Z2WkLmK9hz7mwnd1fZChcxX66Mg+f7v8TPLVZI8fFWdY3eeTK84P5DMdXxmyqOvMMFozOuo/lBjpVOYP8kPMuTUw2e/JkVGiHAQOvbY8AeaIeV8kkEiU4oDaXzUlrL0P/4qwp2xgMhtAuvgZhosXrofMRtjmL6wcaS7wAsl/zu9Ub/hqyp7Do3UZsywATAOThC/7Zmj5nWdhfIzbkoMsILXqqxZGADG+jL2dhVslJIjrb7mebB8ZJEPHWJ4ha/kH3j9bKP/0XXASGjmNMzPk8RYZotdQXFmKfHmL2+blgCzngn/S00VS/5LHRaGBbIDr+rqcQeAoCRT90wlQjrCA==",
            //     "trade_no": "2023021522001423421425321129",
            //     "auth_app_id": "2017082508368887",
            //     "version": "1.0",
            //     "app_id": "2017082508368887",
            //     "sign_type": "RSA2",
            //     "seller_id": "2088301211369752",
            //     "timestamp": "2023-02-15 16:43:53"
            // }
            if (!string.IsNullOrWhiteSpace(orderId))
            {
                var order = BLL.OrderBLL.QueryModelById(orderId);
                if(order != null)
                {
                    var store = BLL.StoreBLL.QueryModelByStoreId(order.StoreId);
                    if (store != null)
                    {

                        if (!order.IsPay)
                        {
                            bool ispayed = false;
                            if (Request.QueryString.HasValue && !string.IsNullOrWhiteSpace(Request.QueryString.Value))
                            {
                                foreach (var payment in store.PaymentList.Where(p => p.IsEnable))
                                {
                                    if (payment.IsSystem)
                                    {
                                       
                                        ispayed =  SiteContext.Payment.Notify(payment.Name,
                                            new Dictionary<string, string>(),
                                            Request.Query.ToDictionary(p => p.Key
                                                , p => p.Value.ToString()),
                                            Request.Headers.ToDictionary(p => p.Key
                                                , p => p.Value.ToString()),
                                            "",Utils.RequestInfo._ClientIP(Request).ToString(),out string msg);

                                        if (ispayed)
                                        {
                                            order = BLL.OrderBLL.QueryModelById(orderId);
                                            break;
                                        }
                                    }
                                }
                            }
                            
                            if (!ispayed)
                            {
                                var payOrderId = Global.Generator.DateId(1);
                                order.PayOrderId = payOrderId;
                                BLL.OrderBLL.Update(order);
                                
                                var payTickets = new List<Payments.PayTicket>();
                                var index = 0;
                                foreach (var payment in store.PaymentList.Where(p=>p.IsEnable))
                                {
                                    Payments.PayTicket payticket;
                                    if (payment.IsSystem)
                                    {
                                        var pay = SiteContext.Payment.GetPayment(payment.Name);
                                        //发起请求payOrderId会生成 _xx 后缀，防止同一支付平台不允许订单重复
                                        payticket = pay.Pay(payOrderId+"_"+index.ToString("00"), order.PayAmount, Payments.ECurrency.CNY, order.Name,
                                            Utils.RequestInfo._ClientIP(Request),
                                            string.IsNullOrWhiteSpace(SiteContext.Config.SiteDomain )? Url.ActionLink("Order","Home",new {orderid=order.OrderId}):"https://" + SiteContext.Config.SiteDomain + Url.Action("Order","Home",new {orderid=order.OrderId}),
                                            string.IsNullOrWhiteSpace(SiteContext.Config.SiteDomain )? Url.ActionLink("PayNotify","ApiHome",new {payname=payment.Name}):"https://" + SiteContext.Config.SiteDomain + Url.Action("PayNotify","ApiHome",new {payname=payment.Name})
                                            );

                                       //todo 支付test
                                        //  if (payticket.DataFormat == EPayDataFormat.Form)
                                        //  {
                                        //      payticket.DataFormat = EPayDataFormat.Url;
                                        //      var uri = payticket.Uri.IndexOf('?') > -1 ? payticket.Uri +"&"+ Utils.HttpWebUtility.BuildQueryString(payticket.Datas) : payticket.Uri + "?" + Utils.HttpWebUtility.BuildQueryString(payticket.Datas) ;
                                        //      payticket.Uri = uri;
                                        //  }
                                        // payticket.Token = (pay as IPayChannel).Platform.ToString().ToLowerInvariant();
                                        
                                        
                                        
                                        // switch (payment.Name)
                                        // {
                                        //     case "AliPay_AliPay_H5":
                                        //         payticket.Name = payticket.Name.ToLowerInvariant();
                                        //         payticket.Message = "支付宝支付" ;
                                        //         break;
                                        //     case "AliPay_AliPay_QRcode":
                                        //         payticket.Name = payticket.Name.ToLowerInvariant();
                                        //         payticket.Message = "支付宝二维码" ;
                                        //         break;
                                        //     case "AliPay_AliPay_PC":
                                        //         payticket.Name = payticket.Name.ToLowerInvariant();
                                        //         payticket.Message = "支付宝支付" ;
                                        //         break;
                                        //     case "WeChat_WeChat_H5":
                                        //         payticket.Name = payticket.Name.ToLowerInvariant();
                                        //         payticket.Message = "微信支付" ;
                                        //         break;
                                        //     case "WeChat_WeChat_QRcode":
                                        //         payticket.Name = payticket.Name.ToLowerInvariant();
                                        //         payticket.Message = "微信二维码" ;
                                        //         break;
                                        //     default:
                                        //         break;
                                        // }

                                        if (payticket.DataFormat != EPayDataFormat.Error)
                                        {
                                            if (payment.Name.Contains(Payments.EChannel.AliPay.ToString(),
                                                    StringComparison.OrdinalIgnoreCase))
                                            {
                                                if (payment.Name.Contains(Payments.EPayType.QRcode.ToString(),
                                                        StringComparison.OrdinalIgnoreCase))
                                                {
                                                    payticket.Name = payticket.Name.ToLowerInvariant();
                                                    payticket.Message = "支付宝二维码" ;
                                                }
                                                else if (payment.Name.Contains(Payments.EPayType.PC.ToString(),
                                                             StringComparison.OrdinalIgnoreCase))
                                                {
                                                    payticket.Name = payticket.Name.ToLowerInvariant();
                                                    payticket.Message = "支付宝网关支付" ;
                                                }
                                                else if (payment.Name.Contains(Payments.EPayType.H5.ToString(),
                                                             StringComparison.OrdinalIgnoreCase))
                                                {
                                                    payticket.Name = payticket.Name.ToLowerInvariant();
                                                    payticket.Message = "支付宝手机支付" ;
                                                }
                                            }
                                            else if (payment.Name.Contains(Payments.EChannel.WeChat.ToString(),
                                                    StringComparison.OrdinalIgnoreCase))
                                            {
                                                if (payment.Name.Contains(Payments.EPayType.QRcode.ToString(),
                                                        StringComparison.OrdinalIgnoreCase))
                                                {
                                                    payticket.Name = payticket.Name.ToLowerInvariant();
                                                    payticket.Message = "微信二维码" ;
                                                }
                                                else if (payment.Name.Contains(Payments.EPayType.H5.ToString(),
                                                             StringComparison.OrdinalIgnoreCase))
                                                {
                                                    payticket.Name = payticket.Name.ToLowerInvariant();
                                                    payticket.Message = "微信手机支付" ;
                                                }
                                            }
                                            payTickets.Add(payticket);
                                        }
                                    }
                                    else
                                    {
                                        switch (payment.BankType)
                                        {
                                            case EBankType.支付宝:
                                            {
                                                payticket = new Payments.PayTicket()
                                                {
                                                    Name = "alipay",
                                                    DataFormat = Payments.EPayDataFormat.QrCode,
                                                    DataContent = payment.QRCode,
                                                    Message = "支付宝扫码转账",
                                                };
                                        
                                                payTickets.Add(payticket);
                                            }
                                                break;
                                            case EBankType.微信:
                                            { 
                                                payticket = new Payments.PayTicket()
                                                {
                                                    Name = "wechat",
                                                    DataFormat = Payments.EPayDataFormat.QrCode,
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
                                                payticket = new Payments.PayTicket()
                                                {
                                                    Name = "unionpay",
                                                    DataFormat = Payments.EPayDataFormat.QrCode,
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
                                                payticket = new Payments.PayTicket()
                                                {
                                                    Name = "alipay",
                                                    DataFormat = Payments.EPayDataFormat.QrCode,
                                                    DataContent = SiteContext.Payment.TransferToBank(payment, order.PayAmount),
                                                    Message = "支付宝扫码转账",
                                                };
                                        
                                                payTickets.Add(payticket);
                                            }
                                                break;
                                            default:
                                            { 
                                                payticket = new Payments.PayTicket()
                                                {
                                                    Name = payment.Name.ToLowerInvariant(),
                                                    DataFormat = Payments.EPayDataFormat.QrCode,
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
                        }
                        
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
