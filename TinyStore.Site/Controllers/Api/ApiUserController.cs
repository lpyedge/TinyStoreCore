using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TinyStore.Model;

namespace TinyStore.Site.Controllers
{
    [ApiController]
    [MultipleSubmit]
    [UserHeaderToken("Login", "Register")]
    [Produces("application/json")]
    [Route("ApiUser/[action]")]
    public class ApiUserController : ControllerBase
    {
        private UserModel UserCurrent(string storeid, out StoreModel store)
        {
            UserModel user = UserCurrent();
            if (string.IsNullOrWhiteSpace(storeid)) store = null;

            store = BLL.StoreBLL.QueryModelByStoreId(storeid);

            if (store != null && store.UserId != user.UserId) store = null;

            return user;
        }

        private UserModel UserCurrent()
        {
            return HttpContext.Items[UserHeaderToken.ItemKey] as UserModel;
        }

        private void UserLog(int userId, EUserLogType type, HttpRequest request, string storeId = "",
            string memo = "")
        {
            BLL.UserLogBLL.Insert(new UserLogModel
            {
                UserLogId = Global.Generator.DateId(2),
                UserLogType = type,
                UserId = userId,
                ClientIP = Utils.RequestInfo._ClientIP(request).ToString(),
                UserAgent = request.Headers["User-Agent"].ToString(),
                AcceptLanguage = request.Headers["Accept-Language"].ToString(),
                Memo = memo,
                StoreId = storeId,
                CreateDate = DateTime.Now
            });
        }


