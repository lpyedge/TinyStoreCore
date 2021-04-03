using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TinyStore.Model.Extend;

namespace TinyStore.Site.Controllers.Api
{
    [ApiController]
    [MultipleSubmit]
    [UserHeaderToken("Login", "Register")]
    [Produces("application/json")]
    [Route("userapi/[action]")]
    public class UserController : ControllerBase
    {
        private Model.UserModel UserCurrent(string storeid, out Model.StoreModel store)
        {
            var user = UserCurrent();
            if (string.IsNullOrWhiteSpace(storeid))
            {
                store = null;
            }

            store = BLL.StoreBLL.QueryModelByStoreId(storeid);

            if (store != null && store.UserId != user.UserId)
            {
                store = null;
            }

            return user;
        }

        private Model.UserModel UserCurrent()
        {
            return HttpContext.Items[HeaderToken.HeaderKey] as Model.UserModel;
        }

        private void UserLog(int userId, EUserLogType type, HttpRequest request, string storeId = "",
            string memo = "")
        {
            BLL.UserLogBLL.Insert(new Model.UserLogModel
            {
                UserLogId = Global.Generator.DateId(2),
                UserLogType = type,
                UserId = userId,
                ClientIP = SiteContext.RequestInfo._ClientIP(request).ToString(),
                UserAgent = request.Headers["User-Agent"].ToString(),
                AcceptLanguage = request.Headers["Accept-Language"].ToString(),
                Memo = memo,
                StoreId = storeId,
                CreateDate = DateTime.Now,
            });
        }


