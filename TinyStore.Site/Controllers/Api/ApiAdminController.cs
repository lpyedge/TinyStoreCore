﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
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

        [HttpPost]
        public IActionResult Login([FromForm] string account, [FromForm] string password)
        {
            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password))
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);

            var admin = BLL.AdminBLL.QueryModelByAccount(account);
            if (admin != null && string.Equals(admin.Password, Global.Hash(password, admin.Salt),StringComparison.OrdinalIgnoreCase))
            {
                admin.ClientKey = Global.Generator.Guid();
                BLL.AdminBLL.Update(admin);
                
                HeaderToken.SetHeaderToken(HttpContext, admin.AdminId.ToString(), admin.ClientKey);
                
                admin.ClientKey = "";
                admin.Salt = "";
                admin.Password = "";

                AdminLog(admin.AdminId, EAdminLogType.登录, Request, "管理员登录");

                return ApiResult.RData(admin);
            }
            else
            {
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);
            }
        }

        
        [HttpPost]
        public IActionResult ConfigLoad()
        {
            AdminModel admin = AdminCurrent();

            return ApiResult.RData(SiteContext.Config);
        }
        [HttpPost]
        public IActionResult ConfigSave(SiteContext.ConfigModel config)
        {
            AdminModel admin = AdminCurrent();

            if (config != null)
            {
                SiteContext.Config = config;
                SiteContext.ConfigSave();
            }

            return ApiResult.RData(SiteContext.Config);
        }

        [HttpPost]
        public IActionResult AdminPageList([FromForm] int pageIndex, [FromForm] int pageSize)
        {
            AdminModel admin = AdminCurrent();
            
            var res = BLL.AdminBLL.QueryPageList(pageIndex, pageSize);
            
            return ApiResult.RData(res);
        }
        
        [HttpPost]
        public IActionResult AdminSave(AdminModel adminModel)
        {
            AdminModel admin = AdminCurrent();

            if (adminModel == null)
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);
            
            if (adminModel.AdminId == 0)
            {
                if (BLL.AdminBLL.QueryModelByAccount(adminModel.Account) != null)
                    return ApiResult.RCode(ApiResult.ECode.TargetExist);
                var salt = Global.Generator.Guid().Substring(0, 6);
                adminModel.ClientKey = "";
                adminModel.Salt = salt;
                adminModel.Password = Global.Hash(adminModel.Password, salt);
                BLL.AdminBLL.Insert(adminModel);
                AdminLog(admin.AdminId, EAdminLogType.管理员管理, Request, "管理员添加" + adminModel.AdminId);
                return ApiResult.RCode("");
            }
            else
            {
                var adminOrgin = BLL.AdminBLL.QueryModelById(adminModel.AdminId);
                if (adminOrgin == null)
                    return ApiResult.RCode(ApiResult.ECode.TargetNotExist);
                var datacompare = BLL.AdminBLL.QueryModelByAccount(adminModel.Account);
                if (datacompare != null && admin.AdminId != datacompare.AdminId)
                    return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);
                
                if (!string.IsNullOrEmpty(adminModel.Password))
                    adminModel.Password = Global.Hash(adminModel.Password, admin.Salt);
                BLL.AdminBLL.Update(adminModel);
                AdminLog(admin.AdminId, EAdminLogType.管理员管理, Request, "管理员修改" + adminModel.AdminId);
                return ApiResult.RCode("");
            }
        }
        
        [HttpPost]
        public IActionResult AdminDelete([FromForm] List<int> adminIds)
        {
            var current = AdminCurrent();

            if (adminIds.Count == 0)
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);
            
            BLL.AdminBLL.DeleteByIds(adminIds);

            AdminLog(current.AdminId, EAdminLogType.管理员管理, Request, "管理员删除" + adminIds);

            return ApiResult.RCode("");
        }
        
        public IActionResult AdminLogPageList([FromForm] int pageIndex, [FromForm] int pageSize, [FromForm] int adminId,
            [FromForm] int logType, [FromForm] DateTime begin, [FromForm] DateTime end)
        {
            var current = AdminCurrent();
            begin = begin.Date;
            end = end.Date.AddDays(1).AddSeconds(-1);
            if (!current.IsRoot)
                adminId = current.AdminId;
            var res = BLL.AdminLogBLL.QueryPageListByAdminIdAndType(adminId, logType, begin, end, pageIndex,
                pageSize);
            return ApiResult.RData(new GridData<Model.AdminLogModel>(res.Rows, (int) res.Total));
        }


        public IActionResult OrderPageList([FromForm] int pageIndex, [FromForm] int pageSize, [FromForm] int state,
            [FromForm] int keykind, [FromForm] string key, [FromForm] bool isHasreturn, [FromForm] int timeType,
            [FromForm] DateTime begin, [FromForm] DateTime end)
        {
            var current = AdminCurrent();

            if (state == (int) EState.客户下单 && timeType == (int) EOrderTimeType.付款日期)
            {
                return ApiResult.RData(
                    new GridData<Model.OrderModel>(new List<Model.OrderModel>(), 0));
            }

            begin = begin.Date;
            end = end.Date.AddDays(1).AddSeconds(-1);
            var res = BLL.OrderBLL.QueryPageList(begin, end, state, keykind, key, string.Empty, isHasreturn,
                (EOrderTimeType) timeType, pageIndex, pageSize);
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

        public IActionResult OrderUpdateCost([FromForm] string orderId, [FromForm] double cost)
        {
            var current = AdminCurrent();
            if (string.IsNullOrEmpty(orderId))
                return ApiResult.RCode("订单不存在");

            var order = BLL.OrderBLL.QueryModelByOrderId(orderId);
            if (order == null)
                return ApiResult.RCode("订单不存在");
            BLL.OrderBLL.UpdateCost(order.OrderId, cost);

            AdminLog(current.AdminId, EAdminLogType.订单管理, Request, "修改成本" + orderId);

            return ApiResult.RCode("");
        }

        public IActionResult OrderPay([FromForm] string orderId, [FromForm] double income, [FromForm] string txnid)
        {
            var current = AdminCurrent();
            if (string.IsNullOrEmpty(orderId))
                return ApiResult.RCode("订单不存在");

            var order = BLL.OrderBLL.QueryModelByOrderId(orderId);
            if (order == null)
                return ApiResult.RCode("订单不存在");
            if (order.IsPay)
                return ApiResult.RCode("订单已付款，不能操作");

            SiteContext.OrderHelper.Pay(order.OrderId, income, txnid);

            AdminLog(current.AdminId, EAdminLogType.订单管理, Request, "订单手动变已付款状态" + orderId);

            return ApiResult.RCode("");
        }

        public IActionResult OrderReturn([FromForm] string orderId)
        {
            var current = AdminCurrent();

            if (string.IsNullOrEmpty(orderId))
                return ApiResult.RCode("订单不存在");

            var order = BLL.OrderBLL.QueryModelByOrderId(orderId);
            if (order == null)
                return ApiResult.RCode("订单不存在");
            if (!order.IsPay)
                return ApiResult.RCode("订单尚未付款，不能操作");

            order.RefundDate = DateTime.Now;
            order.LastUpdateDate = DateTime.Now;
            if (!order.IsDelivery && string.Equals(order.Amount.ToString("f2"),
                order.RefundAmount.ToString("f2"),
                StringComparison.OrdinalIgnoreCase))
            {
                order.IsDelivery = true;
                order.DeliveryDate = DateTime.Now;
            }

            //todo 支付平台收款 调用平台方法退款
            // if (Global.EnumsDic<EPaymentType>().Values.Contains(order.PaymentType))
            // {
            //     var userExtend = BLL.UserExtendBLL.QueryModelById(order.UserId);
            //     if (userExtend == null)
            //         return ApiResult.RCode("商户不存在，请检查数据库");
            //     if (userExtend.Amount < order.RefundAmount)
            //         return ApiResult.RCode("商户资金小于退款金额，请联系店铺管理人员解决");
            //     BLL.UserExtendBLL.ChangeAmount(userExtend.UserId, -order.RefundAmount);
            // }

            BLL.OrderBLL.Update(order);
            AdminLog(current.AdminId, EAdminLogType.订单管理, Request, "订单退款" + orderId);

            return ApiResult.RCode("");
        }

        public IActionResult OrderDelete([FromForm] string orderId)
        {
            var current = AdminCurrent();
            if (string.IsNullOrEmpty(orderId))
                return ApiResult.RCode("订单不存在");

            var order = BLL.OrderBLL.QueryModelByOrderId(orderId);
            if (order == null)
                return ApiResult.RCode("订单不存在");

            var ordertrash = Global.Json.Deserialize<Model.OrderTrashModel>(Global.Json.Serialize(order));
            ordertrash.DeleteDate = DateTime.Now;
            BLL.OrderTrashBLL.Insert(ordertrash);

            BLL.OrderBLL.DeleteByOrderId(order.OrderId);
            AdminLog(current.AdminId, EAdminLogType.订单管理, Request, "删除订单" + orderId);
            return ApiResult.RCode("");
        }

        //订单结算   OrderCloseNo => OrderSettle
        public IActionResult OrderSettle([FromForm] string orderIds, [FromForm] bool isSettle)
        {
            var current = AdminCurrent();
            var orderids = Global.Json.Deserialize<List<string>>(orderIds);

            BLL.OrderBLL.UpdateIsSettle(orderids, string.Empty, isSettle, DateTime.Now);

            AdminLog(current.AdminId, EAdminLogType.订单管理, Request, "批量设置未结算");
            return ApiResult.RCode("");
        }

        public IActionResult OrderDeletePageList([FromForm] int pageIndex, [FromForm] int pageSize,
            [FromForm] int state, [FromForm] int keykind, [FromForm] string key, [FromForm] DateTime begin,
            [FromForm] DateTime end)
        {
            var current = AdminCurrent();
            if (current.IsRoot)
            {
                begin = begin.Date;
                end = end.Date.AddDays(1).AddSeconds(-1);
                var res = BLL.OrderTrashBLL.QueryPageList(string.Empty, begin, end, state, keykind, key,
                    string.Empty, pageIndex, pageSize);
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

        public IActionResult ProductSupplyPageList([FromForm] int pageIndex, [FromForm] int pageSize)
        {
            var res = BLL.SupplyBLL.QueryPageListByUserId(SiteContext.Config.SupplyUserIdSys, pageIndex, pageSize);
            return ApiResult.RData(new GridData<Model.SupplyModel>(res.Rows, res.Total));
        }

        public IActionResult ProductSupplyUpdatePriceStock([FromForm] string list)
        {
            var sulllylist = Global.Json.Deserialize<List<Model.SupplyModel>>(list);
            BLL.SupplyBLL.UpdatePriceStock(sulllylist);
            return ApiResult.RCode("");
        }

        public IActionResult UserPageList([FromForm] int pageIndex, [FromForm] int pageSize, [FromForm] string key)
        {
            var res = BLL.UserBLL.QueryPageListByKey(key, pageIndex, pageSize);

            return ApiResult.RData(new GridData<Model.UserModel>(res.Rows, res.Total));
        }

        public IActionResult UserStorePageList([FromForm] int pageIndex, [FromForm] int pageSize, [FromForm] string key)
        {
            var res = BLL.StoreBLL.QueryPageListByKey(key, pageIndex, pageSize);
            return ApiResult.RData(new GridData<Model.StoreModel>(res.Rows, res.Total));
        }

        public IActionResult UserExtend([FromForm] int id)
        {
            return ApiResult.RData(BLL.UserExtendBLL.QueryModelByUserId(id));
        }

        public IActionResult UserPwdModify([FromForm] int id, [FromForm] string pwd)
        {
            var current = AdminCurrent();
            if (string.IsNullOrEmpty(pwd))
                return ApiResult.RCode("密码不能为空");

            var user = id == 0 ? null : BLL.UserBLL.QueryModelById(id);
            if (user == null)
                return ApiResult.RCode("商户不存在");
            
            user.Password = Global.Hash(pwd, user.Salt);
            BLL.UserBLL.Update(user);
            AdminLog(current.AdminId, EAdminLogType.商户管理, Request, "密码修改" + id);
            return ApiResult.RCode("");
        }

        public IActionResult UserLevelModify([FromForm] int id, [FromForm] int level)
        {
            var current = AdminCurrent();

            var userExtend = BLL.UserExtendBLL.QueryModelById(id);
            if (userExtend == null)
                return ApiResult.RCode("商户不存在");

            BLL.UserExtendBLL.ModifyLevel(userExtend.UserId, (EUserLevel) level);
            AdminLog(current.AdminId, EAdminLogType.店铺管理, Request, "商户级别修改" + id);
            return ApiResult.RCode("");
        }

        public IActionResult UserLogin([FromForm] int id)
        {
            var user = id == 0 ? null : BLL.UserBLL.QueryModelById(id);
            if (user == null)
                return ApiResult.RCode("商户不存在");
            //todo UserContext.Login(user, false);
            return ApiResult.RCode("");
        }

        public IActionResult UserLogPageList([FromForm] int pageIndex, [FromForm] int pageSize, [FromForm] int userId,
            string storeId, [FromForm] int logtype, [FromForm] DateTime begin, [FromForm] DateTime end)
        {
            begin = begin.Date;
            end = end.Date.AddDays(1).AddSeconds(-1);
            var res = BLL.UserLogBLL.QueryPageListByUserIdOrStoreIdOrType(userId, storeId, logtype, begin, end,
                pageIndex, pageSize);

            return ApiResult.RData(new GridData<Model.UserLogModel>(res.Rows, res.Total));
        }

        public IActionResult WithDrawPageList([FromForm] int pageIndex, [FromForm] int pageSize, [FromForm] int state,
            [FromForm] DateTime begin, [FromForm] DateTime end)
        {
            var current = AdminCurrent();

            begin = begin.Date;
            end = end.Date.AddDays(1).AddSeconds(-1);
            var res = BLL.WithDrawBLL.QueryPageList(0, state, begin, end, pageIndex, pageSize);
            return ApiResult.RData(new GridData<Model.WithDrawModel>(res.Rows, (int) res.Total));
        }

        public IActionResult WithDrawCheck([FromForm] string id, [FromForm] double income, [FromForm] string tranId,
            [FromForm] string memo)
        {
            var current = AdminCurrent();
            if (string.IsNullOrEmpty(id))
                return ApiResult.RCode("ID不能为空");
            var withdraw = BLL.WithDrawBLL.QueryModelByWithDrawId(id);
            if (withdraw == null)
                return ApiResult.RCode("数据不存在或已被删除");
            var userExtend = BLL.UserExtendBLL.QueryModelById(withdraw.UserId);
            if (userExtend == null)
                return ApiResult.RCode("商户不存在");
            if (withdraw.IsFinish)
                return ApiResult.RCode("提现申请已处理，不能删除");
            if (withdraw.Amount > userExtend.Amount)
                return ApiResult.RCode("商户余额不足以提现");
            if (income > withdraw.Amount)
                return ApiResult.RCode("到账金额不能大于申请金额");
            withdraw.AmountFinish = income;
            withdraw.FinishDate = DateTime.Now;
            withdraw.TranId = tranId;
            withdraw.Memo = memo;
            withdraw.IsFinish = true;
            BLL.WithDrawBLL.Update(withdraw);
            BLL.UserExtendBLL.ChangeAmount(userExtend.UserId, -withdraw.Amount);
            AdminLog(current.AdminId, EAdminLogType.提现管理, Request, "提现申请成功" + id);
            return ApiResult.RCode("");
        }

        public IActionResult WithDrawCheckNo([FromForm] string id, [FromForm] string memo)
        {
            var current = AdminCurrent();
            if (string.IsNullOrEmpty(id))
                return ApiResult.RCode("数据不存在或已被删除");
            var withdraw = BLL.WithDrawBLL.QueryModelByWithDrawId(id);
            if (withdraw == null)
                return ApiResult.RCode("数据不存在或已被删除");
            if (withdraw.IsFinish)
                return ApiResult.RCode("提现申请已处理，不能拒绝");
            withdraw.FinishDate = DateTime.Now;
            withdraw.Memo = memo;
            withdraw.IsFinish = true;
            BLL.WithDrawBLL.Update(withdraw);
            AdminLog(current.AdminId, EAdminLogType.提现管理, Request, "提现申请拒绝" + id);
            return ApiResult.RCode("");
        }

        public IActionResult WithDrawDelete([FromForm] string id)
        {
            var current = AdminCurrent();
            if (string.IsNullOrEmpty(id))
                return ApiResult.RCode("数据不存在或已被删除");
            var withdraw = BLL.WithDrawBLL.QueryModelByWithDrawId(id);
            if (withdraw == null)
                return ApiResult.RCode("数据不存在或已被删除");
            if (withdraw.IsFinish)
                return ApiResult.RCode("提现申请已处理，不能删除");

            BLL.WithDrawBLL.DeleteByWithDrawId(id);
            AdminLog(current.AdminId, EAdminLogType.提现管理, Request, "提现申请删除" + id);
            return ApiResult.RCode("");
        }
    }
}