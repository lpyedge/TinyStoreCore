using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;

namespace TinyStore.Site
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public class MultipleSubmitAttribute : ActionFilterAttribute
    {
        /// <summary>
        /// 可以传参传入要忽略的Action名称,传入的Action不会执行判断
        /// </summary>
        /// <param name="ignoreactions"></param>
        public MultipleSubmitAttribute(params string[] ignoreactions)
        {
            _ignoreactions = ignoreactions.Select(p => p.ToUpperInvariant()).ToArray();
            cacheMilliseconds = 500;
        }

        public int cacheMilliseconds;

        private string[] _ignoreactions;


        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var controller = (Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor)context?.ActionDescriptor;
            if (controller != null)
            {
                var actionname = controller.ActionName.ToUpperInvariant();
                if (!_ignoreactions.Contains(actionname))
                {
                    var clientipaddress = context.HttpContext.Connection.RemoteIpAddress.ToString();
                    var clientuseragent = context.HttpContext.Request.Headers["User-Agent"].ToString();
                    var querystr = context.HttpContext.Request.QueryString.Value;

                    var formstr = string.Equals(context.HttpContext.Request.Method, "POST", StringComparison.OrdinalIgnoreCase) ?
                        (context.HttpContext.Request.HasFormContentType ? context.HttpContext.Request.Form.Aggregate("", (s, item) => s += item.Key + "=" + item.Value.ToString() + "&") : "")
                        : "";
                    var requesthash = Global.Hash(context.HttpContext.Request.Path.Value + "$" + clientipaddress + "$" + clientuseragent + "$" + querystr + "$" + formstr);

                    if (Utils.MemoryCacher.TryGet(requesthash, out object data))
                    {
                        if (string.Equals(context.HttpContext.Request.Method, "POST", StringComparison.OrdinalIgnoreCase))
                        {
                            context.Result = ApiResult.RCode( "通信接口繁忙" );
                        }
                        else
                        {
                            context.Result = new ForbidResult();
                        }
                    }
                    Utils.MemoryCacher.Set(requesthash, this, Utils.MemoryCacher.CacheItemPriority.Low, null, TimeSpan.FromMilliseconds(cacheMilliseconds));
                }
                //else
                //{
                //    var controllername = controller.ControllerName.ToUpperInvariant();
                //    var clientipaddress = context.HttpContext.Connection.RemoteIpAddress.ToString();
                //    var clientuseragent = context.HttpContext.Request.Headers["User-Agent"].ToString();
                //    var querystr = context.HttpContext.Request.QueryString.Value;
                //    var formstr = context.HttpContext.Request.Method == "POST" ? (context.HttpContext.Request.HasFormContentType ? Global.HashArray(Global.Json.Serialize(context.HttpContext.Request.Form)) : "") : "";
                //    var requesthash = Global.HashArray(controllername + "$" + actionname + "$" + clientipaddress + "$" + clientuseragent + "$" + querystr + "$" + formstr);

                //    if (MemoryCacher.TryGet(requesthash, out object data))
                //    {
                //        context.Result = new JsonResult(new Result() { Code = Result.ECode.ApiBusy });
                //    }
                //    MemoryCacher.Set(requesthash, new object(), Microsoft.Extensions.Caching.Memory.CacheItemPriority.Low, null, TimeSpan.FromMilliseconds(cacheMilliseconds));
                //}
            }
        }
    }
}