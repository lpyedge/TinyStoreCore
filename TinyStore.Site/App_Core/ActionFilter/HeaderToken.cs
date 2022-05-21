using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace TinyStore.Site
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public abstract class HeaderToken : ActionFilterAttribute
    {
          //public T ModelGet<T>(IDictionary<object, object> items, string itemkey) where T : class, new()
        //{
        //    if (string.IsNullOrWhiteSpace(itemkey) && items.ContainsKey(itemkey))
        //    {
        //        return (items[itemkey] as T);
        //    }
        //    return default(T);
        //}

        //public void ModelSet<T>(IDictionary<object, object> items, string itemkey,dynamic model) where T : class, new()
        //{
        //    if (string.IsNullOrWhiteSpace(itemkey) )
        //    {
        //        items[itemkey] = model;
        //    }
        //}


        public string[] _Ignoreactions
        {
            get;
            protected set;
        }

        /// <summary>
        ///     可以传参传入要忽略的Action名称,传入的Action不会执行判断
        /// </summary>
        /// <param name="headerTokenKey"></param>
        /// <param name="itemKey"></param>
        /// <param name="ignoreactions"></param>
        public HeaderToken(string headerTokenKey, string itemKey, params string[] ignoreactions)
        {
            HeaderTokenKey = headerTokenKey;
            ItemKey = itemKey;
            _Ignoreactions = ignoreactions.Select(p => p.ToLowerInvariant()).ToArray();
        }

        private string HeaderTokenKey { get; }
        private string ItemKey { get; }

        internal abstract void OnTokenGet(ActionExecutingContext context, TokenData tokendata);

        internal class TokenData
        {
            public string Id { get; set; }

            public string Key { get; set; }
            
            public string Extra { get; set; }
        }

        internal static TokenData FromHeaderToken(string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                var buff = Convert.FromBase64String(token);
                var str = Encoding.UTF8.GetString(buff);
                return Utils.JsonUtility.Deserialize<TokenData>(str);
            }

            return null;
        }

        internal static string ToHeaderToken(TokenData tokendata)
        {
            if (tokendata != null)
            {
                var str = Utils.JsonUtility.Serialize(tokendata);
                var buff = Encoding.UTF8.GetBytes(str);
                return Convert.ToBase64String(buff);
            }

            return "";
        }
        
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var actionname = ((ControllerActionDescriptor) context.ActionDescriptor).ActionName.ToLowerInvariant();
            if (!_Ignoreactions.Contains(actionname))
            {
                var headerTokenStr = context.HttpContext.Request.Headers[HeaderTokenKey];
                TokenData tokendata = null;
                if (!string.IsNullOrWhiteSpace(headerTokenStr))
                {
                    tokendata = FromHeaderToken(headerTokenStr);
                }
                OnTokenGet(context, tokendata);
            }
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            if (!context.HttpContext.Response.Headers.ContainsKey("Access-Control-Allow-Headers"))
                context.HttpContext.Response.Headers["Access-Control-Allow-Headers"] = HeaderTokenKey;
            else if(!context.HttpContext.Response.Headers["Access-Control-Allow-Headers"].Contains(HeaderTokenKey))
                context.HttpContext.Response.Headers["Access-Control-Allow-Headers"] += "," + HeaderTokenKey;

            if (!context.HttpContext.Response.Headers.ContainsKey("Access-Control-Expose-Headers"))
                context.HttpContext.Response.Headers["Access-Control-Expose-Headers"] = HeaderTokenKey;
            else if(!context.HttpContext.Response.Headers["Access-Control-Expose-Headers"].Contains(HeaderTokenKey))
                context.HttpContext.Response.Headers["Access-Control-Expose-Headers"] += "," + HeaderTokenKey;
        }
    }
}