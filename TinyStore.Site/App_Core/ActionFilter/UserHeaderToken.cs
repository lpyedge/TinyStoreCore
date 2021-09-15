using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace TinyStore.Site
{
    public sealed class UserHeaderToken : HeaderToken
    {
        public const string HeaderKey = "UserToken";

        public const string ItemKey = "User";
        /// <summary>
        /// 可以传参传入要忽略的Action名称,传入的Action不会执行判断
        /// </summary>
        /// <param name="ignoreactions"></param>
        public UserHeaderToken(params string[] ignoreactions) : base(HeaderKey, ItemKey,ignoreactions)
        {
        }


        internal override void OnTokenGet(ActionExecutingContext context, TokenData tokendata)
        {
            var ispass = false;
            if (tokendata!=null)
            {
                try
                {
                    var user = BLL.UserBLL.QueryModelById(int.Parse(tokendata.Id));
                    if (user != null)
                    {
                        var isvalidate =
#if DEBUG
                            true;
#else
                        user.ClientKey == tokendata.Key;
#endif
                        if (isvalidate)
                        {
                            ispass = true;
                            context.HttpContext.Items[ItemKey] = user;
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