        [HttpPost]
        public IActionResult Login([FromForm] string account, [FromForm] string password)
        {
            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password))
                return ApiResult.RCode("传参错误");
            var user = BLL.UserBLL.QueryModelByAccount(account);
            if (user != null && string.Equals(user.Password, Global.Hash(password, user.Salt)))
            {
                user.ClientKey = Global.Generator.Guid();
                BLL.UserBLL.Update(user);

                HeaderToken.SetHeaderToken(HttpContext, user.UserId.ToString(), user.ClientKey);

                user.ClientKey = "";
                user.Salt = "";
                user.Password = "";

                var storelist = BLL.StoreBLL.QueryListByUserId(user.UserId);
                var userextra = BLL.UserExtendBLL.QueryModelByUserId(user.UserId);

                UserLog(user.UserId, EUserLogType.登录, Request);

                return ApiResult.RData(new {user = user, storeList = storelist, userExtend = userextra});
            }
            else
            {
                return ApiResult.RCode("用户账号或密码不正确");
            }
        }

        public IActionResult Register([FromForm] string Account, [FromForm] string Password, [FromForm] string QQ,
            [FromForm] string Email, [FromForm] string Telphone)
        {
            if (string.IsNullOrWhiteSpace(Account) || string.IsNullOrWhiteSpace(Password) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Telphone))
            {
                return ApiResult.RCode("传参错误");
            }

            if (BLL.UserBLL.QueryModelByAccount(Account) != null)
                return ApiResult.RCode("用户账号已存在");
            if (BLL.UserExtendBLL.QueryModelByEmail(Email) != null)
                return ApiResult.RCode("保密邮箱已存在");
            if (BLL.UserExtendBLL.QueryModelByTelPhone(Telphone) != null)
                return ApiResult.RCode("手机号已存在");
            var salt = Global.Generator.Guid().Substring(0, 6); //6位有效字符
            BLL.UserBLL.Insert(new Model.UserModel
            {
                Account = Account,
                ClientKey = string.Empty,
                Password = Global.Hash(Password, salt),
                Salt = salt
            });
            var user = BLL.UserBLL.QueryModelByAccount(Account);
            if (user == null)
                return ApiResult.RCode("注册失败");

            var ip = SiteContext.RequestInfo._ClientIP(HttpContext).ToString();
            var useragent = Request.Headers["User-Agent"].ToString();
            var acceptlanguage = Request.Headers["Accept-Language"].ToString();
            BLL.UserExtendBLL.Insert(new Model.UserExtendModel
            {
                BankAccount = string.Empty,
                BankPersonName = string.Empty,
                BankType = EBankType.工商银行,
                Email = Email,
                IdCard = string.Empty,
                Name = string.Empty,
                QQ = QQ,
                TelPhone = Telphone,
                RegisterIP = ip,
                RegisterDate = DateTime.Now,
                UserAgent = useragent,
                AcceptLanguage = acceptlanguage,
                UserId = user.UserId
            });

            // var StoreId = Global.Generator.DateId(1);
            //
            // BLL.StoreBLL.Insert(new Model.Store
            // {
            //     Email = Email,
            //     Name = string.Empty,
            //     QQ = QQ,
            //     TelPhone = Telphone,
            //     UserId = user.UserId,
            //     DomainSub = string.Empty,
            //     DomainTop = string.Empty,
            //     Level = EStoreLevel.无,
            //     Memo = string.Empty,
            //     Template = EStoreTemplate.模板一,
            //     StoreId = StoreId,
            //     Amount = 0,
            //     PaymentJson = "[]",
            //     UniqueId = StoreId
            // });

            UserLog(user.UserId, EUserLogType.注册, Request);

            return ApiResult.RCode("");
        }

        public IActionResult Loginout()
        {
            var user = UserCurrent();
            user.ClientKey = "";
            BLL.UserBLL.Update(user);
            return ApiResult.RCode("");
        }

        public IActionResult UserBasicSave([FromForm] string Name, [FromForm] string QQ, [FromForm] string Email,
            [FromForm] string Telphone, [FromForm] string Idcard, [FromForm] int Banktype,
            [FromForm] string Bankaccount, [FromForm] string BankPersonName)
        {
            var user = UserCurrent();

            if (!string.IsNullOrEmpty(Idcard) && !Utils.IDCard.IsIDCard(Idcard))
                return ApiResult.RCode("身份证号格式不正确");


            var userextend = BLL.UserExtendBLL.QueryModelByUserId(user.UserId);

            // if (!string.IsNullOrEmpty(Email))
            // {
            //     var data = BLL.UserExtendBLL.QueryModelByEmail(Email);
            //     if (data != null && data.UserId != userextend.UserId)
            //         return ApiResult.RCode("联系邮箱已存在");
            // }
            // if (!string.IsNullOrEmpty(Telphone))
            // {
            //     var data = BLL.UserExtendBLL.QueryModelByTelPhone(Telphone);
            //     if (data != null && data.UserId != userextend.UserId)
            //         return ApiResult.RCode("手机号已存在");
            // }
            // if (!string.IsNullOrEmpty(Idcard))
            // {
            //     var data = BLL.UserExtendBLL.QueryModelByIdCard(Idcard);
            //     if (data != null && data.UserId != userextend.UserId)
            //         return ApiResult.RCode("身份证号已存在");
            // }

            userextend.Name = Name;
            userextend.QQ = QQ;
            userextend.TelPhone = Telphone;
            userextend.Email = Email;
            userextend.IdCard = Idcard;
            userextend.BankAccount = Bankaccount;
            userextend.BankPersonName = BankPersonName;
            userextend.BankType = (EBankType) Banktype;
            BLL.UserExtendBLL.Update(userextend);
            UserLog(userextend.UserId, EUserLogType.修改个人信息, Request, "", "个人信息修改");

            return ApiResult.RData(userextend);
        }

        public IActionResult OrderPageList([FromForm] string StoreId, [FromForm] DateTime Begin,
            [FromForm] DateTime End, [FromForm] int State, [FromForm] int Keykind, [FromForm] string Key,
            [FromForm] bool Ishasreturn, [FromForm] int Timetype, [FromForm] int PageIndex, [FromForm] int PageSize)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");
            Begin = Begin.Date;
            End = End.Date.AddDays(1).AddSeconds(-1);

            if (State == (int) EState.用户下单 && Timetype == (int) EOrderTimeType.付款日期)
            {
                return ApiResult.RData(new GridData<Model.OrderModel>(new List<Model.OrderModel>(), 0, new
                {
                    Income = 0,
                    ReturnAmount = 0,
                    Cost = 0,
                    CostNoClose = 0,
                    PaymentFee = -0,
                    //PaymentFeeReturn = paymentfeereturn,
                    //ProfitGross = profilegross,
                    Profit = 0
                }));
            }

            var orderstat = OrderListStat(BLL.OrderBLL.QueryList(Begin, End, State, Keykind, Key, store.StoreId,
                Ishasreturn, (EOrderTimeType) Timetype));

            var res = BLL.OrderBLL.QueryPageList(Begin, End, State, Keykind, Key, store.StoreId, Ishasreturn,
                (EOrderTimeType) Timetype, PageIndex, PageSize);
            return ApiResult.RData(new GridData<Model.OrderModel>(res.Rows, (int) res.Total, orderstat));
        }

        public dynamic OrderListStat(List<Model.OrderModel> orders)
        {
            double incomme = 0,
                returnamount = 0,
                cost = 0,
                costnoclose = 0,
                paymentfee = 0,
                //paymentfeereturn = 0,
                //profilegross = 0,
                profit = 0;

            foreach (var item in orders.Where(p => p.IsPay && p.Income > 0))
            {
                //实际收款(退款金额)
                incomme += item.Income;
                returnamount += item.ReturnAmount;
                //合计成本(待结算)
                cost += item.Cost + item.PaymentFee;
                if (!item.IsSettle)
                    costnoclose += item.Cost;
                //手续费(退款返还)
                paymentfee += item.PaymentFee;
                //毛利润（总收入-总支出） //纯利润（毛利润-总手续费）
                profit += item.Income - item.ReturnAmount - item.Cost - item.PaymentFee;
            }

            return new
            {
                Income = incomme,
                ReturnAmount = returnamount,
                Cost = cost,
                CostNoClose = costnoclose,
                PaymentFee = -paymentfee,
                // PaymentFeeReturn = paymentfeereturn,
                // ProfitGross = profilegross,
                Profit = profit
            };
        }

        public IActionResult OrderPageListIsPaid([FromForm] int PageIndex, [FromForm] int PageSize,
            [FromForm] string StoreId)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");

            var res = BLL.OrderBLL.QueryPageListIsPaidByStoreId(store.StoreId, PageIndex, PageSize);
            return ApiResult.RData(new GridData<Model.OrderModel>(res.Rows, (int) res.Total));
        }

        public IActionResult OrderPageListBySid([FromForm] string StoreId, [FromForm] string SId, [FromForm] int State,
            [FromForm] int Keykind, [FromForm] string Key, [FromForm] bool Ishasreturn, [FromForm] int PageIndex,
            [FromForm] int PageSize)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");

            var orderstat =
                OrderListStat(BLL.OrderBLL.QueryListBySid(SId, State, Keykind, Key, store.StoreId, Ishasreturn));

            var res = BLL.OrderBLL.QueryPageListBySid(SId, State, Keykind, Key, store.StoreId, Ishasreturn,
                PageIndex, PageSize);

            return ApiResult.RData(new GridData<Model.OrderModel>(res.Rows, (int) res.Total, orderstat));
        }

        public IActionResult OrderInsert([FromForm] string StoreId, [FromForm] string Order)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");
            if (!string.IsNullOrWhiteSpace(Order))
                return ApiResult.RCode("传参错误");
            var order = Global.Json.Deserialize<Model.OrderModel>(Order);
            if (order == null)
                return ApiResult.RCode("传参错误");

            var product = BLL.ProductBLL.QueryModelByProductIdAndStoreId(order.ProductId, store.StoreId);
            if (product == null)
                return ApiResult.RCode("产品不存在");
            if (product.DeliveryType == EDeliveryType.卡密)
            {
                var stockcount =
                    BLL.StockBLL.QueryCountBySupplyIdCanUse(product.SupplyId);
                if (stockcount < product.QuantityMin)
                    return ApiResult.RCode("库存数量不足");
                if (order.Quantity > stockcount)
                    return ApiResult.RCode("下单数量不能大于库存数量");
                if (order.Quantity < product.QuantityMin)
                    return ApiResult.RCode("下单数量不能小于最小下单量");
            }
            else if (order.Quantity < product.QuantityMin)
                return ApiResult.RCode("下单数量不能小于最小下单量");

            if (string.IsNullOrEmpty(order.NoticeAccount))
                return ApiResult.RCode("手机号或电子邮箱不能为空");

            Model.Extend.Payment payment = SiteContext.Store.GetPaymentList(store)
                .FirstOrDefault(p => p.PaymentType == order.PaymentType);

            if (payment == null)
                return ApiResult.RCode("支付方式不存在");
            if (!string.IsNullOrEmpty(order.TranId) &&
                BLL.OrderBLL.QueryModelByTranId(order.TranId) != null)
                return ApiResult.RCode("交易编号已存在");
            //var price = product.Amount;
            double discount = 0;
            var orderfee = order.NoticeAccount.Contains("@") ? 0 : 0;
            // var supplier = string.IsNullOrEmpty(product.SupplierId)
            //     ? null
            //     : BLL.SupplierBLL.QueryModelBySId(product.SupplierId);


            var ip = SiteContext.RequestInfo._ClientIP(HttpContext).ToString();
            var useragent = Request.Headers["User-Agent"].ToString();
            var acceptlanguage = Request.Headers["Accept-Language"].ToString();

            var data = new Model.OrderModel
            {
                Amount = order.Amount,
                ClientIP = ip,
                Contact = order.Contact,
                Cost = product.Cost * order.Quantity,
                CreateDate = order.CreateDate,
                Discount = discount,
                Income = string.IsNullOrEmpty(order.TranId) ? 0 : order.Amount,
                IsPay = order.IsPay,
                IsDelivery = order.IsPay && order.IsDelivery,
                Memo = product.Memo,
                Name = product.Name,
                NoticeAccount = order.NoticeAccount,
                OrderId = Global.Generator.DateId(2),
                PaymentFee = order.Amount * payment.Rate,
                PaymentType = payment.PaymentType,
                ProductId = product.ProductId,
                Quantity = order.Quantity,
                ReturnAmount = 0,
                ReturnDate = DateTime.Now,
                DeliveryDate = DateTime.Now,
                StockList = new List<StockOrder>(),
                StoreId = store.StoreId,
                TranId = order.IsPay ? order.TranId : string.Empty,
                UserAgent = useragent,
                AcceptLanguage = acceptlanguage,
                IsSettle = false,
                SettleDate = DateTime.Now,
                SupplyId = product.SupplyId,
                LastUpdateDate = DateTime.Now,
            };

            BLL.OrderBLL.Insert(data);

            UserLog(store.UserId, EUserLogType.订单管理, Request, store.StoreId,
                "订单手动添加");

            return ApiResult.RCode("");
        }

        public IActionResult OrderDelivery([FromForm] string StoreId, [FromForm] string OrderId) // 保存了两次订单
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");

            if (string.IsNullOrEmpty(OrderId))
                return ApiResult.RCode("订单不存在");
            var order = BLL.OrderBLL.QueryModelByOrderIdAndStoreId(OrderId, store.StoreId);
            if (order == null)
                return ApiResult.RCode("订单不存在");
            if (!order.IsPay)
                return ApiResult.RCode("订单尚未付款，不能操作");
            if (order.IsDelivery)
                return ApiResult.RCode("订单已发货，不能操作");

            SiteContext.OrderHelper.Delivery(order); //更新了订单

            UserLog(store.UserId, EUserLogType.订单管理, Request, store.StoreId, "订单手动发货");

            return ApiResult.RCode("");
        }

        public IActionResult OrderEmail([FromForm] string StoreId, [FromForm] string OrderId)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");

            if (string.IsNullOrEmpty(OrderId))
                return ApiResult.RCode("订单ID不能为空");
            var order = BLL.OrderBLL.QueryModelByOrderIdAndStoreId(OrderId, store.StoreId);
            if (order == null)
                return ApiResult.RCode("订单不存在");
            if (!order.IsDelivery)
                return ApiResult.RCode("订单尚未发货，不能手动发邮件");

            SiteContext.OrderHelper.DeliveryEmail(order);

            UserLog(store.UserId, EUserLogType.订单管理, Request, store.StoreId, "订单邮件发货");

            return ApiResult.RCode("");
        }

        public IActionResult OrderReturn([FromForm] string StoreId, [FromForm] string OrderId,
            [FromForm] double CostUpdate, [FromForm] double ReturnAmount)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");
            if (string.IsNullOrEmpty(OrderId))
                return ApiResult.RCode("订单ID不能为空");
            var order = BLL.OrderBLL.QueryModelByOrderIdAndStoreId(OrderId, store.StoreId);
            if (order == null)
                return ApiResult.RCode("订单不存在");
            if (order.Income < ReturnAmount)
                return ApiResult.RCode("退款金额应不大于到账金额");
            if (!order.IsPay)
                return ApiResult.RCode("订单尚未付款，不能操作");

            var amountchange = ReturnAmount - order.ReturnAmount;
            order.Cost = CostUpdate;
            order.ReturnAmount = ReturnAmount;

            order.ReturnDate = DateTime.Now;
            order.LastUpdateDate = DateTime.Now;
            if (!order.IsDelivery && string.Equals(order.Amount.ToString("f2"), order.ReturnAmount.ToString("f2"),
                StringComparison.OrdinalIgnoreCase))
            {
                order.IsDelivery = true;
                order.DeliveryDate = DateTime.Now;
            }

            var storemodel = BLL.StoreBLL.QueryModelByStoreId(order.StoreId);
            if (storemodel == null)
                return ApiResult.RCode("店铺不存在，请检查数据库");
            if (storemodel.Amount < amountchange)
                return ApiResult.RCode("店铺资金小于退款金额，请联系店铺管理人员解决");
            BLL.StoreBLL.ChangeAmount(store.StoreId, -amountchange); //店铺金额减去变动金额

            BLL.OrderBLL.Update(order);
            UserLog(store.UserId, EUserLogType.订单管理, Request, store.StoreId, "订单退款申请");
            return ApiResult.RCode("");
        }

        public IActionResult OrderUpdateIsSettle([FromForm] string StoreId, [FromForm] string OrderIds)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");
            if (string.IsNullOrWhiteSpace(OrderIds))
                return ApiResult.RCode("订单编号错误");
            var ids = Global.Json.Deserialize<List<string>>(OrderIds);
            if (ids == null || ids.Count < 0)
                return ApiResult.RCode("订单编号错误");

            BLL.OrderBLL.UpdateIsSettle(ids, store.StoreId, true, DateTime.Now);
            UserLog(store.UserId, EUserLogType.订单管理, Request, store.StoreId, "批量结算");
            return ApiResult.RCode("");
        }

        public IActionResult UserPwdModify([FromForm] string PwdOld, [FromForm] string PwdNew)
        {
            var user = UserCurrent();
            if (!string.Equals(Global.Hash(PwdOld, user.Salt), user.Password,
                StringComparison.OrdinalIgnoreCase))
                return ApiResult.RCode("旧密码不正确");
            user.Password = Global.Hash(PwdNew, user.Salt);
            BLL.UserBLL.Update(user);
            UserLog(user.UserId, EUserLogType.修改个人信息, Request, "", "密码修改");
            return ApiResult.RCode("");
        }

        public IActionResult UserLogPageList([FromForm] string StoreId, [FromForm] int PageIndex,
            [FromForm] int Pagesize, [FromForm] int UserLogType, [FromForm] DateTime Begin, [FromForm] DateTime End)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");

            Begin = Begin.Date;
            End = End.Date.AddDays(1).AddSeconds(-1);
            var res = BLL.UserLogBLL.QueryPageListByUserIdOrStoreIdOrType(user.UserId, store.StoreId, UserLogType,
                Begin,
                End, PageIndex, Pagesize);

            return ApiResult.RData(new GridData<Model.UserLogModel>(res.Rows, (int) res.Total));
        }

        public IActionResult ProducPageList([FromForm] string StoreId, [FromForm] int PageIndex,
            [FromForm] int Pagesize)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");

            var res = BLL.ProductBLL.QueryPageListByStoreId(store.StoreId, PageIndex, Pagesize);
            return ApiResult.RData(new GridData<Model.ProductModel>(res.Rows, (int) res.Total));
        }

        // public IActionResult SupplierList([FromForm] string StoreId)
        // {
        //     var user = User(StoreId, out Model.Store store);
        //     if (store != null)
        //         return ApiResult.RCode("未知错误");
        //
        //     return new JsonResult(Api.UserApi.Supplier.GetAllList(store));
        // }

        //todo  StoreId=>UserId 前端需要配合修改
        public IActionResult ProductSupplyPageList([FromForm] int PageIndex,
            [FromForm] int Pagesize)
        {
            var user = UserCurrent();

            var res = BLL.SupplyBLL.QueryPageListByUserIds(
                new List<int>() {user.UserId, SiteContext.Config.SupplyUserIdSys}, PageIndex, Pagesize);
            return ApiResult.RData(
                new GridData<Model.SupplyModel>(res.Rows, (int) res.Total));
        }

        public IActionResult ProductAddFromSupply([FromForm] string StoreId, [FromForm] string Ids,
            [FromForm] string Category)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");
            if (string.IsNullOrWhiteSpace(Ids))
                return ApiResult.RCode("商品编号错误");
            var ids = Global.Json.Deserialize<List<string>>(Ids);
            if (ids == null)
                return ApiResult.RCode("商品编号错误");
            if (string.IsNullOrEmpty(Category))
                return ApiResult.RCode("产品分类不存在");


            var productlist = new List<Model.ProductModel>();
            foreach (var item in BLL.SupplyBLL.QueryListByIds(ids))
            {
                productlist.Add(new Model.ProductModel
                {
                    Amount = item.FaceValue,
                    Category = Category,
                    Cost = (item.Cost * SiteContext.Config.SupplyRates[store.Level]),
                    IsShow = false,
                    Memo = item.Name,
                    Name = item.Name,
                    ProductId = Global.Generator.DateId(1),
                    QuantityMin = 1,
                    Sort = 0,
                    DeliveryType = item.DeliveryType,
                    StoreId = StoreId,
                    SupplyId = item.SupplyId,
                });
            }

            BLL.ProductBLL.InsertRange(productlist);
            return ApiResult.RCode("");
        }

        public IActionResult ProductSave([FromForm] string StoreId, [FromForm] string Product)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");


            if (string.IsNullOrWhiteSpace(Product))
                return ApiResult.RCode("商品信息为空");
            var product = Global.Json.Deserialize<Model.ProductModel>(Product);
            if (product == null)
                return ApiResult.RCode("商品信息数据格式错误");
            if (string.IsNullOrEmpty(product.Name))
                return ApiResult.RCode("产品名称不能为空");

            if (product.Amount <= 0)
                return ApiResult.RCode("产品单价应为正数");

            if (product.QuantityMin < 1)
                return ApiResult.RCode("最小购买量必须大于0");


            if (string.IsNullOrEmpty(product.ProductId))
            {
                //product.StoreId = store.StoreId;
                //product.ProductId = Global.Generator.DateId(2);
                if (!string.IsNullOrEmpty(product.Icon) && product.Icon.StartsWith("/Resouce/Product/"))
                {
                    var str = product.Icon.Replace("/Resouce/Product/", ""); //取图片地址的id值
                    if (str.Contains("/"))
                    {
                        product.ProductId = str.Split("/")[0];
                    }
                }

                if (string.IsNullOrWhiteSpace(product.ProductId))
                {
                    product.ProductId = Global.Generator.DateId(2);
                }

                product.Icon = SiteContext.Resource.MoveTempFile(product.Icon);
                BLL.ProductBLL.Insert(product);
                UserLog(user.UserId, EUserLogType.产品管理, Request, store.StoreId,
                    "产品增加" + product.ProductId);
                return ApiResult.RCode("");
            }
            else
            {
                var datacompare =
                    BLL.ProductBLL.QueryModelByProductIdAndStoreId(product.ProductId, store.StoreId);
                if (datacompare == null)
                    return ApiResult.RCode("产品不存在或已被删除");
                product.Icon = SiteContext.Resource.MoveTempFile(product.Icon);
                BLL.ProductBLL.Update(product);
                UserLog(user.UserId, EUserLogType.产品管理, Request, store.StoreId,
                    "产品修改" + product.ProductId);
                return ApiResult.RCode("");
            }
        }

        public IActionResult ProductDelete([FromForm] string StoreId, [FromForm] string ProductId)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");

            if (string.IsNullOrEmpty(ProductId))
                return ApiResult.RCode("产品编号错误");
            var product = BLL.ProductBLL.QueryModelByProductIdAndStoreId(ProductId, store.StoreId);
            if (product == null)
                return ApiResult.RCode("产品不存在或已被删除");

            BLL.ProductBLL.DeleteByProductIdAndStoreId(ProductId, store.StoreId);
            UserLog(store.UserId, EUserLogType.产品管理, Request, store.StoreId, "产品删除");
            return ApiResult.RCode("");
        }

        public IActionResult ProductModifyIsShow([FromForm] string StoreId, [FromForm] string ProductId,
            [FromForm] bool IsShow)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");


            if (string.IsNullOrEmpty(ProductId))
                return ApiResult.RCode("产品ID不能为空");

            var product = BLL.ProductBLL.QueryModelByProductIdAndStoreId(ProductId, store.StoreId);
            if (product == null)
                return ApiResult.RCode("产品不存在或已被删除");

            if (product.IsShow != IsShow)
            {
                product.IsShow = IsShow;
                BLL.ProductBLL.Update(product);
                UserLog(store.UserId, EUserLogType.产品管理, Request,
                    store.StoreId, "产品" + (IsShow ? "上架" : "下架"));
            }

            return ApiResult.RCode("");
        }

        public IActionResult StoreSave([FromForm] string Store)
        {
            var store = Global.Json.Deserialize<Model.StoreModel>(Store);
            if (store == null)
                return ApiResult.RCode("店铺数据格式错误");
            var user = UserCurrent(store.StoreId, out Model.StoreModel store2);
            if (store2 != null)
                return ApiResult.RCode("未知错误");


            // if (string.Equals("admin", store.UniqueId, StringComparison.OrdinalIgnoreCase) ||
            //     string.Equals("user", store.UniqueId, StringComparison.OrdinalIgnoreCase))
            //     return ApiResult.RCode(store.UniqueId + "是系统关键词，不能使用");

            if (store.UniqueId.Length <= 5)
                return ApiResult.RCode("店铺标识长度必须大于5位");

            if (!string.IsNullOrWhiteSpace(store.UniqueId))
            {
                var compare = BLL.StoreBLL.QueryModelByUniqueId(store.UniqueId);
                if (compare != null && !string.Equals(compare.StoreId, store.StoreId,
                    StringComparison.OrdinalIgnoreCase))
                    return ApiResult.RCode(store.UniqueId + "已被使用");
            }

            var storeOrgin = BLL.StoreBLL.QueryModelByStoreId(store.StoreId);

            store.Initial = Global.Initial(store.Name);

            store.StoreId = storeOrgin.StoreId;
            store.UserId = storeOrgin.UserId;
            store.Level = storeOrgin.Level;
            store.PaymentList = storeOrgin.PaymentList;
            store.Amount = storeOrgin.Amount;
            store.Logo = SiteContext.Resource.MoveTempFile(store.Logo);
            BLL.StoreBLL.Update(store);

            UserLog(store.UserId, EUserLogType.修改店铺信息, Request, store.StoreId, "店铺信息修改");
            return ApiResult.RData(store);
        }

        public IActionResult OrderPageListBenifit([FromForm] string StoreId, [FromForm] string SId,
            [FromForm] bool IsHasReturn, [FromForm] DateTime Begin, [FromForm] DateTime End, [FromForm] int PageIndex,
            [FromForm] int Pagesize)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");

            Begin = Begin.Date;
            End = End.Date.AddDays(1).AddSeconds(-1);
            var res = BLL.OrderBLL.QueryPageListBenifitByStoreId(SId, Begin, End, store.StoreId, IsHasReturn,
                PageIndex, Pagesize);


            var orderstat = OrderListStat(BLL.OrderBLL.QueryListBenifitByStoreId(SId, Begin, End, store.StoreId,
                IsHasReturn));


            return ApiResult.RData(new GridData<Model.OrderModel>(res.Rows, (int) res.Total, orderstat));
        }

        public IActionResult WithDrawInsert([FromForm] string StoreId, [FromForm] double Amount,
            [FromForm] int BankType, [FromForm] string Bankaccount, [FromForm] string BankPersonName,
            [FromForm] string Name)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");

            if (string.IsNullOrEmpty(Name))
                return ApiResult.RCode("收款名称不能为空");
            // if (string.IsNullOrEmpty(BankPersonName))
            //     return ApiResult.RCode("收款开户行不能为空");
            if (string.IsNullOrEmpty(Bankaccount))
                return ApiResult.RCode("收款帐号不能为空");

            if (Amount < SiteContext.Config.WithDrawMin)
                return ApiResult.RCode($"提现金额必须大于等于{SiteContext.Config.WithDrawMin}");
            if (Amount > SiteContext.Config.WithDrawMax)
                return ApiResult.RCode($"提现金额必须小于等于{SiteContext.Config.WithDrawMax}");

            if (string.IsNullOrEmpty(StoreId))
                return ApiResult.RCode("店铺ID不能为空");
            if (user == null)
                return ApiResult.RCode("你没登录，请登录后操作");
            if (Amount > store.Amount)
                return ApiResult.RCode("提现金额不能大于当前金额");
            var withdraw = new Model.WithDrawModel
            {
                Amount = Amount,
                BankAccount = Bankaccount,
                BankPersonName = BankPersonName,
                BankType = (EBankType) BankType,
                CreateDate = DateTime.Now,
                FinishDate = DateTime.Now,
                Income = 0,
                IsFinish = false,
                Memo = string.Empty,
                StoreId = store.StoreId,
                TranId = string.Empty,
                WithDrawId = Global.Generator.DateId()
            };

            BLL.WithDrawBLL.Insert(withdraw);
            UserLog(store.UserId, EUserLogType.提现, Request, store.StoreId, "提现申请发布");
            return ApiResult.RCode("");
        }

        public IActionResult WithDrawPageList([FromForm] string StoreId, [FromForm] int PageIndex,
            [FromForm] int Pagesize, [FromForm] int State, [FromForm] DateTime Begin, [FromForm] DateTime End)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");


            Begin = Begin.Date;
            End = End.Date.AddDays(1).AddSeconds(-1);
            var res = BLL.WithDrawBLL.QueryPageList(store.StoreId, State, Begin, End, PageIndex, Pagesize);
            return ApiResult.RData(new GridData<Model.WithDrawModel>(res.Rows, (int) res.Total));
        }

        public IActionResult WithDrawDelete([FromForm] string StoreId, [FromForm] string Id)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");
            if (string.IsNullOrEmpty(Id))
                return ApiResult.RCode("ID不能为空");
            if (user == null)
                return ApiResult.RCode("你没登录，请登录后操作");

            var withdraw = BLL.WithDrawBLL.QueryModelByWithDrawIdAndStoreId(Id, store.StoreId);
            if (withdraw == null)
                return ApiResult.RCode("数据不存在或已被删除");
            if (withdraw.IsFinish)
                return ApiResult.RCode("提现申请已处理，不能删除");
            BLL.WithDrawBLL.DeleteByWithDrawIdAndStoreId(Id, store.StoreId);
            UserLog(store.UserId, EUserLogType.提现, Request, store.StoreId, "提现申请删除");
            return ApiResult.RCode("");
        }

        public IActionResult ProductListIsStock([FromForm] string StoreId, [FromForm] string Category)
        {
            var user = UserCurrent(StoreId, out Model.StoreModel store);
            if (store != null)
                return ApiResult.RCode("未知错误");
            return ApiResult.RData(BLL.ProductBLL
                .QueryListByCategoryIdAndStoreIdIsStock(Category, store.StoreId));
        }


        public IActionResult StockPageList([FromForm] string SupplyId,
            [FromForm] bool IsShow,
            [FromForm] int PageIndex, [FromForm] int Pagesize)
        {
            var user = UserCurrent();

            var res = BLL.StockBLL.QueryPageListBySupplyId(SupplyId, user.UserId, IsShow, PageIndex,
                Pagesize);

            return ApiResult.RData(new GridData<Model.StockModel>(res.Rows, (int) res.Total));
        }

        //todo 后期修改优化此接口
        public IActionResult StockInsert([FromForm] string SupplyId, [FromForm] bool Spilt,
            [FromForm] string Stock, [FromForm] bool CheckRepeatName, [FromForm] bool CheckRepeatPwd)
        {
            var user = UserCurrent();

            if (string.IsNullOrEmpty(Stock))
                return ApiResult.RCode("卡号卡密不能为空");
            var supply = string.IsNullOrEmpty(SupplyId)
                ? null
                : BLL.SupplyBLL.QueryModelById(SupplyId);
            if (supply == null || supply.DeliveryType != EDeliveryType.卡密)
                return ApiResult.RCode("货源不存在或已被删除");
            Stock = Stock.Replace("\r", string.Empty); //不用加换行符了
            var stocklist = BLL.StockBLL.QueryListBySupplyIdCanUse(SupplyId, user.UserId);
            var repaet_name = new List<string>();
            var repaet_pwd = new List<string>();
            string card_name, card_pwd;
            var ids = new List<string>();
            foreach (var card in Stock.Split('\n'))
            {
                if (Spilt)
                {
                    card_name = card.Split("|||")[0];
                    card_pwd = card.Split("|||")[1];
                    if (!string.IsNullOrEmpty(card_name) || !string.IsNullOrEmpty(card_pwd))
                    {
                        if (CheckRepeatName && stocklist.FirstOrDefault(p => p.Name == card_name) != null)
                        {
                            repaet_name.Add(card_name);
                        }
                        else if (CheckRepeatPwd &&
                                 stocklist.FirstOrDefault(p => p.Memo == card_pwd) != null) //密码重复验证？
                        {
                            repaet_pwd.Add(card_pwd);
                        }
                        else
                        {
                            var id = Global.Generator.DateId(2);
                            do
                            {
                                id = Global.Generator.DateId(2);
                            } while (ids.Contains(id));

                            ids.Add(id);
                            stocklist.Add(new Model.StockModel
                            {
                                Name = card_name,
                                Memo = card_pwd,
                                CreateDate = DateTime.Now,
                                DeliveryDate = DateTime.Now,
                                IsShow = false,
                                IsDelivery = false,
                                SupplyId = SupplyId,
                                StockId = id,
                                UserId = user.UserId
                            });
                        }
                    }
                }
            }


            BLL.StockBLL.InsertRange(stocklist);
            UserLog(user.UserId, EUserLogType.库存管理, Request,
                "", "增加库存" + stocklist.Count + "条");
            string res = "你一共增加了" + stocklist.Count + "个库存 ";
            if (repaet_name.Count > 0)
            {
                res += "重复卡号" + repaet_name.Count + "个，分别是:<ul>";
                foreach (var item in repaet_name)
                {
                    res += "<li>" + item + "</li>";
                }

                res += "</ul>";
            }

            if (repaet_pwd.Count > 0)
            {
                res += "重复密码" + repaet_pwd.Count + "个，分别是:<ul>";
                foreach (var item in repaet_pwd)
                {
                    res += "<li>" + item + "</li>";
                }

                res += "</ul>";
            }

            return ApiResult.RCode(res, ApiResult.ECode.Success);
        }


        public IActionResult StockDelete([FromForm] string StockIds)
        {
            var user = UserCurrent();

            var stockids = Global.Json.Deserialize<List<string>>(StockIds);
            if (stockids == null || stockids.Count == 0)
                return ApiResult.RCode("库存编号为空");


            BLL.StockBLL.DeleteByStockIds(stockids, user.UserId);

            UserLog(user.UserId, EUserLogType.库存管理, Request, "", "库存删除" + StockIds);
            return ApiResult.RCode("");
        }


        public IActionResult StockSetIsShow([FromForm] string StockIds,
            [FromForm] bool IsShow)
        {
            var user = UserCurrent();

            var stockids = Global.Json.Deserialize<List<string>>(StockIds);
            if (stockids == null || stockids.Count == 0)
                return ApiResult.RCode("库存编号为空");

            BLL.StockBLL.UpdateIsShowByStockIds(stockids, user.UserId, IsShow);

            UserLog(user.UserId, EUserLogType.库存管理, Request, "",
                "库存" + (IsShow ? "上架" : "下架") + StockIds);

            return ApiResult.RCode("");
        }


        // public IActionResult SupplierPageList([FromForm] string StoreId, [FromForm] int PageIndex,
        //     [FromForm] int Pagesize)
        // {
        //     var user = User(StoreId, out Model.Store store);
        //     if (store != null)
        //         return ApiResult.RCode("未知错误");
        //     return new JsonResult(Api.UserApi.Supplier.GetPageList(user, StoreId, PageIndex, Pagesize));
        // }
        //
        // public IActionResult SupplierSave([FromForm] string StoreId, [FromForm] string Sid, [FromForm] string Name,
        //     [FromForm] int BankType, [FromForm] string Email, [FromForm] string BankAccount, [FromForm] string QQ,
        //     [FromForm] double Feerate)
        // {
        //     var user = User(StoreId, out Model.Store store);
        //     if (store != null)
        //         return ApiResult.RCode("未知错误");
        //     if (string.IsNullOrEmpty(Name))
        //         return ApiResult.RCode("供货商名称不能为空");
        //     EBankType ebanktype = (EBankType) BankType;
        //     if (ebanktype == 0)
        //         return ApiResult.RCode("请输入正确的账户类型");
        //     if (user == null)
        //         return ApiResult.RCode("你没登录，请登录后操作");
        //
        //     if (string.IsNullOrEmpty(Sid))
        //     {
        //         if (BLL.SupplierBLL.QueryModelByStoreIdAndName(store.StoreId, Name) != null)
        //             return ApiResult.RCode("供货商名称已存在");
        //         if (BLL.SupplierBLL.QueryModelByStoreIdAndEmail(store.StoreId, Email) != null)
        //             return ApiResult.RCode("供货商邮箱已存在");
        //         var supplier = new Model.Supplier
        //         {
        //             Email = Email, BankType = ebanktype, SId = Global.Generator.DateId(1), QQ = QQ, Name = Name,
        //             BankAccount = BankAccount, UserId = user.UserId, FeeRate = Feerate
        //         };
        //         BLL.SupplierBLL.Insert(supplier);
        //         UserLog(store.UserId, EUserLogType.供货商管理, Request, store.StoreId,
        //             "供货商添加");
        //         return ApiResult.RCode("");
        //     }
        //     else
        //     {
        //         var data = BLL.SupplierBLL.QueryModelBySId(Sid);
        //         if (data == null)
        //             return ApiResult.RCode("供货商不存在或已被删除");
        //         var datacompare = BLL.SupplierBLL.QueryModelByStoreIdAndName(store.StoreId, Name);
        //         if (datacompare != null &&
        //             !string.Equals(data.SId, datacompare.SId, StringComparison.OrdinalIgnoreCase))
        //             return ApiResult.RCode("供货商名称已存在");
        //         datacompare = BLL.SupplierBLL.QueryModelByStoreIdAndEmail(store.StoreId, Email);
        //         if (datacompare != null &&
        //             !string.Equals(data.SId, datacompare.SId, StringComparison.OrdinalIgnoreCase))
        //             return ApiResult.RCode("供货商邮箱已存在");
        //         data.BankAccount = BankAccount;
        //         data.BankType = ebanktype;
        //         data.QQ = QQ;
        //         data.Name = Name;
        //         data.FeeRate = Feerate;
        //         data.Email = Email;
        //         BLL.SupplierBLL.Update(data);
        //         UserLog(store.UserId, EUserLogType.供货商管理, Request, store.StoreId,
        //             "供货商修改");
        //         return ApiResult.RCode("");
        //     }
        // }
        //
        // public IActionResult SupplierDelete([FromForm] string StoreId, [FromForm] string Sid)
        // {
        //     var user = User(StoreId, out Model.Store store);
        //     if (store != null)
        //         return ApiResult.RCode("未知错误");
        //     return new JsonResult(Api.UserApi.Supplier.Delete(user, StoreId, Sid,
        //         SiteContext.RequestInfo._ClientIP(HttpContext).ToString(),
        //         SiteContext.RequestInfo._UserAgent(HttpContext)));
        // }
        //
        // public IActionResult SupplierModel([FromForm] string Sid)
        // {
        //     var res = new Msg<Model.Supplier>();
        //     var user = User();
        //     var model = BLL.SupplierBLL.QueryModelBySId(Sid);
        //     if (model == null)
        //     {
        //         res.Result = false;
        //         res.Message = "供应商不存在";
        //     }
        //     else
        //     {
        //         res.Data = model;
        //     }
        //
        //     return new JsonResult(res);
        // }

        public async Task<IActionResult> UploadFormFile([FromForm] string Model, [FromForm] string Id,
            [FromForm] string Name)
        {
            try
            {
                if (!string.IsNullOrWhiteSpace(Model))
                {
                    if (Request.Form.Files.Count > 0)
                    {
                        var files = new Dictionary<string, byte[]>();
                        for (var i = 0; i < Request.Form.Files.Count; i++)
                        {
                            using (var stream = Request.Form.Files[i].OpenReadStream())
                            {
                                byte[] buffer = new byte[Request.Form.Files[i].Length];
                                await stream.ReadAsync(buffer, 0, buffer.Length);
                                files.Add(Request.Form.Files[i].ContentType,
                                    buffer); //  {name: {key:contenttype,value:byte[]},}
                            }
                        }

                        if (string.IsNullOrWhiteSpace(Id))
                        {
                            Id = Global.Generator.DateId(2);
                        }

                        var res = SiteContext.Resource.UploadFiles(Model, Id, Name, files);
                        return new JsonResult(res);
                    }
                }
                else
                {
                    return ApiResult.RCode("参数错误");
                }
            }
            catch
            {
            }

            return ApiResult.RCode("未上传任何文件");
        }
    }
}