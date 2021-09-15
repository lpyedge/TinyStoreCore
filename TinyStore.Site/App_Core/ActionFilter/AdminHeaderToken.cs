using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace TinyStore.Site
{
    public sealed class AdminHeaderToken : HeaderToken
    {
        public const string HeaderKey = "AdminToken";
        public const string ItemKey = "Admin";
        
        /// <summary>
        /// 可以传参传入要忽略的Action名称,传入的Action不会执行判断
        /// </summary>
        /// <param name="ignoreactions"></param>
        public AdminHeaderToken(params string[] ignoreactions) : base(HeaderKey, ItemKey,  ignoreactions)
        {
        }

        internal override void OnTokenGet(ActionExecutingContext context, TokenData tokendata)
        {
            var ispass = false;
            if (tokendata != null)
            {
                try
                {
                    var admin = BLL.AdminBLL.QueryModelById(int.Parse(tokendata.Id));
                    if (admin != null)
                    {
                        var isvalidate =
#if DEBUG
                            true;
#else
                        admin.ClientKey == tokendata.Key;
#endif
                        if (isvalidate)
                        {
                            ispass = true;
                            context.HttpContext.Items[ItemKey] = admin;
                        }
                    }
                }
                catch
                {
                }
            }

            if (!ispass)
            {
                context.Result = ApiResult.RCode("请登录后操作");
                context.HttpContext.Response.StatusCode = 200;
            }
        }
    }
}