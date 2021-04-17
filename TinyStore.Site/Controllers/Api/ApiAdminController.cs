using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TinyStore.Model;

namespace TinyStore.Site.Controllers.Api
{
    [ApiController]
    [MultipleSubmit]
    [AdminHeaderToken("Login")]
    [Produces("application/json")]
    [Route("ApiAdmin/[action]")]
    public class ApiAdminController : ControllerBase
    {
        private Model.AdminModel AdminCurrent()
        {
            return HttpContext.Items[HeaderToken.HeaderKey] as Model.AdminModel;
        }

        private void AdminLog(int adminId, EAdminLogType type, HttpRequest request, string memo = "")
        {
            BLL.AdminLogBLL.Insert(new Model.AdminLogModel
            {
                AdminLogId = Global.Generator.DateId(2),
                AdminLogType = type,
                AdminId = adminId,
                CreateDate = DateTime.Now,
                ClientIP = Utils.RequestInfo._ClientIP(request).ToString(),
                UserAgent = request.Headers["User-Agent"].ToString(),
                AcceptLanguage = request.Headers["Accept-Language"].ToString(),
                Memo = memo,
            });
        }

        public IActionResult Login([FromForm] string Account, [FromForm] string Password)
        {
            if (string.IsNullOrWhiteSpace(Account) || string.IsNullOrWhiteSpace(Password))
                return ApiResult.RCode("传参错误");


            //var s = Global.HashArray("Tiny_12345", "219504");
            var admin = BLL.AdminBLL.QueryModelByAccount(Account);
            if (admin != null && string.Equals(admin.Password, Global.Hash(Password, admin.Salt)))
            {
                admin.ClientKey = Global.Generator.Guid();
                BLL.AdminBLL.Update(admin);
                //Response.Headers.Add("Access-Control-Allow-Headers", "Token");
                //Response.Headers.Add("Access-Control-Expose-Headers", "Token");
                //Response.Headers.Add("Token", HeaderToken.ToToken(new { AdminId = admin.AdminId, ClientKey = admin.ClientKey }));
                // HttpContext.Items[AdminHeaderToken.ItemKey] =
                //     new {AdminId = admin.AdminId, ClientKey = admin.ClientKey};
                //todo untest
                HeaderToken.SetHeaderToken(HttpContext, admin.AdminId.ToString(), admin.ClientKey);
                
                admin.ClientKey = "";
                admin.Salt = "";
                admin.Password = "";

                AdminLog(admin.AdminId, EAdminLogType.登录, Request, "管理员登录");

                return ApiResult.RData(admin);
            }
            else
            {
                return ApiResult.RCode("账号或密码不正确");
            }
        }

        public IActionResult AdminLogPageList([FromForm] int PageIndex, [FromForm] int PageSize, [FromForm] int AdminId,
            [FromForm] int LogType, [FromForm] DateTime Begin, [FromForm] DateTime End)
        {
            var current = AdminCurrent();
            Begin = Begin.Date;
            End = End.Date.AddDays(1).AddSeconds(-1);
            if (!current.IsRoot)
                AdminId = current.AdminId;
            var res = BLL.AdminLogBLL.QueryPageListByAdminIdAndType(AdminId, LogType, Begin, End, PageIndex,
                PageSize);
            return ApiResult.RData(new GridData<Model.AdminLogModel>(res.Rows, (int) res.Total));
        }

