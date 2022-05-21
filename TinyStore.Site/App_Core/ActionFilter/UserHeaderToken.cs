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
    }


    internal override void OnTokenGet(ActionExecutingContext context, TokenData tokendata)
    {
        Model.UserModel model = null;
        if (tokendata != null)
        {
            model = TokenModel(tokendata);
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

    public static Func<TokenData, Model.UserModel> TokenModel =
        (tokendata) => {
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