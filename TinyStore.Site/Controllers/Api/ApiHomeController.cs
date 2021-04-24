using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
        public IActionResult OrderInsert([FromForm] string productid, [FromForm] int quantity,
            [FromForm] string contact, [FromForm] string message, [FromForm] string paymentType)
        {
            if (string.IsNullOrWhiteSpace(productid) || string.IsNullOrWhiteSpace(contact) ||
                string.IsNullOrWhiteSpace(paymentType) || quantity <= 0)
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);
            try
            {
                if (string.IsNullOrEmpty(message))
                    return ApiResult.RCode(ApiResult.ECode.DataFormatError);
                if (!Global.Regex.Email.IsMatch(message))
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

                var price = product.Amount;

                var order = new OrderModel
                {
                    OrderId = Global.Generator.DateId(1),

                    StoreId = store.StoreId,
                    UserId = user.UserId,
                    SupplyId = product.SupplyId,
                    ProductId = product.ProductId,
                    Name = product.Name,
                    Amount = price,
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
#if DEBUG
                //order.IsPay = true;
                //Delivery(order);
#endif
                if (!string.IsNullOrWhiteSpace(paymentType))
                {
                    // if (order.IsPay)
                    //     return ApiResult.RCode( "订单不存在或已付款");

                    Payment payment = store.PaymentList.FirstOrDefault(p =>
                        p.IsEnable && string.Equals(p.Name, paymentType, StringComparison.OrdinalIgnoreCase));

                    if (payment == null)
                        return ApiResult.RCode(ApiResult.ECode.DataFormatError);

                    if (paymentType != order.PaymentType)
                    {
                        order.PaymentFee = order.Amount * payment.Rate;
                        order.PaymentType = paymentType;
                        OrderBLL.Update(order);
                    }

                    return ApiResult.RData(
                        SiteContext.OrderHelper.GetPayTicket(order.PaymentType, order.OrderId, order.Amount));
                }

                return ApiResult.RData(order);
            }
            catch (Exception ex)
            {
                return ApiResult.RCode(ApiResult.ECode.UnKonwError);
            }
        }


        public IActionResult OrderPay([FromForm] string orderId, [FromForm] string paymentType)
        {
            OrderModel order = OrderBLL.QueryModelByOrderId(orderId);

            if (order == null || order.IsPay)
                return ApiResult.RCode(ApiResult.ECode.Fail);

            StoreModel store = StoreBLL.QueryModelById(order.StoreId);
            if (store == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

            Payment payment = store.PaymentList.FirstOrDefault(p =>
                p.IsEnable && string.Equals(p.Name, paymentType, StringComparison.OrdinalIgnoreCase));

            if (payment == null)
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);

            if (paymentType != order.PaymentType)
            {
                order.PaymentFee = order.Amount * payment.Rate;
                order.PaymentType = paymentType;
                OrderBLL.Update(order);
            }

            return ApiResult.RData(
                SiteContext.OrderHelper.GetPayTicket(order.PaymentType, order.OrderId, order.Amount));
        }

        public IActionResult OrderInfo([FromForm] string orderId)
        {
            OrderModel order = OrderBLL.QueryModelByOrderId(orderId);
            if (order == null)
                return ApiResult.RCode("订单不存在");
            order.StockList = new List<StockOrder>();
            StoreModel store = StoreBLL.QueryModelByStoreId(order.StoreId);
            order.StoreUniqueId = store?.UniqueId;
            order.StoreName = store?.Name;

            return ApiResult.RCode("");
        }

        [HttpPost]
        public async Task<IActionResult> PayNotify()
        {
            using (Stream stream = Request.Body)
            {
                var buffer = new byte[Request.ContentLength.Value];
                await stream.ReadAsync(buffer, 0, buffer.Length);
                var body = Encoding.UTF8.GetString(buffer);
                return new JsonResult(SiteContext.OrderHelper.Notify(body));
            }
        }

        //[Marvin.Cache.Headers.HttpCacheExpiration(CacheLocation = Marvin.Cache.Headers.CacheLocation.Public,MaxAge = 60)]
        [HttpCacheValidation(MustRevalidate = true, NoCache = true)]
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