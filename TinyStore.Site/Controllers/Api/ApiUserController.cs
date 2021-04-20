using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TinyStore.Model.Extend;

namespace TinyStore.Site.Controllers
{
    [ApiController]
    [MultipleSubmit]
    [UserHeaderToken("Login", "Register")]
    [Produces("application/json")]
    [Route("ApiUser/[action]")]
    public class ApiUserController : ControllerBase
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
                ClientIP = Utils.RequestInfo._ClientIP(request).ToString(),
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
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);
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
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);
            }
        }

        [HttpPost]
        public IActionResult UserExtendSave(Model.UserExtendModel userExtend)
        {
            var user = UserCurrent();

            if (!string.IsNullOrEmpty(userExtend.IdCard) && !Utils.IDCard.IsIDCard(userExtend.IdCard))
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);

            if (!string.IsNullOrEmpty(userExtend.Email) && !userExtend.Email.Contains("@"))
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);


            var userExtendOrigin = BLL.UserExtendBLL.QueryModelByUserId(user.UserId);

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
            UserLog(userExtendOrigin.UserId, EUserLogType.修改商户信息, Request, "", "个人信息修改");

            return ApiResult.RData(userExtendOrigin);
        }


        [HttpPost]
        public IActionResult UserPasswordModify([FromForm] string passwordOld, [FromForm] string passwordNew)
        {
            var user = UserCurrent();
            if (!string.Equals(Global.Hash(passwordOld, user.Salt), user.Password,
                StringComparison.OrdinalIgnoreCase))
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);

            user.Password = Global.Hash(passwordNew, user.Salt);
            BLL.UserBLL.Update(user);
            UserLog(user.UserId, EUserLogType.修改商户信息, Request, "", "密码修改");
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
                {
                    using (var stream = Request.Form.Files[i].OpenReadStream())
                    {
                        byte[] buffer = new byte[Request.Form.Files[i].Length];
                        await stream.ReadAsync(buffer, 0, buffer.Length);
                        files.Add(Request.Form.Files[i].ContentType,
                            buffer); //  {name: {key:contenttype,value:byte[]},}
                    }
                }

                if (string.IsNullOrWhiteSpace(id))
                {
                    id = Global.Generator.DateId(2);
                }

                var res = SiteContext.Resource.UploadFiles(model, id, name, files);
                return new JsonResult(res);
            }
            catch
            {
                return ApiResult.RCode(ApiResult.ECode.UnKonwError);
            }
        }

        [HttpPost]
        public IActionResult StoreSave(Model.StoreModel store)
        {
            if (store == null)
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);
            var user = UserCurrent(store.StoreId, out Model.StoreModel storeOrigin);
            if (storeOrigin == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

            // if (string.Equals("admin", store.UniqueId, StringComparison.OrdinalIgnoreCase) ||
            //     string.Equals("user", store.UniqueId, StringComparison.OrdinalIgnoreCase))
            //     return ApiResult.RCode(store.UniqueId + "是系统关键词，不能使用");

            if (store.Name.Length >= 10)
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);

            if (store.UniqueId.Length <= 5)
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);

            if (!string.IsNullOrWhiteSpace(store.UniqueId))
            {
                var compare = BLL.StoreBLL.QueryModelByUniqueId(store.UniqueId);
                if (compare != null && !string.Equals(compare.StoreId, store.StoreId,
                    StringComparison.OrdinalIgnoreCase))
                    return ApiResult.RCode(ApiResult.ECode.TargetExist);
            }

            storeOrigin.Logo = SiteContext.Resource.MoveTempFile(store.Logo);

            storeOrigin.Name = store.Name;
            storeOrigin.Initial = Global.Initial(store.Name);
            storeOrigin.UniqueId = store.UniqueId;
            storeOrigin.Template = store.Template;
            storeOrigin.Memo =  string.IsNullOrWhiteSpace(store.Memo) || store.Memo.Length > 4000 ? "" : store.Memo;
            storeOrigin.Email = store.Email;
            storeOrigin.TelPhone = store.TelPhone;
            storeOrigin.QQ = store.QQ;

            BLL.StoreBLL.Update(storeOrigin);

            UserLog(user.UserId, EUserLogType.修改店铺信息, Request, store.StoreId, "店铺信息修改");

            return ApiResult.RData(BLL.StoreBLL.QueryListByUserId(user.UserId));
        }


        [HttpPost]
        public IActionResult StorePaymentListSave(Model.StoreModel store)
        {
            if (store == null)
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);
            var user = UserCurrent(store.StoreId, out Model.StoreModel storeOrigin);
            if (storeOrigin == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

            var systemPaymentList = SiteContext.SystemPaymentList();
            foreach (var item in store.PaymentList.Where(p => p.IsSystem))
            {
                // ReSharper disable once PossibleNullReferenceException
                systemPaymentList.FirstOrDefault(p => p.Name == item.Name).IsEnable = item.IsEnable;
            }

            var paymentList = new List<Model.Extend.Payment>(systemPaymentList);
            paymentList.AddRange(store.PaymentList.Where(p => !p.IsSystem));
            storeOrigin.PaymentList = paymentList;

            BLL.StoreBLL.Update(storeOrigin);

            UserLog(user.UserId, EUserLogType.修改店铺信息, Request, store.StoreId, "店铺收款方式修改");

            return ApiResult.RData(BLL.StoreBLL.QueryListByUserId(user.UserId));
        }

        [HttpPost]
        public IActionResult SupplyList([FromForm] string type)
        {
            var user = UserCurrent();
            var supplyCustom = new List<Model.SupplyModel>();
            var supplySystem = new List<Model.SupplyModel>();
            if (string.IsNullOrWhiteSpace(type))
            {
                supplyCustom = BLL.SupplyBLL.QueryListByUserId(user.UserId);
                supplySystem = BLL.SupplyBLL.QueryListByUserId(SiteContext.Config.SupplyUserIdSys);
            }
            else if (string.Equals(type, "custom", StringComparison.OrdinalIgnoreCase))
            {
                supplyCustom = BLL.SupplyBLL.QueryListByUserId(user.UserId);
            }
            else if (string.Equals(type, "system", StringComparison.OrdinalIgnoreCase))
            {
                supplySystem = BLL.SupplyBLL.QueryListByUserId(SiteContext.Config.SupplyUserIdSys);
            }

            return ApiResult.RData(new {supplySystem, supplyCustom});
        }

        [HttpPost]
        public IActionResult SupplyListSave(List<Model.SupplyModel> supplyList)
        {
            var user = UserCurrent();

            var supplyCustom = BLL.SupplyBLL.QueryListByUserId(user.UserId);
            foreach (var supplyModel in supplyList)
            {
                var data = supplyCustom.FirstOrDefault(p => p.SupplyId == supplyModel.SupplyId);
                if (data == null)
                {
                    supplyModel.SupplyId = Global.Generator.DateId(1);
                    supplyModel.IsShow = true;
                    supplyModel.UserId = user.UserId;
                    supplyModel.Category = string.IsNullOrWhiteSpace(supplyModel.Category) ? "" : supplyModel.Category;
                    supplyModel.Memo = string.IsNullOrWhiteSpace(supplyModel.Memo) || supplyModel.Memo.Length > 4000 ? "" : supplyModel.Memo;

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
                    data.Memo = string.IsNullOrWhiteSpace(supplyModel.Memo) || supplyModel.Memo.Length > 4000 ? "" : supplyModel.Memo;
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
                {
                    supplyCustom.Remove(supplyCustom.FirstOrDefault(p => p.SupplyId == supplyId));
                }
            }

            var supplySystem = BLL.SupplyBLL.QueryListByUserId(SiteContext.Config.SupplyUserIdSys);

            return ApiResult.RData(new {supplySystem, supplyCustom});
        }

        [HttpPost]
        public IActionResult StockPageList([FromForm] string supplyId, [FromForm] string keyname,
            [FromForm] string isShow,
            [FromForm] int pageIndex, [FromForm] int pageSize)
        {
            var user = UserCurrent();

            var supplyModel = BLL.SupplyBLL.QueryModelById(supplyId);
            if (supplyModel == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

            bool? outIsShow = null;
            if (bool.TryParse(isShow, out bool tempIsShow))
            {
                outIsShow = tempIsShow;
            }

            var res = BLL.StockBLL.QueryPageListByUserSearch(supplyId, user.UserId, keyname, outIsShow, pageIndex,
                pageSize);

            return ApiResult.RData(new GridData<Model.StockModel>(res.Rows, res.Total));
        }

        [HttpPost]
        public IActionResult StockMultipleAction([FromForm] string stockIds, [FromForm] int action)
        {
            var user = UserCurrent();

            var stockIdList = Global.Json.Deserialize<List<string>>(stockIds);

            if (stockIdList != null && stockIdList.Count > 0)
            {
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
                            p => p.IsShow == true);
                    }
                        break;
                }
            }

            return ApiResult.RCode();
        }

        [HttpPost]
        public IActionResult StockExport([FromForm] string supplyId, [FromForm] string keyname,
            [FromForm] string isShow)
        {
            var user = UserCurrent();

            var supplyModel = BLL.SupplyBLL.QueryModelById(supplyId);
            if (supplyModel == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

            bool? outIsShow = null;
            if (bool.TryParse(isShow, out bool tempIsShow))
            {
                outIsShow = tempIsShow;
            }

            var res = BLL.StockBLL.QueryListByUserSearch(supplyId, user.UserId, keyname, outIsShow);

            if (res.Count > 0)
            {
                StringBuilder sb = new StringBuilder("卡号" + Environment.NewLine);
                foreach (var item in res)
                {
                    sb.Append(item.Name);
                    sb.Append(Environment.NewLine);
                }

                return ApiResult.RData(sb.ToString());
            }
            else
            {
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);
            }
        }


        [HttpPost]
        public IActionResult StockImport([FromForm] string supplyId, [FromForm] string content,
            [FromForm] bool isShow, [FromForm] bool isAllowRepeat)
        {
            var user = UserCurrent();

            var supplyModel = BLL.SupplyBLL.QueryModelById(supplyId);
            if (supplyModel == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

            var datas = new List<Model.StockModel>();
            var repeatNameList = new List<string>();

            if (!string.IsNullOrWhiteSpace(content))
            {
                var stockPairList = content.Split(Environment.NewLine);
                if (stockPairList.Length > 0)
                {
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

                            datas.Add(new Model.StockModel()
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
            else
            {
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);
            }
        }

        [HttpPost]
        public IActionResult ProductList([FromForm] string storeId)
        {
            var user = UserCurrent(storeId, out Model.StoreModel store);
            if (store == null)
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);

            var res = BLL.ProductBLL.QueryListByStoreId(store.StoreId);
            return ApiResult.RData(res);
        }
        
        [HttpPost]
        public IActionResult ProductListSave([FromQuery] string storeId,List<Model.ProductModel> productList)
        {
            var user = UserCurrent(storeId, out Model.StoreModel store);
            if (store == null)
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);

            var productData = BLL.ProductBLL.QueryListByStoreId(store.StoreId);
            foreach (var productModel in productList)
            {
                var data = productData.FirstOrDefault(p => p.ProductId == productModel.ProductId);
                if (data == null)
                {
                    //ProductId商品编号会在前端生成，这里仅做特殊情况下的处理
                    if(string.IsNullOrWhiteSpace(productModel.ProductId))
                        productModel.ProductId = Global.Generator.DateId(1);
                    productModel.UserId = user.UserId;
                    productModel.StoreId = store.StoreId;
                    productModel.Category = string.IsNullOrWhiteSpace(productModel.Category) ? "" : productModel.Category;
                    productModel.Memo = string.IsNullOrWhiteSpace(productModel.Memo) || productModel.Memo.Length > 4000 ? "" : productModel.Memo;
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
                    data.Memo = string.IsNullOrWhiteSpace(productModel.Memo) || productModel.Memo.Length > 4000 ? "" : productModel.Memo;
                    
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
                {
                    productData.Remove(productData.FirstOrDefault(p => p.ProductId == productId));
                }
            }

            return ApiResult.RData(productData);
        }
        
        [HttpPost]
        public IActionResult OrderPageList([FromForm] string storeId,[FromForm] string productId, 
            [FromForm] string from, [FromForm] string to,
            [FromForm] string keyname,[FromForm] string isPay, [FromForm] string isDelivery,
            [FromForm] int pageIndex, [FromForm] int pageSize)
        {
            var user = UserCurrent(storeId, out Model.StoreModel store);
            if (store == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);
            
            bool? outIsPay = null;
            if (bool.TryParse(isPay, out bool tempIsPay))
            {
                outIsPay = tempIsPay;
            }  
            
            bool? outIsDelivery = null;
            if (bool.TryParse(isDelivery, out bool tempIsDelivery))
            {
                outIsDelivery = tempIsDelivery;
            }

            DateTime? dateFrom = null;
            if (DateTime.TryParse(from, out DateTime tempdateFrom))
            {
                dateFrom = tempdateFrom;
            }  
            
            DateTime? dateTo = null;
            if (DateTime.TryParse(to, out DateTime tempdateTo ))
            {
                dateTo = tempdateTo.AddDays(1) ;
            }
            
            var res = BLL.OrderBLL.QueryPageListBySearch(store.StoreId, productId,dateFrom,dateTo, keyname,outIsPay, outIsDelivery, pageIndex, pageSize);
            
            return ApiResult.RData(res);
        }
        
        
        public IActionResult Register([FromForm] string account, [FromForm] string password, [FromForm] string qq,
            [FromForm] string email, [FromForm] string telphone)
        {
            if (string.IsNullOrWhiteSpace(account) || string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(telphone))
            {
                return ApiResult.RCode("传参错误");
            }

            if (BLL.UserBLL.QueryModelByAccount(account) != null)
                return ApiResult.RCode("商户账号已存在");
            if (BLL.UserExtendBLL.QueryModelByEmail(email) != null)
                return ApiResult.RCode("保密邮箱已存在");
            if (BLL.UserExtendBLL.QueryModelByTelPhone(telphone) != null)
                return ApiResult.RCode("手机号已存在");
            var salt = Global.Generator.Guid().Substring(0, 6); //6位有效字符
            BLL.UserBLL.Insert(new Model.UserModel
            {
                Account = account,
                ClientKey = string.Empty,
                Password = Global.Hash(password, salt),
                Salt = salt
            });
            var user = BLL.UserBLL.QueryModelByAccount(account);
            if (user == null)
                return ApiResult.RCode("注册失败");
            var ip = Utils.RequestInfo._ClientIP(HttpContext).ToString();
            var useragent = Request.Headers["User-Agent"].ToString();
            var acceptlanguage = Request.Headers["Accept-Language"].ToString();
            BLL.UserExtendBLL.Insert(new Model.UserExtendModel
            {
                BankAccount = string.Empty,
                BankPersonName = string.Empty,
                BankType = EBankType.工商银行,
                Email = email,
                IdCard = string.Empty,
                Name = string.Empty,
                QQ = qq,
                TelPhone = telphone,
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

            foreach (var item in orders.Where(p => p.IsPay))
            {
                //实际收款(退款金额)
                incomme += item.Amount;
                returnamount += item.ReturnAmount;
                //合计成本(待结算)
                cost += item.Cost + item.PaymentFee;
                if (!item.IsSettle)
                    costnoclose += item.Cost;
                //手续费(退款返还)
                paymentfee += item.PaymentFee;
                //毛利润（总收入-总支出） //纯利润（毛利润-总手续费）
                profit += item.Amount - item.ReturnAmount - item.Cost - item.PaymentFee;
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

        public IActionResult OrderPageListIsPaid([FromForm] int pageIndex, [FromForm] int pageSize,
            [FromForm] string storeId)
        {
            var user = UserCurrent(storeId, out Model.StoreModel store);
            if (store == null)
                return ApiResult.RCode("未知错误");

            var res = BLL.OrderBLL.QueryPageListIsPaidByStoreId(store.StoreId, pageIndex, pageSize);
            return ApiResult.RData(new GridData<Model.OrderModel>(res.Rows, (int) res.Total));
        }

        public IActionResult OrderPageListBySid([FromForm] string storeId, [FromForm] string sId, [FromForm] int state,
            [FromForm] int keykind, [FromForm] string key, [FromForm] bool ishasreturn, [FromForm] int pageIndex,
            [FromForm] int pageSize)
        {
            var user = UserCurrent(storeId, out Model.StoreModel store);
            if (store == null)
                return ApiResult.RCode("未知错误");

            var orderstat =
                OrderListStat(BLL.OrderBLL.QueryListBySid(sId, state, keykind, key, store.StoreId, ishasreturn));

            var res = BLL.OrderBLL.QueryPageListBySid(sId, state, keykind, key, store.StoreId, ishasreturn,
                pageIndex, pageSize);

            return ApiResult.RData(new GridData<Model.OrderModel>(res.Rows, (int) res.Total, orderstat));
        }

        public IActionResult OrderInsert([FromForm] string storeId, [FromForm] string order)
        {
            var user = UserCurrent(storeId, out Model.StoreModel store);
            if (store == null)
                return ApiResult.RCode("未知错误");
            if (!string.IsNullOrWhiteSpace(order))
                return ApiResult.RCode("传参错误");
            var orderMpdel = Global.Json.Deserialize<Model.OrderModel>(order);
            if (orderMpdel == null)
                return ApiResult.RCode("传参错误");

            var product = BLL.ProductBLL.QueryModelByProductIdAndStoreId(orderMpdel.ProductId, store.StoreId);
            if (product == null)
                return ApiResult.RCode("产品不存在");
            if (product.DeliveryType == EDeliveryType.卡密)
            {
                var stockcount =
                    BLL.StockBLL.QueryCountBySupplyIdCanUse(product.SupplyId);
                if (stockcount < product.QuantityMin)
                    return ApiResult.RCode("库存数量不足");
                if (orderMpdel.Quantity > stockcount)
                    return ApiResult.RCode("下单数量不能大于库存数量");
                if (orderMpdel.Quantity < product.QuantityMin)
                    return ApiResult.RCode("下单数量不能小于最小下单量");
            }
            else if (orderMpdel.Quantity < product.QuantityMin)
                return ApiResult.RCode("下单数量不能小于最小下单量");

            if (string.IsNullOrEmpty(orderMpdel.Contact))
                return ApiResult.RCode("电子邮箱不能为空");
            
            // if (string.IsNullOrEmpty(orderMpdel.Message))
            //     return ApiResult.RCode("留言信息不能为空");

            Model.Extend.Payment payment = store.PaymentList.FirstOrDefault(p =>
                p.IsEnable && string.Equals(p.Name, orderMpdel.PaymentType, StringComparison.OrdinalIgnoreCase));

            if (payment == null)
                return ApiResult.RCode("支付方式不存在");
            if (!string.IsNullOrEmpty(orderMpdel.TranId) &&
                BLL.OrderBLL.QueryModelByTranId(orderMpdel.TranId) != null)
                return ApiResult.RCode("交易编号已存在");
            //var price = product.Amount;
            double discount = 0;
            var orderfee = orderMpdel.Contact.Contains("@") ? 0 : 0;
            // var supplier = string.IsNullOrEmpty(product.SupplierId)
            //     ? null
            //     : BLL.SupplierBLL.QueryModelBySId(product.SupplierId);


            var ip = Utils.RequestInfo._ClientIP(HttpContext).ToString();
            var useragent = Request.Headers["User-Agent"].ToString();
            var acceptlanguage = Request.Headers["Accept-Language"].ToString();

            var data = new Model.OrderModel
            {
                OrderId = Global.Generator.DateId(2),
                UserId = user.UserId,
                StoreId = store.StoreId,
                SupplyId = product.SupplyId,
                ProductId = product.ProductId,
                Name = product.Name,
                Memo = product.Memo,
                
                Quantity = orderMpdel.Quantity,
                Amount = orderMpdel.Amount,
                Cost = product.Cost * orderMpdel.Quantity,
                CreateDate = orderMpdel.CreateDate,
                //Discount = discount,
                //Income = string.IsNullOrEmpty(orderMpdel.TranId) ? 0 : orderMpdel.Amount,
                
                IsPay = orderMpdel.IsPay,
                IsDelivery = orderMpdel.IsPay && orderMpdel.IsDelivery,
                
                PaymentFee = orderMpdel.Amount * payment.Rate,
                PaymentType = payment.Name,
                
                DeliveryDate = DateTime.Now,
                StockList = new List<StockOrder>(),
                TranId = orderMpdel.IsPay ? orderMpdel.TranId : string.Empty,
                
                
                
                Contact = orderMpdel.Contact,
                Message = orderMpdel.Message,
                ClientIP = ip,
                UserAgent = useragent,
                AcceptLanguage = acceptlanguage,
                 
                IsSettle = false,
                SettleDate = DateTime.Now,
                
                ReturnAmount = 0,
                ReturnDate = DateTime.Now,
                
                LastUpdateDate = DateTime.Now,
            };

            BLL.OrderBLL.Insert(data);

            UserLog(store.UserId, EUserLogType.订单管理, Request, store.StoreId,
                "订单手动添加");

            return ApiResult.RCode("");
        }

        public IActionResult OrderDelivery([FromForm] string storeId, [FromForm] string orderId) // 保存了两次订单
        {
            var user = UserCurrent(storeId, out Model.StoreModel store);
            if (store == null)
                return ApiResult.RCode("未知错误");

            if (string.IsNullOrEmpty(orderId))
                return ApiResult.RCode("订单不存在");
            var order = BLL.OrderBLL.QueryModelByOrderIdAndStoreId(orderId, store.StoreId);
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

        public IActionResult OrderEmail([FromForm] string storeId, [FromForm] string orderId)
        {
            var user = UserCurrent(storeId, out Model.StoreModel store);
            if (store == null)
                return ApiResult.RCode("未知错误");

            if (string.IsNullOrEmpty(orderId))
                return ApiResult.RCode("订单ID不能为空");
            var order = BLL.OrderBLL.QueryModelByOrderIdAndStoreId(orderId, store.StoreId);
            if (order == null)
                return ApiResult.RCode("订单不存在");
            if (!order.IsDelivery)
                return ApiResult.RCode("订单尚未发货，不能手动发邮件");

            SiteContext.OrderHelper.DeliveryEmail(order);

            UserLog(store.UserId, EUserLogType.订单管理, Request, store.StoreId, "订单邮件发货");

            return ApiResult.RCode("");
        }

        public IActionResult OrderReturn([FromForm] string storeId, [FromForm] string orderId,
            [FromForm] double costUpdate, [FromForm] double returnAmount)
        {
            var user = UserCurrent(storeId, out Model.StoreModel store);
            if (store == null)
                return ApiResult.RCode("未知错误");
            if (string.IsNullOrEmpty(orderId))
                return ApiResult.RCode("订单ID不能为空");
            var order = BLL.OrderBLL.QueryModelByOrderIdAndStoreId(orderId, store.StoreId);
            if (order == null)
                return ApiResult.RCode("订单不存在");
            if (order.Amount < returnAmount)
                return ApiResult.RCode("退款金额应不大于到账金额");
            if (!order.IsPay)
                return ApiResult.RCode("订单尚未付款，不能操作");

            var amountchange = returnAmount - order.ReturnAmount;
            order.Cost = costUpdate;
            order.ReturnAmount = returnAmount;

            order.ReturnDate = DateTime.Now;
            order.LastUpdateDate = DateTime.Now;
            if (!order.IsDelivery && string.Equals(order.Amount.ToString("f2"), order.ReturnAmount.ToString("f2"),
                StringComparison.OrdinalIgnoreCase))
            {
                order.IsDelivery = true;
                order.DeliveryDate = DateTime.Now;
            }

            var userExtend = BLL.UserExtendBLL.QueryModelById(order.UserId);
            if (userExtend == null)
                return ApiResult.RCode("商户不存在，请检查数据库");
            if (userExtend.Amount < amountchange)
                return ApiResult.RCode("商户资金小于退款金额，请联系店铺管理人员解决");
            BLL.UserExtendBLL.ChangeAmount(userExtend.UserId, -amountchange); //店铺金额减去变动金额

            BLL.OrderBLL.Update(order);
            UserLog(store.UserId, EUserLogType.订单管理, Request, store.StoreId, "订单退款申请");
            return ApiResult.RCode("");
        }

        public IActionResult OrderUpdateIsSettle([FromForm] string storeId, [FromForm] string orderIds)
        {
            var user = UserCurrent(storeId, out Model.StoreModel store);
            if (store == null)
                return ApiResult.RCode("未知错误");
            if (string.IsNullOrWhiteSpace(orderIds))
                return ApiResult.RCode("订单编号错误");
            var ids = Global.Json.Deserialize<List<string>>(orderIds);
            if (ids == null || ids.Count < 0)
                return ApiResult.RCode("订单编号错误");

            BLL.OrderBLL.UpdateIsSettle(ids, store.StoreId, true, DateTime.Now);
            UserLog(store.UserId, EUserLogType.订单管理, Request, store.StoreId, "批量结算");
            return ApiResult.RCode("");
        }

        public IActionResult UserLogPageList([FromForm] string storeId, [FromForm] int pageIndex,
            [FromForm] int pagesize, [FromForm] int userLogType, [FromForm] DateTime begin, [FromForm] DateTime end)
        {
            var user = UserCurrent(storeId, out Model.StoreModel store);
            if (store == null)
                return ApiResult.RCode("未知错误");

            begin = begin.Date;
            end = end.Date.AddDays(1).AddSeconds(-1);
            var res = BLL.UserLogBLL.QueryPageListByUserIdOrStoreIdOrType(user.UserId, store.StoreId, userLogType,
                begin,
                end, pageIndex, pagesize);

            return ApiResult.RData(new GridData<Model.UserLogModel>(res.Rows, (int) res.Total));
        }


// public IActionResult SupplierList([FromForm] string StoreId)
// {
//     var user = User(StoreId, out Model.Store store);
//     if (store == null)
//         return ApiResult.RCode("未知错误");
//
//     return new JsonResult(Api.UserApi.Supplier.GetAllList(store));
// }

//todo  StoreId=>UserId 前端需要配合修改


        public IActionResult ProductAddFromSupply([FromForm] string storeId, [FromForm] string ids,
            [FromForm] string category)
        {
            var user = UserCurrent(storeId, out Model.StoreModel store);
            if (store == null)
                return ApiResult.RCode("未知错误");
            if (string.IsNullOrWhiteSpace(ids))
                return ApiResult.RCode("商品编号错误");
            var idList = Global.Json.Deserialize<List<string>>(ids);
            if (idList == null)
                return ApiResult.RCode("商品编号错误");
            if (string.IsNullOrEmpty(category))
                return ApiResult.RCode("产品分类不存在");

            var userExtend = BLL.UserExtendBLL.QueryModelById(user.UserId);
            if (userExtend == null)
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);

            var productlist = new List<Model.ProductModel>();
            foreach (var item in BLL.SupplyBLL.QueryListByIds(idList))
            {
                productlist.Add(new Model.ProductModel
                {
                    Amount = item.FaceValue,
                    Category = category,
                    Cost = (item.Cost * SiteContext.Config.SupplyRates[userExtend.Level]),
                    IsShow = false,
                    Memo = item.Name,
                    Name = item.Name,
                    ProductId = Global.Generator.DateId(1),
                    QuantityMin = 1,
                    Sort = 0,
                    DeliveryType = item.DeliveryType,
                    StoreId = storeId,
                    SupplyId = item.SupplyId,
                });
            }

            BLL.ProductBLL.InsertRange(productlist);
            return ApiResult.RCode("");
        }

        public IActionResult ProductSave([FromForm] string storeId, [FromForm] string product)
        {
            var user = UserCurrent(storeId, out Model.StoreModel store);
            if (store == null)
                return ApiResult.RCode("未知错误");


            if (string.IsNullOrWhiteSpace(product))
                return ApiResult.RCode("商品信息为空");
            var productModel = Global.Json.Deserialize<Model.ProductModel>(product);
            if (productModel == null)
                return ApiResult.RCode("商品信息数据格式错误");
            if (string.IsNullOrEmpty(productModel.Name))
                return ApiResult.RCode("产品名称不能为空");

            if (productModel.Amount <= 0)
                return ApiResult.RCode("产品单价应为正数");

            if (productModel.QuantityMin < 1)
                return ApiResult.RCode("最小购买量必须大于0");


            if (string.IsNullOrEmpty(productModel.ProductId))
            {
                //product.StoreId = store.StoreId;
                //product.ProductId = Global.Generator.DateId(2);
                if (!string.IsNullOrEmpty(productModel.Icon) && productModel.Icon.StartsWith("/Resouce/Product/"))
                {
                    var str = productModel.Icon.Replace("/Resouce/Product/", ""); //取图片地址的id值
                    if (str.Contains("/"))
                    {
                        productModel.ProductId = str.Split("/")[0];
                    }
                }

                if (string.IsNullOrWhiteSpace(productModel.ProductId))
                {
                    productModel.ProductId = Global.Generator.DateId(2);
                }

                productModel.Icon = SiteContext.Resource.MoveTempFile(productModel.Icon);
                BLL.ProductBLL.Insert(productModel);
                UserLog(user.UserId, EUserLogType.产品管理, Request, store.StoreId,
                    "产品增加" + productModel.ProductId);
                return ApiResult.RCode("");
            }
            else
            {
                var datacompare =
                    BLL.ProductBLL.QueryModelByProductIdAndStoreId(productModel.ProductId, store.StoreId);
                if (datacompare == null)
                    return ApiResult.RCode("产品不存在或已被删除");
                productModel.Icon = SiteContext.Resource.MoveTempFile(productModel.Icon);
                BLL.ProductBLL.Update(productModel);
                UserLog(user.UserId, EUserLogType.产品管理, Request, store.StoreId,
                    "产品修改" + productModel.ProductId);
                return ApiResult.RCode("");
            }
        }

        public IActionResult ProductDelete([FromForm] string storeId, [FromForm] string productId)
        {
            var user = UserCurrent(storeId, out Model.StoreModel store);
            if (store == null)
                return ApiResult.RCode("未知错误");

            if (string.IsNullOrEmpty(productId))
                return ApiResult.RCode("产品编号错误");
            var product = BLL.ProductBLL.QueryModelByProductIdAndStoreId(productId, store.StoreId);
            if (product == null)
                return ApiResult.RCode("产品不存在或已被删除");

            BLL.ProductBLL.DeleteByProductIdAndStoreId(productId, store.StoreId);
            UserLog(store.UserId, EUserLogType.产品管理, Request, store.StoreId, "产品删除");
            return ApiResult.RCode("");
        }

        public IActionResult ProductModifyIsShow([FromForm] string storeId, [FromForm] string productId,
            [FromForm] bool isShow)
        {
            var user = UserCurrent(storeId, out Model.StoreModel store);
            if (store == null)
                return ApiResult.RCode("未知错误");


            if (string.IsNullOrEmpty(productId))
                return ApiResult.RCode("产品ID不能为空");

            var product = BLL.ProductBLL.QueryModelByProductIdAndStoreId(productId, store.StoreId);
            if (product == null)
                return ApiResult.RCode("产品不存在或已被删除");

            if (product.IsShow != isShow)
            {
                product.IsShow = isShow;
                BLL.ProductBLL.Update(product);
                UserLog(store.UserId, EUserLogType.产品管理, Request,
                    store.StoreId, "产品" + (isShow ? "上架" : "下架"));
            }

            return ApiResult.RCode("");
        }


        public IActionResult OrderPageListBenifit([FromForm] string storeId, [FromForm] string sId,
            [FromForm] bool isHasReturn, [FromForm] DateTime begin, [FromForm] DateTime end, [FromForm] int pageIndex,
            [FromForm] int pagesize)
        {
            var user = UserCurrent(storeId, out Model.StoreModel store);
            if (store == null)
                return ApiResult.RCode("未知错误");

            begin = begin.Date;
            end = end.Date.AddDays(1).AddSeconds(-1);
            var res = BLL.OrderBLL.QueryPageListBenifitByStoreId(sId, begin, end, store.StoreId, isHasReturn,
                pageIndex, pagesize);


            var orderstat = OrderListStat(BLL.OrderBLL.QueryListBenifitByStoreId(sId, begin, end, store.StoreId,
                isHasReturn));


            return ApiResult.RData(new GridData<Model.OrderModel>(res.Rows, (int) res.Total, orderstat));
        }

        public IActionResult WithDrawInsert([FromForm] double amount,
            [FromForm] int bankType, [FromForm] string bankaccount, [FromForm] string bankPersonName,
            [FromForm] string name)
        {
            var user = UserCurrent();


            var userExtend = BLL.UserExtendBLL.QueryModelById(user.UserId);
            if (userExtend == null)
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);

            if (string.IsNullOrEmpty(name))
                return ApiResult.RCode("收款名称不能为空");
            // if (string.IsNullOrEmpty(BankPersonName))
            //     return ApiResult.RCode("收款开户行不能为空");
            if (string.IsNullOrEmpty(bankaccount))
                return ApiResult.RCode("收款帐号不能为空");

            if (amount < SiteContext.Config.WithDrawMin)
                return ApiResult.RCode($"提现金额必须大于等于{SiteContext.Config.WithDrawMin}");
            if (amount > SiteContext.Config.WithDrawMax)
                return ApiResult.RCode($"提现金额必须小于等于{SiteContext.Config.WithDrawMax}");

            if (amount > userExtend.Amount)
                return ApiResult.RCode("提现金额不能大于商户资金");
            var withdraw = new Model.WithDrawModel
            {
                Amount = amount,
                BankAccount = bankaccount,
                BankPersonName = bankPersonName,
                BankType = (EBankType) bankType,
                CreateDate = DateTime.Now,
                FinishDate = DateTime.Now,
                Income = 0,
                IsFinish = false,
                Memo = string.Empty,
                UserId = user.UserId,
                TranId = string.Empty,
                WithDrawId = Global.Generator.DateId()
            };

            BLL.WithDrawBLL.Insert(withdraw);
            UserLog(user.UserId, EUserLogType.提现, Request, "", "提现申请发布");
            return ApiResult.RCode("");
        }

        public IActionResult WithDrawPageList([FromForm] int pageIndex,
            [FromForm] int pagesize, [FromForm] int state, [FromForm] DateTime begin, [FromForm] DateTime end)
        {
            var user = UserCurrent();

            begin = begin.Date;
            end = end.Date.AddDays(1).AddSeconds(-1);
            var res = BLL.WithDrawBLL.QueryPageList(user.UserId, state, begin, end, pageIndex, pagesize);
            return ApiResult.RData(new GridData<Model.WithDrawModel>(res.Rows, (int) res.Total));
        }

        public IActionResult WithDrawDelete([FromForm] string id)
        {
            var user = UserCurrent();
            if (string.IsNullOrEmpty(id))
                return ApiResult.RCode("ID不能为空");

            var withdraw = BLL.WithDrawBLL.QueryModelByWithDrawIdAndUserId(id, user.UserId);
            if (withdraw == null)
                return ApiResult.RCode("数据不存在或已被删除");
            if (withdraw.IsFinish)
                return ApiResult.RCode("提现申请已处理，不能删除");
            BLL.WithDrawBLL.DeleteByWithDrawIdAndUserId(id, user.UserId);
            UserLog(user.UserId, EUserLogType.提现, Request, "", "提现申请删除");
            return ApiResult.RCode("");
        }

        public IActionResult ProductListIsStock([FromForm] string storeId, [FromForm] string category)
        {
            var user = UserCurrent(storeId, out Model.StoreModel store);
            if (store == null)
                return ApiResult.RCode("未知错误");
            return ApiResult.RData(BLL.ProductBLL
                .QueryListByCategoryIdAndStoreIdIsStock(category, store.StoreId));
        }


// public IActionResult SupplierPageList([FromForm] string StoreId, [FromForm] int PageIndex,
//     [FromForm] int Pagesize)
// {
//     var user = User(StoreId, out Model.Store store);
//     if (store == null)
//         return ApiResult.RCode("未知错误");
//     return new JsonResult(Api.UserApi.Supplier.GetPageList(user, StoreId, PageIndex, Pagesize));
// }
//
// public IActionResult SupplierSave([FromForm] string StoreId, [FromForm] string Sid, [FromForm] string Name,
//     [FromForm] int BankType, [FromForm] string Email, [FromForm] string BankAccount, [FromForm] string QQ,
//     [FromForm] double Feerate)
// {
//     var user = User(StoreId, out Model.Store store);
//     if (store == null)
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
//     if (store == null)
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
    }
}