        public IActionResult AdminSave([FromForm] int AdminId, [FromForm] string Account, [FromForm] string Password,
            [FromForm] bool IsRoot)
        {
            var current = AdminCurrent();
            if (string.IsNullOrEmpty(Account))
                return ApiResult.RCode("账号名称不能为空");
            if (AdminId == 0)
            {
                if (BLL.AdminBLL.QueryModelByAccount(Account) != null)
                    return ApiResult.RCode("账号名称重复");
                var salt = Global.Generator.Guid().Substring(0, 6);
                var admin = new Model.AdminModel
                {
                    Account = Account,
                    ClientKey = string.Empty,
                    CreateDate = DateTime.Now,
                    IsRoot = IsRoot,
                    Password = Global.Hash(Password, salt),
                    Salt = salt
                };
                BLL.AdminBLL.Insert(admin);
                AdminLog(current.AdminId, EAdminLogType.管理员管理, Request, "管理员添加" + admin.AdminId);
                return ApiResult.RCode("");
            }
            else
            {
                var admin = BLL.AdminBLL.QueryModelById(AdminId);
                if (admin == null)
                    return ApiResult.RCode("管理员不存在或已被删除");
                var datacompare = BLL.AdminBLL.QueryModelByAccount(Account);
                if (datacompare != null && admin.AdminId != datacompare.AdminId)
                    return ApiResult.RCode("账号名称重复");
                admin.IsRoot = IsRoot;
                admin.Account = Account;
                if (!string.IsNullOrEmpty(Password))
                    admin.Password = Global.Hash(Password, admin.Salt);
                BLL.AdminBLL.Update(admin);
                AdminLog(current.AdminId, EAdminLogType.管理员管理, Request, "管理员修改" + admin.AdminId);
                return ApiResult.RCode("");
            }
        }

        public IActionResult AdminDelete([FromForm] int AdminId)
        {
            var current = AdminCurrent();

            if (AdminId == 0)
                return ApiResult.RCode("管理员不存在或已被删除");
            if (BLL.AdminBLL.QueryModelById(AdminId) == null)
                return ApiResult.RCode("管理员不存在或已被删除");
            BLL.AdminBLL.DeleteById(AdminId);

            AdminLog(current.AdminId, EAdminLogType.管理员管理, Request, "管理员删除" + AdminId);

            return ApiResult.RCode("");
        }

        public IActionResult AdminPageList([FromForm] int PageIndex, [FromForm] int PageSize)
        {
            var res = BLL.AdminBLL.QueryPageList(PageIndex, PageSize);
            return ApiResult.RData(new GridData<Model.AdminModel>(res.Rows, (int) res.Total));
        }

        public IActionResult OrderPageList([FromForm] int PageIndex, [FromForm] int PageSize, [FromForm] int State,
            [FromForm] int Keykind, [FromForm] string Key, [FromForm] bool IsHasreturn, [FromForm] int TimeType,
            [FromForm] DateTime Begin, [FromForm] DateTime End)
        {
            var current = AdminCurrent();

            if (State == (int) EState.客户下单 && TimeType == (int) EOrderTimeType.付款日期)
            {
                return ApiResult.RData(
                    new GridData<Model.OrderModel>(new List<Model.OrderModel>(), 0));
            }

            Begin = Begin.Date;
            End = End.Date.AddDays(1).AddSeconds(-1);
            var res = BLL.OrderBLL.QueryPageList(Begin, End, State, Keykind, Key, string.Empty, IsHasreturn,
                (EOrderTimeType) TimeType, PageIndex, PageSize);
            if (res != null && res.Rows.Count > 0)
            {
                var storeids = res.Rows.Select(p => p.StoreId).Distinct().ToList();
                var stores = BLL.StoreBLL.QueryListByStoreIds(storeids);
                foreach (var item in res.Rows)
                {
                    var model = stores.FirstOrDefault(p => p.StoreId == item.StoreId);
                    if (model != null)
                    {
                        item.StoreName = model.Name;
                        item.StoreUniqueId = model.UniqueId;
                    }
                }
            }

            return ApiResult.RData(new GridData<Model.OrderModel>(res.Rows, res.Total));
        }

        public IActionResult OrderUpdateCost([FromForm] string OrderId, [FromForm] double Cost)
        {
            var current = AdminCurrent();
            if (string.IsNullOrEmpty(OrderId))
                return ApiResult.RCode("订单不存在");

            var order = BLL.OrderBLL.QueryModelByOrderId(OrderId);
            if (order == null)
                return ApiResult.RCode("订单不存在");
            BLL.OrderBLL.UpdateCost(order.OrderId, Cost);

            AdminLog(current.AdminId, EAdminLogType.订单管理, Request, "修改成本" + OrderId);

            return ApiResult.RCode("");
        }