        [HttpPost]
        public IActionResult Login([FromForm] string account, [FromForm] string password)
        {
            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password))
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);
            UserModel user = BLL.UserBLL.QueryModelByAccount(account);
            if (user != null && string.Equals(user.Password, Global.Hash(password, user.Salt)))
            {
                user.ClientKey = Global.Generator.Guid();
                BLL.UserBLL.Update(user);

                Response.Headers[UserHeaderToken.TokenKey] = HeaderToken.ToToken( new HeaderToken.TokenData()
                {
                    Id = user.UserId.ToString(),
                    Key = user.ClientKey
                });

                user.ClientKey = "";
                user.Salt = "";
                user.Password = "";

                var storelist = BLL.StoreBLL.QueryListByUserId(user.UserId);
                UserExtendModel userextra = BLL.UserExtendBLL.QueryModelById(user.UserId);

                UserLog(user.UserId, EUserLogType.登录, Request);

                return ApiResult.RData(new {user, storeList = storelist, userExtend = userextra});
            }

            return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);
        }

        [HttpPost]
        public IActionResult UserExtendSave(UserExtendModel userExtend)
        {
            UserModel user = UserCurrent();

            if (!string.IsNullOrEmpty(userExtend.IdCard) && !Utils.IDCard.IsIDCard(userExtend.IdCard))
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);

            if (!string.IsNullOrEmpty(userExtend.Email) && !userExtend.Email.Contains("@"))
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);


            UserExtendModel userExtendOrigin = BLL.UserExtendBLL.QueryModelById(user.UserId);

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

            userExtendOrigin.Name = userExtend.Name;
            userExtendOrigin.QQ = userExtend.QQ;
            userExtendOrigin.TelPhone = userExtend.TelPhone;
            userExtendOrigin.Email = userExtend.Email;
            userExtendOrigin.IdCard = userExtend.IdCard;

            BLL.UserExtendBLL.Update(userExtendOrigin);
            UserLog(userExtendOrigin.UserId, EUserLogType.商户信息, Request, "", "个人信息修改");

            return ApiResult.RData(userExtendOrigin);
        }

        [HttpPost]
        public IActionResult UserWithDrawSave(UserExtendModel userExtend)
        {
            UserModel user = UserCurrent();

            if (string.IsNullOrEmpty(userExtend.BankAccount) || string.IsNullOrEmpty(userExtend.BankPersonName))
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);

            UserExtendModel userExtendOrigin = BLL.UserExtendBLL.QueryModelById(user.UserId);

            userExtendOrigin.BankType = userExtend.BankType;
            userExtendOrigin.BankAccount = userExtend.BankAccount;
            userExtendOrigin.BankPersonName = userExtend.BankPersonName;

            BLL.UserExtendBLL.Update(userExtendOrigin);

            UserLog(userExtendOrigin.UserId, EUserLogType.商户信息, Request, "", "个人信息修改");

            return ApiResult.RData(userExtendOrigin);
        }


        [HttpPost]
        public IActionResult UserWithDraw([FromForm] double amount)
        {

            UserModel user = UserCurrent();

            var data1 = BLL.WithDrawBLL.QueryList(p => p.UserId == user.UserId && !p.IsFinish);
            if (data1.Count > 0)
                return ApiResult.RCode(ApiResult.ECode.TargetExist);

            var userExtend = BLL.BaseBLL<UserExtendModel>.QueryModelById(user.UserId);
            if (userExtend.Amount < amount)
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);

            BLL.UserExtendBLL.Update(p => p.UserId == user.UserId,
                p => new UserExtendModel() {Amount = p.Amount - amount});
            
            var withDraw = new WithDrawModel()
            {
                WithDrawId = Global.Generator.DateId(1),
                UserId = user.UserId,
                Amount = amount,
                BankType = userExtend.BankType,
                BankPersonName = userExtend.BankPersonName,
                BankAccount = userExtend.BankAccount,
                CreateDate = DateTime.Now,
                Memo = "",
                IsFinish = false,
                AmountFinish = 0,
                TranId = "",
                FinishDate = null,
            };
            
            BLL.WithDrawBLL.Insert(withDraw);
            
            BLL.BillBLL.Insert(new BillModel()
            {
                BillId = Global.Generator.DateId(1),
                UserId = user.UserId,
                Amount = -amount,
                AmountCharge = 0,
                BillType = EBillType.提现,
                CreateDate = DateTime.Now,
                Extra = withDraw.WithDrawId
            });

            return ApiResult.RData(BLL.BaseBLL<UserExtendModel>.QueryModelById(user.UserId));
        }

        [HttpPost]
        public IActionResult UserPasswordModify([FromForm] string passwordOld, [FromForm] string passwordNew)
        {
            UserModel user = UserCurrent();
            if (!string.Equals(Global.Hash(passwordOld, user.Salt), user.Password,
                StringComparison.OrdinalIgnoreCase))
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);

            user.Password = Global.Hash(passwordNew, user.Salt);
            BLL.UserBLL.Update(user);
            UserLog(user.UserId, EUserLogType.商户信息, Request, "", "密码修改");
            return ApiResult.RCode();
        }

        [HttpPost]
        public IActionResult NewDateId([FromForm] int state)
        {
            return ApiResult.RData(Global.Generator.DateId(state));
        }

        [HttpPost]
        public async Task<IActionResult> UploadFormFile([FromForm] string model, [FromForm] string id,
            [FromForm] string name)
        {
            if (string.IsNullOrEmpty(model) || string.IsNullOrEmpty(id) || string.IsNullOrEmpty(name))
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);
            if (Request.Form.Files.Count == 0)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

            try
            {
                var files = new Dictionary<string, byte[]>();
                for (var i = 0; i < Request.Form.Files.Count; i++)
                    using (Stream stream = Request.Form.Files[i].OpenReadStream())
                    {
                        var buffer = new byte[Request.Form.Files[i].Length];
                        await stream.ReadAsync(buffer, 0, buffer.Length);
                        files.Add(Request.Form.Files[i].ContentType,
                            buffer); //  {name: {key:contenttype,value:byte[]},}
                    }

                if (string.IsNullOrWhiteSpace(id)) id = Global.Generator.DateId(2);

                ApiResult res = SiteContext.Resource.UploadFiles(model, id, name, files);
                return new JsonResult(res);
            }
            catch
            {
                return ApiResult.RCode(ApiResult.ECode.UnKonwError);
            }
        }

        [HttpPost]
        public IActionResult StoreSave(StoreModel store)
        {
            if (store == null)
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);
            UserModel user = UserCurrent(store.StoreId, out StoreModel storeOrigin);
            if (storeOrigin == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

            if (string.Equals("admin", store.UniqueId, StringComparison.OrdinalIgnoreCase) ||
                string.Equals("user", store.UniqueId, StringComparison.OrdinalIgnoreCase))
                return ApiResult.RCode(store.UniqueId + "是系统关键词，不能使用");

            if (store.Name.Length >= 10)
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);

            if (store.UniqueId.Length <= 5)
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);

            if (!string.IsNullOrWhiteSpace(store.UniqueId))
            {
                StoreModel compare = BLL.StoreBLL.QueryModelByUniqueId(store.UniqueId);
                if (compare != null && !string.Equals(compare.StoreId, store.StoreId,
                    StringComparison.OrdinalIgnoreCase))
                    return ApiResult.RCode(ApiResult.ECode.TargetExist);
            }

            storeOrigin.Logo = SiteContext.Resource.MoveTempFile(store.Logo);

            storeOrigin.Name = store.Name;
            storeOrigin.Initial = Global.Initial(store.Name);
            storeOrigin.UniqueId = store.UniqueId;
            storeOrigin.Template = store.Template;
            storeOrigin.Memo = string.IsNullOrWhiteSpace(store.Memo) || store.Memo.Length > 4000 ? "" : store.Memo;
            storeOrigin.Email = store.Email;
            storeOrigin.TelPhone = store.TelPhone;
            storeOrigin.QQ = store.QQ;
            storeOrigin.BlockList = store.BlockList
                .Where(p=>!string.IsNullOrWhiteSpace(p))
                .Select(p=>p.Trim())
                .ToList();

            BLL.StoreBLL.Update(storeOrigin);

            UserLog(user.UserId, EUserLogType.店铺信息, Request, store.StoreId, "店铺信息修改");

            return ApiResult.RData(BLL.StoreBLL.QueryListByUserId(user.UserId));
        }


        [HttpPost]
        public IActionResult StorePaymentListSave(StoreModel store)
        {
            if (store == null)
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);
            UserModel user = UserCurrent(store.StoreId, out StoreModel storeOrigin);
            if (storeOrigin == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

            var systemPaymentList = SiteContext.Payment.SystemPaymentList();
            foreach (Model.PaymentView item in store.PaymentList.Where(p => p.IsSystem))
                // ReSharper disable once PossibleNullReferenceException
                systemPaymentList.FirstOrDefault(p => p.Name == item.Name).IsEnable = item.IsEnable;

            var paymentList = new List<Model.PaymentView>(systemPaymentList);
            paymentList.AddRange(store.PaymentList.Where(p => !p.IsSystem));
            storeOrigin.PaymentList = paymentList;

            BLL.StoreBLL.Update(storeOrigin);

            UserLog(user.UserId, EUserLogType.店铺信息, Request, store.StoreId, "店铺收款方式修改");

            return ApiResult.RData(BLL.StoreBLL.QueryListByUserId(user.UserId));
        }

        [HttpPost]
        public IActionResult SupplyList([FromForm] string type)
        {
            UserModel user = UserCurrent();
            var supplyCustom = new List<SupplyModel>();
            var supplySystem = new List<SupplyModel>();
            if (string.IsNullOrWhiteSpace(type))
            {
                supplyCustom = BLL.SupplyBLL.QueryListByUserIdIsShow(user.UserId);
                supplySystem = BLL.SupplyBLL.QueryListByUserIdIsShow(SiteContext.Config.SupplyUserIdSys);
            }
            else if (string.Equals(type, "custom", StringComparison.OrdinalIgnoreCase))
            {
                supplyCustom = BLL.SupplyBLL.QueryListByUserIdIsShow(user.UserId);
            }
            else if (string.Equals(type, "system", StringComparison.OrdinalIgnoreCase))
            {
                supplySystem = BLL.SupplyBLL.QueryListByUserIdIsShow(SiteContext.Config.SupplyUserIdSys);
            }

            return ApiResult.RData(new {supplySystem, supplyCustom});
        }

        [HttpPost]
        public IActionResult SupplyListSave(List<SupplyModel> supplyList)
        {
            UserModel user = UserCurrent();

            var supplyCustom = BLL.SupplyBLL.QueryListByUserIdIsShow(user.UserId);
            foreach (SupplyModel supplyModel in supplyList)
            {
                SupplyModel data = supplyCustom.FirstOrDefault(p => p.SupplyId == supplyModel.SupplyId);
                if (data == null)
                {
                    supplyModel.SupplyId = Global.Generator.DateId(1);
                    supplyModel.IsShow = true;
                    supplyModel.UserId = user.UserId;
                    supplyModel.Category = string.IsNullOrWhiteSpace(supplyModel.Category) ? "" : supplyModel.Category;
                    supplyModel.Memo = string.IsNullOrWhiteSpace(supplyModel.Memo) || supplyModel.Memo.Length > 4000
                        ? ""
                        : supplyModel.Memo;

                    BLL.SupplyBLL.Insert(supplyModel);

                    supplyCustom.Add(supplyModel);
                }
                else
                {
                    data.Name = supplyModel.Name;
                    data.DeliveryType = supplyModel.DeliveryType;
                    data.Category = supplyModel.Category;
                    data.FaceValue = supplyModel.FaceValue;
                    data.Cost = supplyModel.Cost;
                    data.Memo = string.IsNullOrWhiteSpace(supplyModel.Memo) || supplyModel.Memo.Length > 4000
                        ? ""
                        : supplyModel.Memo;
                    data.IsShow = true;
                    data.UserId = user.UserId;

                    BLL.SupplyBLL.Update(data);
                }
            }

            var supplyIds2Remove = supplyCustom.Where(p => supplyList.All(x => x.SupplyId != p.SupplyId))
                .Select(p => p.SupplyId).ToList();
            if (supplyIds2Remove.Count > 0)
            {
                BLL.SupplyBLL.DeleteByIdsAndUserId(supplyIds2Remove, user.UserId);
                foreach (var supplyId in supplyIds2Remove)
                    supplyCustom.Remove(supplyCustom.FirstOrDefault(p => p.SupplyId == supplyId));
            }

            var supplySystem = BLL.SupplyBLL.QueryListByUserIdIsShow(SiteContext.Config.SupplyUserIdSys);

            return ApiResult.RData(new {supplySystem, supplyCustom});
        }

        [HttpPost]
        public IActionResult StockPageList([FromForm] string supplyId, [FromForm] string keyname,
            [FromForm] string isShow,
            [FromForm] int pageIndex, [FromForm] int pageSize)
        {
            UserModel user = UserCurrent();

            if(string.IsNullOrWhiteSpace(supplyId))
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);
            SupplyModel supplyModel = BLL.SupplyBLL.QueryModelById(supplyId);
            if (supplyModel == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

            bool? outIsShow = null;
            if (bool.TryParse(isShow, out var tempIsShow)) outIsShow = tempIsShow;

            var res = BLL.StockBLL.QueryPageListByUserSearch(supplyId, user.UserId, keyname, outIsShow, pageIndex,
                pageSize);

            return ApiResult.RData(new BLL.PageList<StockModel>(res.Rows, res.Total));
        }

        [HttpPost]
        public IActionResult StockMultipleAction([FromForm] string stockIds, [FromForm] int action)
        {
            UserModel user = UserCurrent();

            var stockIdList = Utils.JsonUtility.Deserialize<List<string>>(stockIds);

            if (stockIdList != null && stockIdList.Count > 0)
                switch (action)
                {
                    case 0:
                    {
                        //删除
                        BLL.StockBLL.Delete(p => p.UserId == user.UserId && stockIdList.Contains(p.StockId));
                    }
                        break;
                    case 1:
                    {
                        //下架 isShow = false
                        BLL.StockBLL.Update(p => p.UserId == user.UserId && stockIdList.Contains(p.StockId),
                            p => p.IsShow == false);
                    }
                        break;
                    case 2:
                    {
                        //上架 isShow = true
                        BLL.StockBLL.Update(p => p.UserId == user.UserId && stockIdList.Contains(p.StockId),
                            p => p.IsShow);
                    }
                        break;
                }

            return ApiResult.RCode();
        }

        [HttpPost]
        public IActionResult StockExport([FromForm] string supplyId, [FromForm] string keyname,
            [FromForm] string isShow)
        {
            UserModel user = UserCurrent();

            SupplyModel supplyModel = BLL.SupplyBLL.QueryModelById(supplyId);
            if (supplyModel == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

            bool? outIsShow = null;
            if (bool.TryParse(isShow, out var tempIsShow)) outIsShow = tempIsShow;

            var res = BLL.StockBLL.QueryListByUserSearch(supplyId, user.UserId, keyname, outIsShow);

            if (res.Count > 0)
            {
                var sb = new StringBuilder("--卡密数据--" + Environment.NewLine);
                foreach (StockModel item in res)
                {
                    sb.Append(item.Name);
                    sb.Append(Environment.NewLine);
                }

                return ApiResult.RData(sb.ToString());
            }

            return ApiResult.RCode(ApiResult.ECode.TargetNotExist);
        }


        [HttpPost]
        public IActionResult StockImport([FromForm] string supplyId, [FromForm] string content,
            [FromForm] bool isShow, [FromForm] bool isAllowRepeat)
        {
            UserModel user = UserCurrent();

            SupplyModel supplyModel = BLL.SupplyBLL.QueryModelById(supplyId);
            if (supplyModel == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

            var datas = new List<StockModel>();
            var repeatNameList = new List<string>();

            if (!string.IsNullOrWhiteSpace(content))
            {
                var stockPairList = content.Split(Environment.NewLine);
                if (stockPairList.Length > 0)
                    foreach (var item in stockPairList)
                    {
                        var stockName = Regex.Replace(item.Trim(), @"[\s\u00A0\u0020\u3000]+", " ");
                        if (!string.IsNullOrWhiteSpace(stockName) && stockName != "卡号")
                        {
                            if (!isAllowRepeat && datas.Any(p =>
                                string.Equals(p.Name, stockName, StringComparison.OrdinalIgnoreCase)))
                            {
                                repeatNameList.Add(stockName);
                                continue;
                            }

                            datas.Add(new StockModel
                            {
                                Name = stockName,
                                Memo = "",

                                UserId = user.UserId,
                                SupplyId = supplyId,
                                CreateDate = DateTime.Now,
                                IsShow = isShow,
                                DeliveryDate = DateTime.Now,
                                IsDelivery = false,
                                StockId = Global.Generator.DateId(2)
                            });
                        }
                    }
            }

            if (datas.Count > 0)
            {
                if (!isAllowRepeat)
                {
                    var namelist = datas.Select(p => p.Name).ToList();
                    var stockRepeatList = BLL.StockBLL.QueryList(p =>
                        p.UserId == user.UserId && p.SupplyId == supplyId && namelist.Contains(p.Name));
                    if (stockRepeatList.Count > 0)
                    {
                        repeatNameList.AddRange(stockRepeatList.Select(p => p.Name));
                        datas = datas.Where(p => !stockRepeatList.Select(x => x.Name).ToList().Contains(p.Name))
                            .ToList();
                    }
                }

                BLL.StockBLL.InsertRange(datas);
                return ApiResult.RData(repeatNameList);
            }

            return ApiResult.RCode(ApiResult.ECode.TargetNotExist);
        }

        [HttpPost]
        public IActionResult ProductList([FromForm] string storeId)
        {
            UserModel user = UserCurrent(storeId, out StoreModel store);
            if (store == null)
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);

            var res = BLL.ProductBLL.QueryListByStoreId(store.StoreId);
            return ApiResult.RData(res);
        }

        [HttpPost]
        public IActionResult ProductListSave([FromQuery] string storeId, List<ProductModel> productList)
        {
            UserModel user = UserCurrent(storeId, out StoreModel store);
            if (store == null)
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);

            var productData = BLL.ProductBLL.QueryListByStoreId(store.StoreId);
            foreach (ProductModel productModel in productList)
            {
                ProductModel data = productData.FirstOrDefault(p => p.ProductId == productModel.ProductId);
                if (data == null)
                {
                    var supply = new SupplyModel()
                    {
                        SupplyId = productModel.SupplyId,
                        UserId = user.UserId
                    };
                    if (!string.IsNullOrWhiteSpace(productModel.SupplyId))
                    {
                        supply = BLL.SupplyBLL.QueryModelById(productModel.SupplyId);
                        if (supply == null)
                            return ApiResult.RCode(ApiResult.ECode.DataFormatError);
                    }

                    //ProductId商品编号会在前端生成，这里仅做特殊情况下的处理
                    if (string.IsNullOrWhiteSpace(productModel.ProductId))
                        productModel.ProductId = Global.Generator.DateId(1);
                    productModel.UserId = user.UserId;
                    productModel.StoreId = store.StoreId;
                    productModel.SupplyUserId = supply.UserId;
                    productModel.Category =
                        string.IsNullOrWhiteSpace(productModel.Category) ? "" : productModel.Category;
                    productModel.Memo = string.IsNullOrWhiteSpace(productModel.Memo) || productModel.Memo.Length > 4000
                        ? ""
                        : productModel.Memo;
                    productModel.Icon = string.IsNullOrWhiteSpace(productModel.Icon) ? "" : productModel.Icon;

                    productModel.Icon = SiteContext.Resource.MoveTempFile(productModel.Icon);

                    BLL.ProductBLL.Insert(productModel);

                    productData.Add(productModel);
                }
                else
                {
                    data.UserId = user.UserId;
                    data.Name = productModel.Name;
                    data.Category = productModel.Category;
                    data.Amount = productModel.Amount;
                    data.QuantityMin = productModel.QuantityMin;
                    data.FaceValue = productModel.FaceValue;
                    data.Cost = productModel.Cost;
                    data.IsShow = productModel.IsShow;
                    data.DeliveryType = productModel.DeliveryType;
                    data.Memo = string.IsNullOrWhiteSpace(productModel.Memo) || productModel.Memo.Length > 4000
                        ? ""
                        : productModel.Memo;

                    data.Icon = SiteContext.Resource.MoveTempFile(productModel.Icon);

                    BLL.ProductBLL.Update(data);
                }
            }

            var productIds2Remove = productData.Where(p => productList.All(x => x.ProductId != p.ProductId))
                .Select(p => p.ProductId).ToList();
            if (productIds2Remove.Count > 0)
            {
                BLL.ProductBLL.DeleteByIdsAndUserId(productIds2Remove, store.StoreId);
                foreach (var productId in productIds2Remove)
                    productData.Remove(productData.FirstOrDefault(p => p.ProductId == productId));
            }

            return ApiResult.RData(productData);
        }

        [HttpPost]
        public IActionResult OrderPageList([FromForm] string storeId, [FromForm] string productId,
            [FromForm] string from, [FromForm] string to,
            [FromForm] string keyname, [FromForm] string isPay, [FromForm] string isDelivery,
            [FromForm] string isSettle,
            [FromForm] int pageIndex, [FromForm] int pageSize)
        {
            UserModel user = UserCurrent(storeId, out StoreModel store);
            if (store == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

            bool? outIsPay = null;
            if (bool.TryParse(isPay, out var tempIsPay)) outIsPay = tempIsPay;
            bool? outIsDelivery = null;
            if (bool.TryParse(isDelivery, out var tempIsDelivery)) outIsDelivery = tempIsDelivery;
            bool? outIsSettle = null;
            if (bool.TryParse(isSettle, out var tempIsSettle)) outIsSettle = tempIsSettle;

            DateTime? dateFrom = null;
            if (DateTime.TryParse(from, out DateTime tempdateFrom)) dateFrom = tempdateFrom;

            DateTime? dateTo = null;
            if (DateTime.TryParse(to, out DateTime tempdateTo)) dateTo = tempdateTo.AddDays(1);

            var res = BLL.OrderBLL.QueryPageListBySearch(store.StoreId, productId, dateFrom, dateTo, keyname, outIsPay,
                outIsDelivery, outIsSettle, pageIndex, pageSize);

            return ApiResult.RData(res);
        }

        [HttpPost]
        public IActionResult OrderSave([FromQuery] string storeId, OrderModel order)
        {
            UserModel user = UserCurrent(storeId, out StoreModel store);
            if (store == null)
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);

            
            if (order.IsPay)
            {
                if (string.IsNullOrWhiteSpace(order.PaymentType) ||
                    store.PaymentList.All(p => !string.Equals(p.Name, order.PaymentType)))
                    return ApiResult.RCode(ApiResult.ECode.DataFormatError);

                order.PaymentDate = order.PaymentDate != null ? order.PaymentDate : DateTime.Now;
                order.TranId = !string.IsNullOrWhiteSpace(order.TranId) ? order.TranId : "";
            }

            if (order.RefundAmount > 0) order.RefundDate = order.RefundDate != null ? order.RefundDate : DateTime.Now;

            if (order.IsDelivery)
            {
                order.DeliveryDate = order.DeliveryDate != null ? order.DeliveryDate : DateTime.Now;
                order.StockList = order.StockList != null ? order.StockList : new List<Model.StockOrderView>();
            }

            if (string.IsNullOrWhiteSpace(order.OrderId))
            {
                var product = BLL.ProductBLL.QueryModelById(order.ProductId);
                
                order.OrderId = Global.Generator.DateId(1);
                order.UserId = user.UserId;
                order.StoreId = store.StoreId;

                order.ProductId = product.ProductId;
                order.SupplyId = product.SupplyId;
                order.SupplyUserId = product.SupplyUserId;

                order.ClientIP = Utils.RequestInfo._ClientIP(Request).ToString();
                order.UserAgent = Request.Headers["User-Agent"].ToString();
                order.AcceptLanguage = Request.Headers["Accept-Language"].ToString();

                order.CreateDate = DateTime.Now;
                order.LastUpdateDate = DateTime.Now;

                if (order.IsDelivery)
                {
                    //手动订单新增 手动发货 发送卡密数据到客户邮箱
                    SiteContext.OrderHelper.Email_OrderDelivery(order);
                }

                BLL.OrderBLL.Insert(order);

                return ApiResult.RCode();
            }

            OrderModel data = BLL.BaseBLL<OrderModel>.QueryModelById(order.OrderId);
            if (data != null && data.UserId == user.UserId && data.StoreId == store.StoreId)
            {
                data.Memo = order.Memo;
                if (!data.IsPay && order.IsPay)
                {
                    data.IsPay = order.IsPay;
                    data.PaymentType = order.PaymentType;
                    data.PaymentDate = order.PaymentDate;
                    data.TranId = order.TranId;
                }

                if (!data.IsDelivery && order.IsDelivery)
                {
                    data.IsDelivery = order.IsDelivery;
                    data.DeliveryDate = order.DeliveryDate;
                    data.StockList = order.StockList;

                    //todo 客户订单 手动发货 发送卡密数据到客户邮箱
                    
                }

                if (data.RefundAmount == 0 && order.RefundAmount > 0)
                {
                    data.RefundAmount = order.RefundAmount;
                    data.RefundDate = order.RefundDate;
                }

                BLL.OrderBLL.Update(data);

                return ApiResult.RCode();
            }

            return ApiResult.RCode(ApiResult.ECode.UnKonwError);
        }

        [HttpPost]
        public IActionResult OrderMultipleAction([FromForm] string storeId, [FromForm] string orderIds,
            [FromForm] int action)
        {
            UserModel user = UserCurrent(storeId, out StoreModel store);
            if (store == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

            var orderIdList = Utils.JsonUtility.Deserialize<List<string>>(orderIds);

            if (orderIdList != null && orderIdList.Count > 0)
                switch (action)
                {
                    case 0:
                    {
                        var orderTrashList = Model.OrderTrashModel.Map(
                                BLL.OrderBLL.QueryList(p =>p.IsPay ==false && p.UserId == user.UserId && p.StoreId == store.StoreId && orderIdList.Contains(p.OrderId))
                            );
                        
                        BLL.OrderTrashBLL.InsertRangeAsync(orderTrashList);
                        
                        //删除
                        BLL.OrderBLL.Delete(p => p.IsPay ==false &&
                                                 p.UserId == user.UserId && p.StoreId == store.StoreId && orderIdList.Contains(p.OrderId));

                    }
                        break;
                    case 1:
                    {
                        //结算 isSettle = true
                        //只能结算个人货源订单
                        BLL.OrderBLL.Update(
                            p => p.UserId == user.UserId && p.StoreId == store.StoreId && p.SupplyUserId == p.UserId 
                                 && orderIdList.Contains(p.OrderId) && p.IsPay && p.IsDelivery,
                            p => new OrderModel {IsSettle = true, SettleDate = DateTime.Now});
                    }
                        break;
                }

            return ApiResult.RCode();
        }

        [HttpPost]
        public IActionResult StoreStat([FromForm] string storeId, [FromForm] int statType,
            [FromForm] string from, [FromForm] string to)
        {
            UserModel user = UserCurrent(storeId, out StoreModel store);
            if (store == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

            if (string.IsNullOrWhiteSpace(from) || string.IsNullOrWhiteSpace(to))
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);

            DateTime dateFrom = DateTime.Parse(from);

            DateTime dateTo = DateTime.Parse(to).AddDays(1);

            switch (statType)
            {
                //商品统计
                case 1:
                {
                    var orderList = BLL.OrderBLL.QueryListStat(store.StoreId, dateFrom, dateTo);
                    var productIdGroup = orderList.GroupBy(p => p.ProductId);
                    var orderProductIdList = productIdGroup.Select(p => p.Key).ToList();
                    var productList = BLL.ProductBLL.QueryListByStoreId(store.StoreId)
                        .Where(p => orderProductIdList.Contains(p.ProductId)).ToList();
                    var productStatList = new List<Model.ProductStatView>();
                    foreach (var item in productIdGroup)
                    {
                        var productId = item.Key;
                        var productName = "";
                        var product = productList.FirstOrDefault(p => p.ProductId == productId);
                        if (product != null)
                        {
                            productName = product.Name;
                        }
                        else
                        {
                            productName = "[未知商品]";
                        }

                        var orderProductList = new List<Model.OrderModel>(item);
                        for (int i = 0; i < dateTo.Subtract(dateFrom).TotalDays; i++)
                        {
                            var orderDate = dateFrom.AddDays(i);
                            var orderInDayList = orderProductList
                                .Where(p => p.CreateDate >= orderDate && p.CreateDate <= orderDate.AddDays(1)).ToList();
                            var productStat = new Model.ProductStatView()
                            {
                                Name = productName,
                                Amount = orderInDayList.Sum(p => p.PayAmount),
                                Cost = orderInDayList.Sum(p => p.Cost * p.Quantity),
                                Quantity = orderInDayList.Sum(p => p.Quantity),
                                Count = orderInDayList.Count(),
                                RefundAmount = orderInDayList.Sum(p => p.RefundAmount),
                                CreateDate = orderDate
                            };
                            productStatList.Add(productStat);
                        }
                    }

                    return ApiResult.RData(productStatList);
                }
            }

            return ApiResult.RCode(ApiResult.ECode.DataFormatError);
        }

        [HttpPost]
        public IActionResult OrderNotifyList()
        {
            UserModel user = UserCurrent();

            var data = BLL.OrderBLL.QueryOrderListNotify(user.UserId, SiteContext.Config.OrderNotifyPreDays);
            return ApiResult.RData(data);
        }

        [HttpPost]
        public IActionResult OrderNotifyReset([FromForm] string orderIds, [FromForm] string notifyDate)
        {
            UserModel user = UserCurrent();

            if (string.IsNullOrWhiteSpace(orderIds))
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);
            var orderIdList = Utils.JsonUtility.Deserialize<List<string>>(orderIds);
            if (orderIdList == null || orderIdList.Count == 0)
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);

            DateTime? outNotifyDate = null;
            if (DateTime.TryParse(notifyDate, out DateTime tempnotifyDate)) outNotifyDate = tempnotifyDate;

            var res = BLL.OrderBLL.Update(p => orderIdList.Contains(p.OrderId),
                p => new OrderModel {NotifyDate = outNotifyDate});

            if (res)
            {
                var data = BLL.OrderBLL.QueryOrderListNotify(user.UserId, SiteContext.Config.OrderNotifyPreDays);
                return ApiResult.RData(data);
            }

            return ApiResult.RCode(ApiResult.ECode.UnKonwError);
        }

        [HttpPost]
        public IActionResult BillPageList([FromForm] string from, [FromForm] string to,
            [FromForm] int billType,
            [FromForm] int pageIndex, [FromForm] int pageSize)
        {
            UserModel user = UserCurrent();

            DateTime? dateFrom = null;
            if (DateTime.TryParse(from, out DateTime tempdateFrom)) dateFrom = tempdateFrom;

            DateTime? dateTo = null;
            if (DateTime.TryParse(to, out DateTime tempdateTo)) dateTo = tempdateTo.AddDays(1);

            if (dateFrom == null || dateTo == null)
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);

            var data = BLL.BillBLL.QueryPageListBySearch(pageIndex, pageSize, user.UserId, billType, (DateTime) dateFrom,
                (DateTime) dateTo);

            return ApiResult.RData(data);
        }

        [HttpPost]
        public IActionResult WithDrawList()
        {

            UserModel user = UserCurrent();

            var data1 = BLL.WithDrawBLL.QueryList(p => p.UserId == user.UserId && !p.IsFinish);
            var data2 = BLL.WithDrawBLL.QueryList(3, p => p.UserId == user.UserId && p.IsFinish && p.AmountFinish > 0);
            var data3 = BLL.WithDrawBLL.QueryList(3, p => p.UserId == user.UserId && p.IsFinish && p.AmountFinish == 0);

            var data = new List<Model.WithDrawModel>();
            data.AddRange(data1);
            data.AddRange(data2);
            data.AddRange(data3);

            return ApiResult.RData(data);
        }

