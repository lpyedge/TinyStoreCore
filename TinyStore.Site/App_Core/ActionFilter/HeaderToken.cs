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

        static System.Security.Cryptography.SymmetricAlgorithm Provider =>
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

        public static void SetHeaderToken(HttpContext httpContext, string Id, string Key)
        {
            var tokendata = new TokenModel()
            {
                Id = Id,
                Key = Key
            };
            if (!httpContext.Response.Headers.ContainsKey("Access-Control-Allow-Headers"))
            {
                httpContext.Response.Headers["Access-Control-Allow-Headers"] = HeaderKey;
            }
            else if(!httpContext.Response.Headers["Access-Control-Allow-Headers"].Contains(HeaderKey))
            {
                httpContext.Response.Headers["Access-Control-Allow-Headers"] += "," + HeaderKey;
            }
            if (!httpContext.Response.Headers.ContainsKey("Access-Control-Expose-Headers"))
            {
                httpContext.Response.Headers["Access-Control-Expose-Headers"] = HeaderKey;
            }
            else if(!httpContext.Response.Headers["Access-Control-Expose-Headers"].Contains(HeaderKey))
            {
                httpContext.Response.Headers["Access-Control-Expose-Headers"] += "," + HeaderKey;
            }
            httpContext.Response.Headers[HeaderKey] = HeaderToken.ToToken(tokendata);
        }
    }
}