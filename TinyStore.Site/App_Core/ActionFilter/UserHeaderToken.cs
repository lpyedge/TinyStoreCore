using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace TinyStore.Site;

public sealed class UserHeaderToken : HeaderToken
{
    public const string TokenKey = "UserToken";

    public const string ItemKey = "User";

    /// <summary>
    ///     可以传参传入要忽略的Action名称,传入的Action不会执行判断
    /// </summary>
    /// <param name="ignoreactions"></param>
    public UserHeaderToken(params string[] ignoreactions) : base(TokenKey, ItemKey, ignoreactions)
    {
        TokenToModel = (tokendata) => {
            try
            {
                var model = BLL.UserBLL.QueryModelById(tokendata.Id);
                if (model != null)
                {
                    if (model.ClientKey == tokendata.Key
#if DEBUG
                        || true
#endif
                       )
                    {
                        return model;
                    }
                }
            }
            catch
            {
            }

            return null;
        };
    }



}