//         public IActionResult Register([FromForm] string account, [FromForm] string password, [FromForm] string qq,
//             [FromForm] string email, [FromForm] string telphone)
//         {
//             if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password) ||
//                 string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(telphone))
//                 return ApiResult.RCode("传参错误");
//
//             if (BLL.UserBLL.QueryModelByAccount(account) != null)
//                 return ApiResult.RCode("商户账号已存在");
//             if (BLL.UserExtendBLL.QueryModelByEmail(email) != null)
//                 return ApiResult.RCode("保密邮箱已存在");
//             if (BLL.UserExtendBLL.QueryModelByTelPhone(telphone) != null)
//                 return ApiResult.RCode("手机号已存在");
//             var salt = Global.Generator.Guid().Substring(0, 6); //6位有效字符
//             BLL.UserBLL.Insert(new UserModel
//             {
//                 Account = account,
//                 ClientKey = string.Empty,
//                 Password = Global.Hash(password, salt),
//                 Salt = salt
//             });
//             UserModel user = BLL.UserBLL.QueryModelByAccount(account);
//             if (user == null)
//                 return ApiResult.RCode("注册失败");
//             var ip = RequestInfo._ClientIP(HttpContext).ToString();
//             var useragent = Request.Headers["User-Agent"].ToString();
//             var acceptlanguage = Request.Headers["Accept-Language"].ToString();
//             BLL.UserExtendBLL.Insert(new UserExtendModel
//             {
//                 BankAccount = string.Empty,
//                 BankPersonName = string.Empty,
//                 BankType = EBankType.工商银行,
//                 Email = email,
//                 IdCard = string.Empty,
//                 Name = string.Empty,
//                 QQ = qq,
//                 TelPhone = telphone,
//                 RegisterIP = ip,
//                 RegisterDate = DateTime.Now,
//                 UserAgent = useragent,
//                 AcceptLanguage = acceptlanguage,
//                 UserId = user.UserId
//             });
//
// // var StoreId = Global.Generator.DateId(1);
// //
// // BLL.(BLL.StoreBLL.)Insert(new Model.Store
// // {
// //     Email = Email,
// //     Name = string.Empty,
// //     QQ = QQ,
// //     TelPhone = Telphone,
// //     UserId = user.UserId,
// //     DomainSub = string.Empty,
// //     DomainTop = string.Empty,
// //     Level = EStoreLevel.无,
// //     Memo = string.Empty,
// //     Template = EStoreTemplate.模板一,
// //     StoreId = StoreId,
// //     Amount = 0,
// //     PaymentJson = "[]",
// //     UniqueId = StoreId
// // });
//             UserLog(user.UserId, EUserLogType.注册, Request);
//             return ApiResult.RCode("");
//         }



    }
}