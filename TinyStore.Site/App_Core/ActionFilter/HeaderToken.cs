using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace TinyStore.Site
{
    [AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
    public abstract class HeaderToken : ActionFilterAttribute
    {
        public const string HeaderKey = "Token";

        /// <summary>
        /// 可以传参传入要忽略的Action名称,传入的Action不会执行判断
        /// </summary>
        /// <param name="ignoreactions"></param>
        protected HeaderToken(params string[] ignoreactions)
        {
            Ignoreactionss = ignoreactions.Select(p => p.ToLowerInvariant()).ToArray();
        }


        public abstract void OnTokenGet(ActionExecutingContext context, string token);

        private static readonly System.Security.Cryptography.SymmetricAlgorithm Provider =
            Utils.DESCrypto.Generate(nameof(HeaderToken), Utils.DESCrypto.CryptoEnum.Rijndael);

        protected class TokenModel
        {
            public string Id { get; set; }

            public string Key { get; set; }
        }

        protected static TokenModel FromToken(string token)
        {
            if (!string.IsNullOrWhiteSpace(token))
            {
                byte[] buff = Convert.FromBase64String(token);
                var str = Utils.DESCrypto.Decrypt(Provider, buff);
                return Global.Json.Deserialize<TokenModel>(str);
            }

            return null;
        }

        protected static string ToToken(TokenModel tokendata)
        {
            if (tokendata != null)
            {
                var str = Global.Json.Serialize(tokendata);
                var buff = Utils.DESCrypto.Encrypt2Byte(Provider, str);
                return Convert.ToBase64String(buff);
            }

            return "";
        }

        protected string[] Ignoreactionss;

        public override void OnActionExecuting(ActionExecutingContext context)
        {
            base.OnActionExecuting(context);

            var actionname =
                ((Microsoft.AspNetCore.Mvc.Controllers.ControllerActionDescriptor) context.ActionDescriptor).ActionName
                .ToLowerInvariant();
            if (!Ignoreactionss.Contains(actionname))
            {
                string token = context.HttpContext.Request.Headers[HeaderKey];
                OnTokenGet(context, token);
            }
        }

        // public override void OnActionExecuted(ActionExecutedContext context)
        // {
        //     base.OnActionExecuted(context);
        //     var tokendata = context.HttpContext.Items[ItemKey] as TokenModel;
        //     if (tokendata != null)
        //     {
        //         context.HttpContext.Response.Headers.Add("Access-Control-Allow-Headers", HeaderKey);
        //         context.HttpContext.Response.Headers.Add("Access-Control-Expose-Headers", HeaderKey);
        //         context.HttpContext.Response.Headers.Add(HeaderKey, HeaderToken.ToToken(tokendata));
        //     }
        // }

        public static void SetHeaderToken(HttpContext httpContext, string Id, string Key)
        {
            var tokendata = new TokenModel()
            {
                Id = Id,
                Key = Key
            };
            httpContext.Response.Headers.Add("Access-Control-Allow-Headers", HeaderKey);
            httpContext.Response.Headers.Add("Access-Control-Expose-Headers", HeaderKey);
            httpContext.Response.Headers.Add(HeaderKey, HeaderToken.ToToken(tokendata));
        }
    }
}