using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace TinyStore.Site
{
    public sealed class AdminHeaderToken : HeaderToken
    {
        /// <summary>
        /// 可以传参传入要忽略的Action名称,传入的Action不会执行判断
        /// </summary>
        /// <param name="ignoreactions"></param>
        public AdminHeaderToken(params string[] ignoreactions) : base( ignoreactions)
        {
        }

        public override void OnTokenGet(ActionExecutingContext context, string token)
        {
            var ispass = false;
            if (!string.IsNullOrWhiteSpace(token))
            {
                var data = HeaderToken.FromToken(token);
                try
                {
                    var admin = BLL.AdminBLL.QueryModelById(int.Parse(data.Id));
                    if (admin != null)
                    {
                        var isvalidate =
#if DEBUG
                            true;
#else
                        admin.ClientKey == data.Key;
#endif
                        if (isvalidate)
                        {
                            ispass = true;
                            context.HttpContext.Items[HeaderKey] = admin;
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