        public IActionResult OrderPay([FromForm] string OrderId, [FromForm] double Income, [FromForm] string Txnid)
        {
            var current = AdminCurrent();
            if (string.IsNullOrEmpty(OrderId))
                return ApiResult.RCode("订单不存在");

            var order = BLL.OrderBLL.QueryModelByOrderId(OrderId);
            if (order == null)
                return ApiResult.RCode("订单不存在");
            if (order.IsPay)
                return ApiResult.RCode("订单已付款，不能操作");

            SiteContext.OrderHelper.Pay(order.OrderId, Income, Txnid);

            AdminLog(current.AdminId, EAdminLogType.订单管理, Request, "订单手动变已付款状态" + OrderId);

            return ApiResult.RCode("");
        }

        public IActionResult OrderReturn([FromForm] string OrderId)
        {
            var current = AdminCurrent();

            if (string.IsNullOrEmpty(OrderId))
                return ApiResult.RCode("订单不存在");

            var order = BLL.OrderBLL.QueryModelByOrderId(OrderId);
            if (order == null)
                return ApiResult.RCode("订单不存在");
            if (!order.IsPay)
                return ApiResult.RCode("订单尚未付款，不能操作");

            order.ReturnDate = DateTime.Now;
            order.LastUpdateDate = DateTime.Now;
            if (!order.IsDelivery && string.Equals(order.Amount.ToString("f2"),
                order.ReturnAmount.ToString("f2"),
                StringComparison.OrdinalIgnoreCase))
            {
                order.IsDelivery = true;
                order.DeliveryDate = DateTime.Now;
            }

            //todo 支付平台收款 调用平台方法退款
            if (Global.EnumsDic<EPaymentType>().Values.Contains(order.PaymentType))
            {
                var userExtend = BLL.UserExtendBLL.QueryModelById(order.UserId);
                if (userExtend == null)
                    return ApiResult.RCode("商户不存在，请检查数据库");
                if (userExtend.Amount < order.ReturnAmount)
                    return ApiResult.RCode("商户资金小于退款金额，请联系店铺管理人员解决");
                BLL.UserExtendBLL.ChangeAmount(userExtend.UserId, -order.ReturnAmount);
            }

            BLL.OrderBLL.Update(order);
            AdminLog(current.AdminId, EAdminLogType.订单管理, Request, "订单退款" + OrderId);

            return ApiResult.RCode("");
        }

        public IActionResult OrderDelete([FromForm] string OrderId)
        {
            var current = AdminCurrent();
            if (string.IsNullOrEmpty(OrderId))
                return ApiResult.RCode("订单不存在");

            var order = BLL.OrderBLL.QueryModelByOrderId(OrderId);
            if (order == null)
                return ApiResult.RCode("订单不存在");

            var ordertrash = Global.Json.Deserialize<Model.OrderTrashModel>(Global.Json.Serialize(order));
            ordertrash.DeleteDate = DateTime.Now;
            BLL.OrderTrashBLL.Insert(ordertrash);

            BLL.OrderBLL.DeleteByOrderId(order.OrderId);
            AdminLog(current.AdminId, EAdminLogType.订单管理, Request, "删除订单" + OrderId);
            return ApiResult.RCode("");
        }

        //订单结算   OrderCloseNo => OrderSettle
        public IActionResult OrderSettle([FromForm] string OrderIds, [FromForm] bool IsSettle)
        {
            var current = AdminCurrent();
            var orderids = Global.Json.Deserialize<List<string>>(OrderIds);

            BLL.OrderBLL.UpdateIsSettle(orderids, string.Empty, IsSettle, DateTime.Now);

            AdminLog(current.AdminId, EAdminLogType.订单管理, Request, "批量设置未结算");
            return ApiResult.RCode("");
        }

