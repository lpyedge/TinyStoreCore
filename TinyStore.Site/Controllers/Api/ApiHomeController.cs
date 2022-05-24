using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using TinyStore.BLL;
using TinyStore.Model;

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
                    ClientIP = Utils.RequestInfo._ClientIP(Request).ToString(),
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
        public IActionResult OrderInfo([FromForm] string orderId)
        {
            OrderModel order = OrderBLL.QueryModelById(orderId);
            if (order == null)
                return ApiResult.RCode("订单不存在");
            order.StockList = new List<Model.StockOrderView>();
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
        [ResponseCache(Location =ResponseCacheLocation.Any,Duration = 600)]
        [HttpGet("/" + SiteContext.Resource.ResourcePrefix + "/{Model}/{Id}/{Name}")]
        public dynamic Resouce(string model, string id, string name)
        {
            if (!string.IsNullOrWhiteSpace(model) && !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(name))
                return SiteContext.Resource.Result(model, id, name, false);
            return new NotFoundResult();
        }

        [ResponseCache(NoStore = true,Location = ResponseCacheLocation.None)]
        [HttpGet("/" + SiteContext.Resource.ResourcePrefix + "_" + SiteContext.Resource.Temp + "/{Model}/{Id}/{Name}")]
        public dynamic ResouceTemp(string model, string id, string name)
        {
            if (!string.IsNullOrWhiteSpace(model) && !string.IsNullOrWhiteSpace(id) && !string.IsNullOrWhiteSpace(name))
                return SiteContext.Resource.Result(model, id, name, true);
            return new NotFoundResult();
        }
    }
}