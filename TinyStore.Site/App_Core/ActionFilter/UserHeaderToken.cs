using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace TinyStore.Site
{
    public sealed class UserHeaderToken : HeaderToken
    {
        /// <summary>
        /// 可以传参传入要忽略的Action名称,传入的Action不会执行判断
        /// </summary>
        /// <param name="ignoreactions"></param>
        public UserHeaderToken(params string[] ignoreactions) : base(ignoreactions)
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
                    var user = BLL.UserBLL.QueryModelById(int.Parse(data.Id));
                    if (user != null)
                    {
                        var isvalidate =
#if DEBUG
                            true;
#else
                        user.ClientKey == data.Key;
#endif
                        if (isvalidate)
                        {
                            ispass = true;
                            context.HttpContext.Items[HeaderKey] = user;
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