        public IActionResult OrderDeletePageList([FromForm] int PageIndex, [FromForm] int PageSize,
            [FromForm] int State, [FromForm] int Keykind, [FromForm] string Key, [FromForm] DateTime Begin,
            [FromForm] DateTime End)
        {
            var current = AdminCurrent();
            if (current.IsRoot)
            {
                Begin = Begin.Date;
                End = End.Date.AddDays(1).AddSeconds(-1);
                var res = BLL.OrderTrashBLL.QueryPageList(string.Empty, Begin, End, State, Keykind, Key,
                    string.Empty, PageIndex, PageSize);
                if (res != null && res.Rows.Count > 0)
                {
                    var storeids = res.Rows.Select(p => p.StoreId).Distinct().ToList();
                    var stores = BLL.StoreBLL.QueryListByStoreIds(storeids);
                    foreach (var item in res.Rows)
                    {
                        var model = stores.FirstOrDefault(p => p.StoreId == item.StoreId);
                        if (model != null)
                        {
                            item.StoreName = model.Name;
                        }
                    }
                }

                return ApiResult.RData(
                    new GridData<Model.OrderTrashModel>(res.Rows, (int) res.Total));
            }
            else
            {
                return ApiResult.RData(
                    new GridData<Model.OrderTrashModel>(new List<OrderTrashModel>(), 0));
            }
        }

        public IActionResult ProductSupplyPageList([FromForm] int PageIndex, [FromForm] int PageSize)
        {
            var res = BLL.SupplyBLL.QueryPageListByUserId(SiteContext.Config.SupplyUserIdSys, PageIndex, PageSize);
            return ApiResult.RData(new GridData<Model.SupplyModel>(res.Rows, res.Total));
        }

        public IActionResult ProductSupplyUpdatePriceStock([FromForm] string List)
        {
            var list = Global.Json.Deserialize<List<Model.SupplyModel>>(List);
            BLL.SupplyBLL.UpdatePriceStock(list);
            return ApiResult.RCode("");
        }

        public IActionResult UserPageList([FromForm] int PageIndex, [FromForm] int PageSize, [FromForm] string Key)
        {
            var res = BLL.UserBLL.QueryPageListByKey(Key, PageIndex, PageSize);

            return ApiResult.RData(new GridData<Model.UserModel>(res.Rows, res.Total));
        }

        public IActionResult UserStorePageList([FromForm] int PageIndex, [FromForm] int PageSize, [FromForm] string Key)
        {
            var res = BLL.StoreBLL.QueryPageListByKey(Key, PageIndex, PageSize);
            return ApiResult.RData(new GridData<Model.StoreModel>(res.Rows, res.Total));
        }

        public IActionResult UserExtend([FromForm] int id)
        {
            return ApiResult.RData(BLL.UserExtendBLL.QueryModelByUserId(id));
        }

        public IActionResult UserPwdModify([FromForm] int Id, [FromForm] string Pwd)
        {
            var current = AdminCurrent();
            if (string.IsNullOrEmpty(Pwd))
                return ApiResult.RCode("密码不能为空");

            var user = Id == 0 ? null : BLL.UserBLL.QueryModelById(Id);
            if (user == null)
                return ApiResult.RCode("商户不存在");
            
            user.Password = Global.Hash(Pwd, user.Salt);
            BLL.UserBLL.Update(user);
            AdminLog(current.AdminId, EAdminLogType.商户管理, Request, "密码修改" + Id);
            return ApiResult.RCode("");
        }

        public IActionResult UserLevelModify([FromForm] int Id, [FromForm] int Level)
        {
            var current = AdminCurrent();

            var userExtend = BLL.UserExtendBLL.QueryModelById(Id);
            if (userExtend == null)
                return ApiResult.RCode("商户不存在");

            BLL.UserExtendBLL.ModifyLevel(userExtend.UserId, (EUserLevel) Level);
            AdminLog(current.AdminId, EAdminLogType.店铺管理, Request, "商户级别修改" + Id);
            return ApiResult.RCode("");
        }

        public IActionResult UserLogin([FromForm] int Id)
        {
            var user = Id == 0 ? null : BLL.UserBLL.QueryModelById(Id);
            if (user == null)
                return ApiResult.RCode("商户不存在");
            //todo UserContext.Login(user, false);
            return ApiResult.RCode("");
        }

