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
                return ApiResult.RCode(ApiResult.ECode.Success);
            }
            else
            {
                var adminOrigin = BLL.AdminBLL.QueryModelById(adminModel.AdminId);
                if (adminOrigin == null)
                    return ApiResult.RCode(ApiResult.ECode.TargetNotExist);
                var datacompare = BLL.AdminBLL.QueryModelByAccount(adminModel.Account);
                if (datacompare != null && datacompare.AdminId != adminModel.AdminId)
                    return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);
                
                adminOrigin.Account = adminModel.Account;
                adminOrigin.IsRoot = adminModel.IsRoot;
                if (!string.IsNullOrEmpty(adminModel.Password))
                    adminOrigin.Password = Global.Hash(adminModel.Password, adminOrigin.Salt);
                
                BLL.AdminBLL.Update(adminOrigin);
                
                AdminLog(admin.AdminId, EAdminLogType.管理员管理, Request, "管理员修改" + adminModel.AdminId);
                return ApiResult.RCode(ApiResult.ECode.Success);
            }
        }
        
        [HttpPost]
        public IActionResult AdminDelete([FromForm] int adminId)
        {
            var admin = AdminCurrent();

            if (adminId == 0)
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);
            
            BLL.AdminBLL.DeleteById(adminId);

            AdminLog(admin.AdminId, EAdminLogType.管理员管理, Request, "管理员删除" + adminId);

            return ApiResult.RCode(ApiResult.ECode.Success);
        }
        
        [HttpPost]
        public IActionResult UserStorePageList([FromForm] int pageIndex, [FromForm] int pageSize, [FromForm] string keyname)
        {
            var res = BLL.UserBLL.QueryPageListBySearch(keyname, pageIndex, pageSize);

            return ApiResult.RData(res);
        }
        
        [HttpPost]
        public IActionResult UserSave(Model.Extend.UserStore userStore)
        {
            var admin = AdminCurrent();
            
            
            if (userStore.UserId == 0)
            {
                if (BLL.UserBLL.QueryModelByAccount(userStore.Account) != null)
                    return ApiResult.RCode(ApiResult.ECode.TargetExist);
                var salt = Global.Generator.Guid().Substring(0, 6);
                var userModel = new UserModel()
                {
                    Account = userStore.Account,
                    Salt = salt,
                    Password = Global.Hash(userStore.Password, salt),
                    ClientKey = "",
                };
                BLL.UserBLL.Insert(userModel);
                var userExtendModel = new UserExtendModel()
                {
                    UserId =  userModel.UserId,
                    Amount = 0,
                    AmountCharge = 0,
                    Level = userStore.UserExtend.Level,
                    Name = userStore.UserExtend.Name,
                    IdCard = userStore.UserExtend.IdCard,
                    TelPhone = userStore.UserExtend.TelPhone,
                    Email = userStore.UserExtend.Email,
                    QQ = userStore.UserExtend.QQ,
                    RegisterDate = DateTime.Now,
                    RegisterIP = Utils.RequestInfo._ClientIP(HttpContext).ToString(),
                    UserAgent =  Request.Headers["User-Agent"].ToString(),
                    AcceptLanguage = Request.Headers["Accept-Language"].ToString(),
                };
                BLL.UserExtendBLL.Insert(userExtendModel);
                AdminLog(admin.AdminId, EAdminLogType.商户管理, Request, "商户添加" + userModel.UserId);
                return ApiResult.RCode(ApiResult.ECode.Success);
            }
            else
            {
                var userOrigin = BLL.UserBLL.QueryModelById(userStore.UserId);
                var userExtendOrigin = BLL.UserExtendBLL.QueryModelById(userStore.UserId);
                if (userOrigin == null || userExtendOrigin == null)
                    return ApiResult.RCode(ApiResult.ECode.TargetNotExist);
                var datacompare = BLL.UserBLL.QueryModelByAccount(userStore.Account);
                if (datacompare != null && datacompare.UserId != userStore.UserId)
                    return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);
                
                userOrigin.Account = userStore.Account;
                if (!string.IsNullOrEmpty(userStore.Password))
                    userOrigin.Password = Global.Hash(userStore.Password, userOrigin.Salt);
                
                BLL.UserBLL.Update(userOrigin);

                userExtendOrigin.Level = userStore.UserExtend.Level;
                userExtendOrigin.Name = userStore.UserExtend.Name;
                userExtendOrigin.IdCard = userStore.UserExtend.IdCard;
                userExtendOrigin.TelPhone = userStore.UserExtend.TelPhone;
                userExtendOrigin.Email = userStore.UserExtend.Email;
                userExtendOrigin.QQ = userStore.UserExtend.QQ;
                
                BLL.UserExtendBLL.Update(userExtendOrigin);
                
                AdminLog(admin.AdminId, EAdminLogType.商户管理, Request, "商户修改" + userOrigin.UserId);
                return ApiResult.RCode(ApiResult.ECode.Success);
            }
        }
        
        
        [HttpPost]
        public IActionResult UserAmountChange([FromForm] int userId,[FromForm] double amountChange,[FromForm] int type)
        {
            var admin = AdminCurrent();
            
            if(!admin.IsRoot)
                return ApiResult.RCode(ApiResult.ECode.AuthorizationFailed);

            if (userId == 0)
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);

            if (type == 0)
            {
                BLL.UserExtendBLL.Update(p=> p.UserId == userId,p=> new UserExtendModel() { Amount =  p.Amount + amountChange });

                BLL.BillBLL.Insert(new BillModel()
                {
                    BillId = Global.Generator.DateId(1),
                    UserId = userId,
                    Amount = amountChange,
                    AmountCharge = 0,
                    BillType = EBillType.系统,
                    CreateDate = DateTime.Now,
                });
                
                AdminLog(admin.AdminId, EAdminLogType.商户管理, Request, "资金修改" + userId + " 【"+amountChange.ToString()+"】");
            }
            else if (type == 1)
            {
                BLL.UserExtendBLL.Update(p=> p.UserId == userId,p=> new UserExtendModel() { AmountCharge = p.AmountCharge + amountChange });
                
                BLL.BillBLL.Insert(new BillModel()
                {
                    BillId = Global.Generator.DateId(1),
                    UserId = userId,
                    Amount = 0,
                    AmountCharge = amountChange,
                    BillType = EBillType.系统,
                    CreateDate = DateTime.Now,
                });
                
                AdminLog(admin.AdminId, EAdminLogType.商户管理, Request, "签帐额修改" + userId + " 【"+amountChange.ToString()+"】");
            }


            return ApiResult.RCode(ApiResult.ECode.Success);
        }
        
        [HttpPost]
        public IActionResult UserDelete([FromForm] int userId)
        {
            var admin = AdminCurrent();

            if (userId == 0)
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);
            
            BLL.UserBLL.DeleteById(userId);

            AdminLog(admin.AdminId, EAdminLogType.商户管理, Request, "商户删除" + userId);

            return ApiResult.RCode(ApiResult.ECode.Success);
        }
        
        [HttpPost]
        public IActionResult StoreSave(StoreModel storeModel)
        {
            var admin = AdminCurrent();
            
            if(storeModel.UserId == 0)
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);
            var userModel = BLL.UserBLL.QueryModelById(storeModel.UserId);
            if(userModel == null)
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);
            
            if (string.IsNullOrWhiteSpace(storeModel.StoreId))
            {
                storeModel.StoreId = Global.Generator.DateId(1);
                storeModel.Initial = Global.Initial(storeModel.Name);
                storeModel.PaymentList = SiteContext.Payment.SystemPaymentList();
                BLL.StoreBLL.Insert(storeModel);
                AdminLog(admin.AdminId, EAdminLogType.商户管理, Request, "商户添加" + userModel.UserId);
                return ApiResult.RCode(ApiResult.ECode.Success);
            }
            else
            {
                var storeOrigin = BLL.StoreBLL.QueryModelById( storeModel.StoreId);

                if (storeModel.Name.Length >= 10)
                    return ApiResult.RCode(ApiResult.ECode.DataFormatError);

                if (storeModel.UniqueId.Length <= 5)
                    return ApiResult.RCode(ApiResult.ECode.DataFormatError);

                if (!string.IsNullOrWhiteSpace(storeModel.UniqueId))
                {
                    StoreModel compare = BLL.StoreBLL.QueryModelByUniqueId(storeModel.UniqueId);
                    if (compare != null && !string.Equals(compare.StoreId, storeModel.StoreId,
                        StringComparison.OrdinalIgnoreCase))
                        return ApiResult.RCode(ApiResult.ECode.TargetExist);
                }

                storeOrigin.Logo = SiteContext.Resource.MoveTempFile(storeModel.Logo);

                storeOrigin.Name = storeModel.Name;
                storeOrigin.Initial = Global.Initial(storeModel.Name);
                storeOrigin.UniqueId = storeModel.UniqueId;
                storeOrigin.Template = storeModel.Template;
                storeOrigin.Memo = string.IsNullOrWhiteSpace(storeModel.Memo) || storeModel.Memo.Length > 4000 ? "" : storeModel.Memo;
                storeOrigin.Email = storeModel.Email;
                storeOrigin.TelPhone = storeModel.TelPhone;
                storeOrigin.QQ = storeModel.QQ;

                BLL.StoreBLL.Update(storeOrigin);
                
                AdminLog(admin.AdminId, EAdminLogType.店铺管理, Request, "店铺修改" + storeOrigin.StoreId);
                return ApiResult.RCode(ApiResult.ECode.Success);
            }

        }
        
        [HttpPost]
        public IActionResult StoreDelete([FromForm] string storeId)
        {
            var admin = AdminCurrent();

            if (string.IsNullOrWhiteSpace(storeId))
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);
            
            BLL.StoreBLL.DeleteById(storeId);

            AdminLog(admin.AdminId, EAdminLogType.店铺管理, Request, "店铺删除" + storeId);

            return ApiResult.RCode(ApiResult.ECode.Success);
        }
        
        [HttpPost]
        public IActionResult SupplyList()
        {
            var admin = AdminCurrent();

            return ApiResult.RData(BLL.SupplyBLL.QueryList(p=>p.UserId == SiteContext.Config.SupplyUserIdSys));
        }
        
        [HttpPost]
        public IActionResult SupplyListSave(List<SupplyModel> supplyList)
        {
            var admin = AdminCurrent();

            var supplySystem = BLL.SupplyBLL.QueryList(p=>p.UserId == SiteContext.Config.SupplyUserIdSys);
            
            foreach (SupplyModel supplyModel in supplyList)
            {
                SupplyModel data = supplySystem.FirstOrDefault(p => p.SupplyId == supplyModel.SupplyId);
                if (data == null)
                {
                    supplyModel.SupplyId = Global.Generator.DateId(1);
                    supplyModel.UserId = SiteContext.Config.SupplyUserIdSys;
                    
                    supplyModel.Category = string.IsNullOrWhiteSpace(supplyModel.Category) ? "" : supplyModel.Category;
                    supplyModel.Memo = string.IsNullOrWhiteSpace(supplyModel.Memo) || supplyModel.Memo.Length > 4000
                        ? ""
                        : supplyModel.Memo;

                    BLL.SupplyBLL.Insert(supplyModel);

                    supplySystem.Add(supplyModel);
                }
                else
                {
                    data.UserId = SiteContext.Config.SupplyUserIdSys;
                    data.IsShow = supplyModel.IsShow;
                    
                    data.Name = supplyModel.Name;
                    data.DeliveryType = supplyModel.DeliveryType;
                    data.Category = supplyModel.Category;
                    data.FaceValue = supplyModel.FaceValue;
                    data.Cost = supplyModel.Cost;
                    data.Memo = string.IsNullOrWhiteSpace(supplyModel.Memo) || supplyModel.Memo.Length > 4000
                        ? ""
                        : supplyModel.Memo;

                    BLL.SupplyBLL.Update(data);
                }
            }

            var supplyIds2Remove = supplySystem.Where(p => supplyList.All(x => x.SupplyId != p.SupplyId))
                .Select(p => p.SupplyId).ToList();
            if (supplyIds2Remove.Count > 0)
            {
                BLL.SupplyBLL.DeleteByIdsAndUserId(supplyIds2Remove, SiteContext.Config.SupplyUserIdSys);
                foreach (var supplyId in supplyIds2Remove)
                    supplySystem.Remove(supplySystem.FirstOrDefault(p => p.SupplyId == supplyId));
            }
            
            AdminLog(admin.AdminId, EAdminLogType.货源管理, Request);
            
            return ApiResult.RData(supplySystem);
        }
        
         [HttpPost]
        public IActionResult StockPageList([FromForm] string supplyId, [FromForm] string keyname,
            [FromForm] string isShow,
            [FromForm] int pageIndex, [FromForm] int pageSize)
        {
            var admin = AdminCurrent();

            SupplyModel supplyModel = BLL.SupplyBLL.QueryModelById(supplyId);
            if (supplyModel == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

            bool? outIsShow = null;
            if (bool.TryParse(isShow, out var tempIsShow)) outIsShow = tempIsShow;

            var res = BLL.StockBLL.QueryPageListByUserSearch(supplyId, SiteContext.Config.SupplyUserIdSys, keyname, outIsShow, pageIndex,
                pageSize);

            return ApiResult.RData(new GridData<StockModel>(res.Rows, res.Total));
        }

        [HttpPost]
        public IActionResult StockMultipleAction([FromForm] string stockIds, [FromForm] int action)
        {
            var admin = AdminCurrent();

            var stockIdList = Global.Json.Deserialize<List<string>>(stockIds);

            if (stockIdList != null && stockIdList.Count > 0)
                switch (action)
                {
                    case 0:
                    {
                        //删除
                        BLL.StockBLL.Delete(p => p.UserId == SiteContext.Config.SupplyUserIdSys && stockIdList.Contains(p.StockId));
                    }
                        break;
                    case 1:
                    {
                        //下架 isShow = false
                        BLL.StockBLL.Update(p => p.UserId == SiteContext.Config.SupplyUserIdSys && stockIdList.Contains(p.StockId),
                            p => p.IsShow == false);
                    }
                        break;
                    case 2:
                    {
                        //上架 isShow = true
                        BLL.StockBLL.Update(p => p.UserId == SiteContext.Config.SupplyUserIdSys && stockIdList.Contains(p.StockId),
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
            var admin = AdminCurrent();

            SupplyModel supplyModel = BLL.SupplyBLL.QueryModelById(supplyId);
            if (supplyModel == null)
                return ApiResult.RCode(ApiResult.ECode.TargetNotExist);

            bool? outIsShow = null;
            if (bool.TryParse(isShow, out var tempIsShow)) outIsShow = tempIsShow;

            var res = BLL.StockBLL.QueryListByUserSearch(supplyId, SiteContext.Config.SupplyUserIdSys, keyname, outIsShow);

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
            var admin = AdminCurrent();

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

                                UserId = SiteContext.Config.SupplyUserIdSys,
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
                        p.UserId == SiteContext.Config.SupplyUserIdSys && p.SupplyId == supplyId && namelist.Contains(p.Name));
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
        public IActionResult UserFinace()
        {
            var admin = AdminCurrent();

            var userFinace = BLL.UserExtendBLL.QueryFinace();
            var withDrawAll =  BLL.WithDrawBLL.QueryFinace();
            
            return ApiResult.RData(new {Amount=userFinace.Amount,AmountCharge=userFinace.AmountCharge,WithDraw = withDrawAll});
        }
        
        [HttpPost]
        public IActionResult BillPageList([FromForm] string from, [FromForm] string to,
            [FromForm] int billType,
            [FromForm] int pageIndex, [FromForm] int pageSize)
        {
            var admin = AdminCurrent();

            DateTime? dateFrom = null;
            if (DateTime.TryParse(from, out DateTime tempdateFrom)) dateFrom = tempdateFrom;

            DateTime? dateTo = null;
            if (DateTime.TryParse(to, out DateTime tempdateTo)) dateTo = tempdateTo.AddDays(1);

            if (dateFrom == null || dateTo == null)
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);

            var data = BLL.BillBLL.QueryPageListBySearch(pageIndex, pageSize, billType,
                (DateTime) dateFrom, (DateTime) dateTo);

            return ApiResult.RData(data);
        }
        
        [HttpPost]
        public IActionResult WithDrawPageList([FromForm] string from, [FromForm] string to,
            [FromForm] string isFinish,
            [FromForm] int pageIndex, [FromForm] int pageSize)
        {
            var admin = AdminCurrent();

            DateTime? dateFrom = null;
            if (DateTime.TryParse(from, out DateTime tempdateFrom)) dateFrom = tempdateFrom;

            DateTime? dateTo = null;
            if (DateTime.TryParse(to, out DateTime tempdateTo)) dateTo = tempdateTo.AddDays(1);

            if (dateFrom == null || dateTo == null)
                return ApiResult.RCode(ApiResult.ECode.DataFormatError);
            
            bool? outIsFinish = null;
            if (bool.TryParse(isFinish, out var tempIsFinish)) outIsFinish = tempIsFinish;
            
            var data = BLL.WithDrawBLL.QueryPageListBySearch(pageIndex, pageSize,outIsFinish,
                (DateTime) dateFrom, (DateTime) dateTo);

            return ApiResult.RData(data);
        }
        
        [HttpPost]
        public IActionResult WithDrawSave(Model.WithDrawModel withDraw)
        {
            var admin = AdminCurrent();

            if (withDraw != null && withDraw.IsFinish)
            {
                if (withDraw.AmountFinish > withDraw.Amount)
                {
                    withDraw.AmountFinish = withDraw.Amount;
                }

                if (withDraw.AmountFinish == 0)
                {
                    BLL.BillBLL.Insert(new BillModel()
                    {
                        BillId = Global.Generator.DateId(1),
                        UserId = withDraw.UserId,
                        Amount = withDraw.Amount,
                        AmountCharge = 0,
                        BillType = EBillType.提现,
                        CreateDate = DateTime.Now,
                        Extra = withDraw.WithDrawId
                    });
                    
                    BLL.UserExtendBLL.Update(p => p.UserId == withDraw.UserId,
                        p => new UserExtendModel()
                        {
                            Amount = p.Amount + withDraw.Amount
                        });
                }
                
                BLL.WithDrawBLL.Update(withDraw);
            }

            return ApiResult.RCode(ApiResult.ECode.Success);
        }

         [HttpPost]
        public IActionResult OrderPageList([FromForm] string from, [FromForm] string to,
            [FromForm] string keyname, [FromForm] string isSettle,
            [FromForm] int pageIndex, [FromForm] int pageSize)
        {
            var admin = AdminCurrent();

            bool? outIsSettle = null;
            if (bool.TryParse(isSettle, out var tempIsSettle)) outIsSettle = tempIsSettle;

            DateTime? dateFrom = null;
            if (DateTime.TryParse(from, out DateTime tempdateFrom)) dateFrom = tempdateFrom;

            DateTime? dateTo = null;
            if (DateTime.TryParse(to, out DateTime tempdateTo)) dateTo = tempdateTo.AddDays(1);

            var res =  BLL.OrderBLL.QueryPageListBySearch4SystemSettle(SiteContext.Config.SupplyUserIdSys,dateFrom, dateTo, keyname, outIsSettle,  pageIndex, pageSize);

            return ApiResult.RData(res);
        }

        [HttpPost]
        public IActionResult OrderSave([FromQuery] string storeId, OrderModel order)
        {
            var admin = AdminCurrent();
           
            OrderModel data =  BLL.OrderBLL.QueryModelById(order.OrderId);
            if (data != null)
            {
                data.Memo = order.Memo;

                BLL.OrderBLL.Update(data);

                return ApiResult.RCode();
            }

            return ApiResult.RCode(ApiResult.ECode.UnKonwError);
        }

        [HttpPost]
        public IActionResult OrderMultipleAction([FromForm] string storeId, [FromForm] string orderIds,
            [FromForm] int action)
        {
            var admin = AdminCurrent();

            var orderIdList = Global.Json.Deserialize<List<string>>(orderIds);

            if (orderIdList != null && orderIdList.Count > 0)
                switch (action)
                {
                    case 1:
                    {
                        //结算 isSettle = true
                        BLL.OrderBLL.Update(
                            p => orderIdList.Contains(p.OrderId) && p.IsPay && p.IsDelivery,
                            p => new OrderModel {IsSettle = true, SettleDate = DateTime.Now});
                    }
                        break;
                }

            return ApiResult.RCode();
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
    }
}