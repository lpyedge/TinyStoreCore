using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TinyStore.Model.Extend;

namespace TinyStore.Site.Controllers.Api
{
    [ApiController]
    [MultipleSubmit("Resouce","ResouceTemp")]
    [Produces("application/json")]
    [Route("Api/[action]")]
    public class ApiHomeController : ControllerBase
    {
        public IActionResult OrderInsert([FromForm] string Productid, [FromForm] int Quantity,
            [FromForm] string Contact, [FromForm] string NoticeAccount, [FromForm] string PaymentType)
        {
            if (string.IsNullOrWhiteSpace(Productid) || string.IsNullOrWhiteSpace(Contact) ||
                string.IsNullOrWhiteSpace(NoticeAccount) || Quantity <= 0)
            {
                return ApiResult.RCode("传参错误");
            }
            else
            {
                try
                {
                    if (string.IsNullOrEmpty(NoticeAccount))
                        return ApiResult.RCode("联系邮箱不能为空");
                    if (!Global.Regex.Email.IsMatch(NoticeAccount))
                        return ApiResult.RCode("邮箱格式不正确");
                    var product = BLL.ProductBLL.QueryModelByProductId(Productid);
                    if (product == null)
                        return ApiResult.RCode("产品不存在");
                    var store = BLL.StoreBLL.QueryModelByStoreId(product.StoreId);
                    if (store == null)
                        return ApiResult.RCode("店铺不存在");
                    var user = BLL.UserBLL.QueryModelById(product.UserId);
                    if (user == null)
                        return ApiResult.RCode("商户不存在");

                    if (product.DeliveryType == EDeliveryType.卡密)
                    {
                        var stockcount =
                            BLL.StockBLL.QueryCountBySupplyIdCanUse(product.SupplyId);
                        if (stockcount < product.QuantityMin)
                            return ApiResult.RCode("库存数量不足");
                        if (Quantity > stockcount)
                            return ApiResult.RCode("下单数量不能大于库存数量");
                        if (Quantity < product.QuantityMin)
                            return ApiResult.RCode("下单数量不能小于最小下单量");
                    }
                    else if (Quantity < product.QuantityMin)
                        return ApiResult.RCode("下单数量不能小于最小下单量");

                    if (string.IsNullOrEmpty(Contact))
                        return ApiResult.RCode("联系方式不能为空");

                    var price = product.Amount;

                    var order = new Model.OrderModel
                    {
                        Amount = price * Quantity,
                        ClientIP = SiteContext.RequestInfo._ClientIP(Request).ToString(),
                        Contact = Contact,
                        Cost = product.Cost * Quantity,
                        CreateDate = DateTime.Now,
                        Discount = 0,
                        Income = 0,
                        IsPay = false,
                        IsDelivery = false,
                        Memo = product.Memo,
                        Name = product.Name,
                        NoticeAccount = NoticeAccount,
                        OrderId = Global.Generator.DateId(2),
                        ProductId = product.ProductId,
                        Quantity = Quantity,
                        ReturnAmount = 0,
                        ReturnDate = DateTime.Now,
                        DeliveryDate = DateTime.Now,
                        StockList = new List<StockOrder>(),
                        StoreId = store.StoreId,
                        UserId = user.UserId,
                        TranId = string.Empty,
                        UserAgent = Request.Headers["User-Agent"].ToString(),
                        AcceptLanguage = Request.Headers["Accept-Language"].ToString(),
                        IsSettle = false,
                        SettleDate = DateTime.Now,
                        SupplyId = product.SupplyId,
                        LastUpdateDate = DateTime.Now,
                        StoreUniqueId = store.UniqueId,
                    };

                    BLL.OrderBLL.Insert(order);
#if DEBUG
                    //order.IsPay = true;
                    //Delivery(order);
#endif
                    if (!string.IsNullOrWhiteSpace(PaymentType))
                    {
                        // if (order.IsPay)
                        //     return ApiResult.RCode( "订单不存在或已付款");

                        Model.Extend.Payment payment = store.PaymentList.FirstOrDefault(p =>
                            p.IsEnable && string.Equals(p.Name, PaymentType, StringComparison.OrdinalIgnoreCase));

                        if (payment == null)
                            return ApiResult.RCode("支付方式不存在");

                        if (PaymentType != order.PaymentType)
                        {
                            order.PaymentFee = order.Amount * payment.Rate;
                            order.PaymentType = PaymentType;
                            BLL.OrderBLL.Update(order);
                        }

                        return ApiResult.RData(SiteContext.OrderHelper.GetPayTicket(order.PaymentType,order.OrderId,order.Amount));
                    }
                    else
                    {
                        return ApiResult.RData(order.OrderId);
                    }
                }
                catch (Exception ex)
                {
                    return ApiResult.RCode(ex.Message);
                }
            }
        }


