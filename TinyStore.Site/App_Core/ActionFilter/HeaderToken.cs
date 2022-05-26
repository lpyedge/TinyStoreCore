using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Controllers;

namespace TinyStore.Site;


// [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
// public abstract class HeaderToken : ActionFilterAttribute
// {
//     /// <summary>
//     ///     可以传参传入要忽略的Action名称,传入的Action不会执行判断
//     /// </summary>
//     /// <param name="tokenKey"></param>
//     /// <param name="itemKey"></param>
//     /// <param name="ignoreactions"></param>
//     public HeaderToken(string tokenKey, string itemKey, params string[] ignoreactions)
//     {
//         TokenKey = tokenKey;
//         ItemKey = itemKey;
//         _Ignoreactions = ignoreactions.Select(p => p.ToLowerInvariant()).ToArray();
//     }
//
//     public string[] _Ignoreactions { get; protected set; }
//
//     private string TokenKey { get; }
//     private string ItemKey { get; }
//
//     internal abstract void OnTokenGet(ActionExecutingContext context, TokenData tokendata);
//
//     internal static TokenData FromHeaderToken(string token)
//     {
//         if (!string.IsNullOrWhiteSpace(token))
//         {
//             var buff = Convert.FromBase64String(token);
//             var str = Encoding.UTF8.GetString(buff);
//             return Utils.JsonUtility.Deserialize<TokenData>(str);
//         }
//
//         return null;
//     }
//
//     internal static string ToHeaderToken(TokenData tokendata)
//     {
//         if (tokendata != null)
//         {
//             var str = Utils.JsonUtility.Serialize(tokendata);
//             var buff = Encoding.UTF8.GetBytes(str);
//             return Convert.ToBase64String(buff);
//         }
//
//         return "";
//     }
//
//     public override void OnActionExecuting(ActionExecutingContext context)
//     {
//         base.OnActionExecuting(context);
//
//         var actionname = ((ControllerActionDescriptor) context.ActionDescriptor).ActionName.ToLowerInvariant();
//         if (!_Ignoreactions.Contains(actionname))
//         {
//             //从header中取token字符串
//             var headerTokenStr = context.HttpContext.Request.Headers[TokenKey];
//             //从cookies中取token字符串
//             var cookiesTokenStr = context.HttpContext.Request.Cookies[TokenKey];
//             TokenData tokendata = null;
//             if (!string.IsNullOrWhiteSpace(headerTokenStr)) 
//                 tokendata = FromHeaderToken(headerTokenStr);
//             else if(!string.IsNullOrWhiteSpace(cookiesTokenStr))
//                 tokendata = FromHeaderToken(cookiesTokenStr);
//             OnTokenGet(context, tokendata);
//         }
//     }
//
//     public override void OnActionExecuted(ActionExecutedContext context)
//     {
//         base.OnActionExecuted(context);
//         if (!context.HttpContext.Response.Headers.ContainsKey("Access-Control-Allow-Headers"))
//             context.HttpContext.Response.Headers["Access-Control-Allow-Headers"] = TokenKey;
//         else if (!context.HttpContext.Response.Headers["Access-Control-Allow-Headers"].Contains(TokenKey))
//             context.HttpContext.Response.Headers["Access-Control-Allow-Headers"] += "," + TokenKey;
//
//         if (!context.HttpContext.Response.Headers.ContainsKey("Access-Control-Expose-Headers"))
//             context.HttpContext.Response.Headers["Access-Control-Expose-Headers"] = TokenKey;
//         else if (!context.HttpContext.Response.Headers["Access-Control-Expose-Headers"].Contains(TokenKey))
//             context.HttpContext.Response.Headers["Access-Control-Expose-Headers"] += "," + TokenKey;
//     }
//
//     public class TokenData
//     {
//         public string Id { get; set; }
//
//         public string Key { get; set; }
//
//         public string Extra { get; set; }
//     }
// }

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public abstract class HeaderToken : ActionFilterAttribute
    {
        /// <summary>
        ///     可以传参传入要忽略的Action名称,传入的Action不会执行判断
        /// </summary>
        /// <param name="tokenKey"></param>
        /// <param name="itemKey"></param>
        /// <param name="ignoreactions"></param>
        public HeaderToken(string tokenKey, string itemKey, params string[] ignoreactions)
        {
            TokenKey = tokenKey;
            ItemKey = itemKey;
            IgnoreActions = ignoreactions;
        }

        private string[] _ignoreActions;

        public string[] IgnoreActions {
            get {
                return _ignoreActions;
            }
            set {
                _ignoreActions = value.Select(p => p.ToLowerInvariant()).ToArray();
            } }

        private string TokenKey { get; set; }
        private string ItemKey { get; set; }

        internal void OnTokenGet(ActionExecutingContext context, TokenData tokendata)
        {
            dynamic model = null;
            if (tokendata != null)
            {
                model = TokenToModel(tokendata);
            }

            if (tokendata != null && model != null)
            {
                context.HttpContext.Items[ItemKey] = model;
            }
            else
            {
                context.Result = ApiResult.RCode(ApiResult.ECode.OffLine);
                context.HttpContext.Response.StatusCode = 200;
            }
        }

        public static TokenData FromHeaderToken(string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                var buff = Convert.FromBase64String(token);
                var str = Encoding.UTF8.GetString(buff);
                return System.Text.Json.JsonSerializer.Deserialize<TokenData>(str);
            }

            return null;
        }

        public static string ToHeaderToken(TokenData tokendata)
        {
            if (tokendata != null)
            {
                var str = System.Text.Json.JsonSerializer.Serialize(tokendata);
                var buff = Encoding.UTF8.GetBytes(str);
                return Convert.ToBase64String(buff);
            }

            return "";
        }

        protected Func<TokenData, dynamic> TokenToModel { get; set; }

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var actionname = ((ControllerActionDescriptor)context.ActionDescriptor).ActionName.ToLowerInvariant();
            if (!_ignoreActions.Contains(actionname))
            {
                //从header中取token字符串
                var headerTokenStr = context.HttpContext.Request.Headers[TokenKey];
                //从cookies中取token字符串
                var cookiesTokenStr = context.HttpContext.Request.Cookies[TokenKey];
                TokenData tokendata = null;
                if (!string.IsNullOrWhiteSpace(headerTokenStr))
                    tokendata = FromHeaderToken(headerTokenStr);
                else if (!string.IsNullOrWhiteSpace(cookiesTokenStr))
                    tokendata = FromHeaderToken(cookiesTokenStr);
                OnTokenGet(context, tokendata);
            }
        }

        public override void OnActionExecuted(ActionExecutedContext context)
        {
            base.OnActionExecuted(context);
            if (!context.HttpContext.Response.Headers.ContainsKey("Access-Control-Allow-Headers"))
                context.HttpContext.Response.Headers["Access-Control-Allow-Headers"] = TokenKey;
            else if (!context.HttpContext.Response.Headers["Access-Control-Allow-Headers"].Contains(TokenKey))
                context.HttpContext.Response.Headers["Access-Control-Allow-Headers"] += "," + TokenKey;

            if (!context.HttpContext.Response.Headers.ContainsKey("Access-Control-Expose-Headers"))
                context.HttpContext.Response.Headers["Access-Control-Expose-Headers"] = TokenKey;
            else if (!context.HttpContext.Response.Headers["Access-Control-Expose-Headers"].Contains(TokenKey))
                context.HttpContext.Response.Headers["Access-Control-Expose-Headers"] += "," + TokenKey;
        }

        public class TokenData
        {
            public string Id { get; set; }

            public string Key { get; set; }

            public string Extra { get; set; }
        }
    }