        public IActionResult UserLogPageList([FromForm] int PageIndex, [FromForm] int PageSize, [FromForm] int UserId,
            string StoreId, [FromForm] int Logtype, [FromForm] DateTime Begin, [FromForm] DateTime End)
        {
            Begin = Begin.Date;
            End = End.Date.AddDays(1).AddSeconds(-1);
            var res = BLL.UserLogBLL.QueryPageListByUserIdOrStoreIdOrType(UserId, StoreId, Logtype, Begin, End,
                PageIndex, PageSize);

            return ApiResult.RData(new GridData<Model.UserLogModel>(res.Rows, res.Total));
        }

        public IActionResult WithDrawPageList([FromForm] int PageIndex, [FromForm] int PageSize, [FromForm] int state,
            [FromForm] DateTime Begin, [FromForm] DateTime End)
        {
            var current = AdminCurrent();

            Begin = Begin.Date;
            End = End.Date.AddDays(1).AddSeconds(-1);
            var res = BLL.WithDrawBLL.QueryPageList(0, state, Begin, End, PageIndex, PageSize);
            return ApiResult.RData(new GridData<Model.WithDrawModel>(res.Rows, (int) res.Total));
        }

        public IActionResult WithDrawCheck([FromForm] string Id, [FromForm] double Income, [FromForm] string TranId,
            [FromForm] string Memo)
        {
            var current = AdminCurrent();
            if (string.IsNullOrEmpty(Id))
                return ApiResult.RCode("ID不能为空");
            var withdraw = BLL.WithDrawBLL.QueryModelByWithDrawId(Id);
            if (withdraw == null)
                return ApiResult.RCode("数据不存在或已被删除");
            var userExtend = BLL.UserExtendBLL.QueryModelById(withdraw.UserId);
            if (userExtend == null)
                return ApiResult.RCode("商户不存在");
            if (withdraw.IsFinish)
                return ApiResult.RCode("提现申请已处理，不能删除");
            if (withdraw.Amount > userExtend.Amount)
                return ApiResult.RCode("商户余额不足以提现");
            if (Income > withdraw.Amount)
                return ApiResult.RCode("到账金额不能大于申请金额");
            withdraw.Income = Income;
            withdraw.FinishDate = DateTime.Now;
            withdraw.TranId = TranId;
            withdraw.Memo = Memo;
            withdraw.IsFinish = true;
            BLL.WithDrawBLL.Update(withdraw);
            BLL.UserExtendBLL.ChangeAmount(userExtend.UserId, -withdraw.Amount);
            AdminLog(current.AdminId, EAdminLogType.提现管理, Request, "提现申请成功" + Id);
            return ApiResult.RCode("");
        }

        public IActionResult WithDrawCheckNo([FromForm] string Id, [FromForm] string Memo)
        {
            var current = AdminCurrent();
            if (string.IsNullOrEmpty(Id))
                return ApiResult.RCode("数据不存在或已被删除");
            var withdraw = BLL.WithDrawBLL.QueryModelByWithDrawId(Id);
            if (withdraw == null)
                return ApiResult.RCode("数据不存在或已被删除");
            if (withdraw.IsFinish)
                return ApiResult.RCode("提现申请已处理，不能拒绝");
            withdraw.FinishDate = DateTime.Now;
            withdraw.Memo = Memo;
            withdraw.IsFinish = true;
            BLL.WithDrawBLL.Update(withdraw);
            AdminLog(current.AdminId, EAdminLogType.提现管理, Request, "提现申请拒绝" + Id);
            return ApiResult.RCode("");
        }

        public IActionResult WithDrawDelete([FromForm] string Id)
        {
            var current = AdminCurrent();
            if (string.IsNullOrEmpty(Id))
                return ApiResult.RCode("数据不存在或已被删除");
            var withdraw = BLL.WithDrawBLL.QueryModelByWithDrawId(Id);
            if (withdraw == null)
                return ApiResult.RCode("数据不存在或已被删除");
            if (withdraw.IsFinish)
                return ApiResult.RCode("提现申请已处理，不能删除");

            BLL.WithDrawBLL.DeleteByWithDrawId(Id);
            AdminLog(current.AdminId, EAdminLogType.提现管理, Request, "提现申请删除" + Id);
            return ApiResult.RCode("");
        }
    }
}