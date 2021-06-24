using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using LPayments;
using Marvin.Cache.Headers;
using Microsoft.AspNetCore.Mvc;
using TinyStore.BLL;
using TinyStore.Model;
using TinyStore.Model.Extend;
using TinyStore.Utils;

namespace TinyStore.Site.Controllers.Api
{
    [ApiController]
    [MultipleSubmit("Resouce", "ResouceTemp")]
    [Produces("application/json")]
    [Route("Api/[action]")]
    public class ApiHomeController : ControllerBase
    { 
        [HttpPost]
        public IActionResult OrderInsert([FromForm] string productid, [FromForm] int quantity,
            [FromForm] string contact, [FromForm] string message)
        {
            if (string.IsNullOrWhiteSpace(productid) || string.IsNullOrWhiteSpace(contact) || quantity <= 0)
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);
            try
            {
                if (!Global.Regex.Email.IsMatch(contact))
                    return ApiResult.RCode(ApiResult.ECode.DataFormatError);

                ProductModel product = ProductBLL.QueryModelByProductId(productid);
                if (product == null)
                    return ApiResult.RCode(ApiResult.ECode.TargetNotExist);
                StoreModel store = StoreBLL.QueryModelByStoreId(product.StoreId);
                if (store == null)
                    return ApiResult.RCode(ApiResult.ECode.TargetNotExist);
                UserModel user = UserBLL.QueryModelById(product.UserId);
                if (user == null)
                    return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

                if (quantity < product.QuantityMin)
                    return ApiResult.RCode(ApiResult.ECode.Fail);

                if (product.DeliveryType == EDeliveryType.卡密)
                {
                    var stockcount = StockBLL.QueryCountBySupplyIdCanUse(product.SupplyId);
                    if (quantity > stockcount)
                        return ApiResult.RCode(ApiResult.ECode.Fail);
                }

                var order = new OrderModel
                {
                    OrderId = Global.Generator.DateId(1),

                    StoreId = store.StoreId,
                    UserId = user.UserId,
                    SupplyId = product.SupplyId,
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Amount = product.Amount,
                    Contact = contact,
                    Cost = product.Cost,
                    Quantity = quantity,

                    CreateDate = DateTime.Now,

                    Memo = "",
                    Message = message,
                    ClientIP = RequestInfo._ClientIP(Request).ToString(),
                    UserAgent = Request.Headers["User-Agent"].ToString(),
                    AcceptLanguage = Request.Headers["Accept-Language"].ToString(),

                    LastUpdateDate = DateTime.Now
                };

                OrderBLL.Insert(order);

                return ApiResult.RData(order);
            }
            catch (Exception ex)
            {
                return ApiResult.RCode(ApiResult.ECode.UnKonwError);
            }
        }

        [HttpPost]
        public IActionResult OrderPay([FromForm] string orderId, [FromForm] string paymentType)
        {
            OrderModel order = OrderBLL.QueryModelById(orderId);
        
            if (order == null || order.IsPay)
                return ApiResult.RCode(ApiResult.ECode.Fail);
        
            StoreModel store = StoreBLL.QueryModelById(order.StoreId);
            if (store == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);
        
            Payment payment = store.PaymentList.FirstOrDefault(p =>
                p.IsEnable && string.Equals(p.Name, paymentType, StringComparison.OrdinalIgnoreCase));
        
            if (payment == null)
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);

            var payorderid = Global.Generator.DateId(1);
            if (paymentType != order.PaymentType)
            {
                order.PayOrderId = payorderid;
                order.PaymentType = paymentType;
                OrderBLL.Update(order);
            }

            

            // var payticket = SiteContext.Payment.GetPayment(payment.Name).Pay(order.OrderId,(order.Amount*order.Quantity-order.Reduction),ECurrency.CNY,order.Name,clietnIP,
            //     Url.ActionLink("Order","Home",new {orderid=order.OrderId}),
            //     Url.ActionLink("PayNotify","ApiHome",new {payname=payment.Name}));