        public IActionResult OrderPay([FromForm] string OrderId, [FromForm] string PaymentType)
        {
            var order = BLL.OrderBLL.QueryModelByOrderId(OrderId);

            if (order == null || order.IsPay)
                return ApiResult.RCode("订单不存在或已付款");

            var store = BLL.StoreBLL.QueryModelById(order.StoreId);
            if (store == null)
                return ApiResult.RCode("店铺已关闭");

            Model.Extend.Payment payment = store.PaymentList.FirstOrDefault(p =>
                p.IsEnable && string.Equals(p.Name, PaymentType, StringComparison.OrdinalIgnoreCase));

            if (payment == null)
                return ApiResult.RCode("支付方式不存在");

            if (PaymentType != order.PaymentType)
            {
                order.PaymentFee = order.Amount * payment.Rate;
                order.PaymentType = PaymentType;
                BLL.OrderBLL.Update(order);
            }

            return ApiResult.RData(SiteContext.OrderHelper.GetPayTicket(order.PaymentType,order.OrderId,order.Amount));
        }

        public IActionResult OrderInfo([FromForm] string OrderId)
        {
            var order = BLL.OrderBLL.QueryModelByOrderId(OrderId);
            if (order == null)
                return ApiResult.RCode("订单不存在");
            order.StockList = new List<StockOrder>();
            var store = BLL.StoreBLL.QueryModelByStoreId(order.StoreId);
            order.StoreUniqueId = store?.UniqueId;
            order.StoreName = store?.Name;

            return ApiResult.RCode("");
        }

        [HttpPost()]
        public async Task<IActionResult> PayNotify()
        {
            using (var stream = Request.Body)
            {
                byte[] buffer = new byte[Request.ContentLength.Value];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                var body = Encoding.UTF8.GetString(buffer);
                return new JsonResult(SiteContext.OrderHelper.Notify(body));
            }
        }

        //[Marvin.Cache.Headers.HttpCacheExpiration(CacheLocation = Marvin.Cache.Headers.CacheLocation.Public,MaxAge = 60)]
        [Marvin.Cache.Headers.HttpCacheValidation(MustRevalidate = true, NoCache = true)]
        [HttpGet("/" + SiteContext.Resource.ResourcePrefix + "/{Model}/{Id}/{Name}")]
        public dynamic Resouce(string Model, string Id, string Name)
        {
            if (!string.IsNullOrWhiteSpace(Model) && !string.IsNullOrWhiteSpace(Id) && !string.IsNullOrWhiteSpace(Name))
            {
                return SiteContext.Resource.Result(Model, Id, Name, false);
            }
            else
            {
                return new NotFoundResult();
            }
        }

        [Marvin.Cache.Headers.HttpCacheExpiration(NoStore = true)]
        [HttpGet("/" + SiteContext.Resource.ResourcePrefix + "_" + SiteContext.Resource.Temp + "/{Model}/{Id}/{Name}")]
        public dynamic ResouceTemp(string Model, string Id, string Name)
        {
            if (!string.IsNullOrWhiteSpace(Model) && !string.IsNullOrWhiteSpace(Id) && !string.IsNullOrWhiteSpace(Name))
            {
                return SiteContext.Resource.Result(Model, Id, Name, true);
            }
            else
            {
                return new NotFoundResult();
            }
        }
    }
}