            LPayments.PayTicket payticket;
            if (payment.IsSystem)
            {
                var pay = SiteContext.Payment.GetPayment(payment.Name);
                payticket = pay.Pay(payorderid, order.PayAmount, ECurrency.CNY, order.Name,
                    Utils.RequestInfo._ClientIP(Request),
                    "https://" + SiteContext.Config.SiteDomain + Url.ActionLink("Order","Home",new {orderid=order.OrderId}),
                    "https://" + SiteContext.Config.SiteDomain + Url.ActionLink("PayNotify","ApiHome",new {payname=payment.Name}));
                
                payticket.Token = (pay as IPayChannel).Platform.ToString().ToLowerInvariant();
            }
            else
            {
                switch (payment.BankType)
                {
                    case EBankType.支付宝:
                    {
                        payticket = new LPayments.PayTicket()
                        {
                            Action = EAction.QrCode,
                            Uri = payment.QRCode,
                            Datas = new Dictionary<string, string>(),
                            Success = true,
                            Message = "支付宝扫码转账",
                            Token = "alipay"
                        };
                    }
                        break;
                    case EBankType.微信:
                    { 
                        payticket = new LPayments.PayTicket()
                        {
                            Action = EAction.QrCode,
                            Uri = payment.QRCode,
                            Datas = new Dictionary<string, string>(),
                            Success = true,
                            Message = "微信扫码转账",
                            Token = "wechat"
                        };
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
                            Action = EAction.QrCode,
                            Uri = payment.QRCode,
                            Datas = new Dictionary<string, string>(),
                            Success = true,
                            Message = "银联扫码转账(云闪付,银行App)",
                            Token = "unionpay"
                        };
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
                            Action = EAction.QrCode,
                            Uri = SiteContext.Payment.TransferToBank(payment, order.PayAmount),
                            Datas = new Dictionary<string, string>(),
                            Success = true,
                            Message = "支付宝扫码转账",
                            Token = "alipay"
                        };
                    }
                        break;
                    default:
                    { 
                        payticket = new LPayments.PayTicket()
                        {
                            Action = EAction.QrCode,
                            Uri = payment.QRCode,
                            Datas = new Dictionary<string, string>(),
                            Success = true,
                            Message = payment.Memo,
                            Token = ""
                        };
                    }
                        break;
                }
                
            }
            
            return ApiResult.RData(payticket);
           
        }
        
        [HttpPost]
        public IActionResult OrderInfo([FromForm] string orderId)
        {
            OrderModel order = OrderBLL.QueryModelById(orderId);
            if (order == null)
                return ApiResult.RCode("订单不存在");
            order.StockList = new List<StockOrder>();
            StoreModel store = StoreBLL.QueryModelByStoreId(order.StoreId);
            order.StoreUniqueId = store?.UniqueId;
            order.StoreName = store?.Name;

            return ApiResult.RCode("");
        }

        [Route("/paynotify/{payname}")]
        public async Task<IActionResult> PayNotify(string payname)
        {
            using (Stream stream = Request.Body)
            {
                var buffer = new byte[Request.ContentLength.Value];
                
                await stream.ReadAsync(buffer, 0, buffer.Length);
                
                var body = Encoding.UTF8.GetString(buffer);
                
                var msg = SiteContext.Payment.Notify(payname,
                    Request.Form.ToDictionary(p => p.Key
                        , p => p.Value.ToString()),
                    Request.Query.ToDictionary(p => p.Key
                        , p => p.Value.ToString()),
                    Request.Headers.ToDictionary(p => p.Key
                        , p => p.Value.ToString()),
                    body,Utils.RequestInfo._ClientIP(Request).ToString());
                
                return Content(msg);
            }
        }

        //[Marvin.Cache.Headers.HttpCacheExpiration(CacheLocation = Marvin.Cache.Headers.CacheLocation.Public,MaxAge = 60)]
        [HttpCacheExpiration(CacheLocation = CacheLocation.Public, MaxAge = 600)]
        [HttpCacheValidation(MustRevalidate = true)]
        [HttpGet("/" + SiteContext.Resource.ResourcePrefix + "/{Model}/{Id}/{Name}")]
        public dynamic Resouce(string model, string id, string name)
        {
            if (!string.IsNullOrWhiteSpace(model) && !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(name))
                return SiteContext.Resource.Result(model, id, name, false);
            return new NotFoundResult();
        }

        [HttpCacheExpiration(NoStore = true)]
        [HttpGet("/" + SiteContext.Resource.ResourcePrefix + "_" + SiteContext.Resource.Temp + "/{Model}/{Id}/{Name}")]
        public dynamic ResouceTemp(string model, string id, string name)
        {
            if (!string.IsNullOrWhiteSpace(model) && !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(name))
                return SiteContext.Resource.Result(model, id, name, true);
            return new NotFoundResult();
        }
    }
}