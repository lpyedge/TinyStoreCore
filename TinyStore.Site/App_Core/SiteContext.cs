using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Text.Json.Serialization;
using IP2Region;
using LPayments;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SqlSugar;
using TinyStore.BLL;
using TinyStore.Model;
using TinyStore.Model.Extend;
using TinyStore.Utils;

namespace TinyStore.Site
{
    public class SiteContext
    {
        public static ConfigModel Config { get; set; }

        public static void ConfigSave()
        {
            var configJson = Global.Json.SerializePretty(new {Config});
            File.WriteAllText(AppDomain.CurrentDomain.BaseDirectory + "App_Data/config.json", configJson);
        }

        /// <summary>
        ///     初始化
        /// </summary>
        public static void Inited(ServiceProvider services, IConfiguration configuration)
        {
            // var ConnStr1 = configuration.GetSection("Config:ConnStr").Value;
            // var ConnStr2 = configuration.GetSection("Config:ConnStr").Get<string>();
            // configuration.GetSection("Config").Get<ConfigModel>();
            //Global.AppSettings.Configuration.GetSection("Config").Bind(Config);

            Config = configuration.GetSection("Config").Get<ConfigModel>();

            InitData();
        }

        private static void InitData()
        {
            BaseBLL.Init(DbType.Sqlite, Config.AppData + "Data.db");

            if (!File.Exists(Config.AppData + "Data.db"))
            {
                BaseBLL.DbClient.DbMaintenance.CreateDatabase();
                BaseBLL.DbClient.CodeFirst.InitTables<AdminModel>();
                BaseBLL.DbClient.CodeFirst.InitTables<AdminLogModel>();
                BaseBLL.DbClient.CodeFirst.InitTables<BillModel>();
                BaseBLL.DbClient.CodeFirst.InitTables<OrderModel>();
                BaseBLL.DbClient.CodeFirst.InitTables<OrderTrashModel>();
                BaseBLL.DbClient.CodeFirst.InitTables<ProductModel>();
                BaseBLL.DbClient.CodeFirst.InitTables<StockModel>();
                BaseBLL.DbClient.CodeFirst.InitTables<StoreModel>();
                BaseBLL.DbClient.CodeFirst.InitTables<SupplyModel>();
                BaseBLL.DbClient.CodeFirst.InitTables<UserModel>();
                BaseBLL.DbClient.CodeFirst.InitTables<UserExtendModel>();
                BaseBLL.DbClient.CodeFirst.InitTables<UserLogModel>();
                BaseBLL.DbClient.CodeFirst.InitTables<WithDrawModel>();

#if DEBUG
                InitDevData();
#endif
            }

            if (AdminBLL.QueryCount(p => p.IsRoot) == 0)
                AdminBLL.Insert(new AdminModel
                {
                    Account = "admin",
                    ClientKey = Guid.NewGuid().ToString(),
                    CreateDate = DateTime.Now,
                    IsRoot = true,
                    Password =
#if DEBUG
                        Global.Hash("admin", "012345"),
#else
                        Global.Hash("tinystorecore", "012345"),
#endif
                    Salt = "012345"
                });
        }

        private static void InitDevData()
        {
            UserBLL.InsertAsync(new UserModel
            {
                Account = "test",
                Password = Global.Hash("test", "test"),
                Salt = "test",
                ClientKey = "test"
            });
            UserExtendBLL.InsertAsync(new UserExtendModel
            {
                UserId = 1,

                Amount = 0,
                AmountCharge = 1000,
                Level = EUserLevel.一星,

                Email = "test@test.com",
                Name = "姓名",
                RegisterDate = DateTime.Now,
                RegisterIP = "127.0.0.1",
                UserAgent = "",
                AcceptLanguage = "",

                BankAccount = "",
                BankType = EBankType.支付宝,
                BankPersonName = "",
                IdCard = "",
                QQ = "",
                TelPhone = ""
            });
            var storeId = Global.Generator.DateId(2);
            storeId = "StoreId";
            StoreBLL.InsertAsync(new StoreModel
            {
                UserId = 1,
                Email = "test@test.com",
                Name = "小网店",
                Initial = Global.Initial("小网店"),
                Logo = "#",
                Memo = "自家供货，自家销售",
                Template = EStoreTemplate.模板一,
                IsSingle = true,
                PaymentList = new List<Model.Extend.Payment>(Payment.SystemPaymentList())
                {
                    new()
                    {
                        Subject = "支付宝",
                        Name = "A先生",
                        Account = "13101010101",
                        IsEnable = true,
                        IsSystem = false,
                        Rate = 0,
                        QRCode = "alipay://xxxxx",
                        Memo = "支付宝shoukuan"
                    }
                },
                UniqueId = "test",
                StoreId = storeId
            });
            StoreBLL.InsertAsync(new StoreModel
            {
                UserId = 1,
                Email = "test@test.com",
                Name = "大网店",
                Initial = Global.Initial("大网店"),
                Logo = "#",
                Memo = "自家供货，自家销售",
                Template = EStoreTemplate.模板一,
                IsSingle = true,
                PaymentList = new List<Model.Extend.Payment>(Payment.SystemPaymentList())
                {
                    new()
                    {
                        Subject = "微信",
                        Name = "B先生",
                        Account = "13101010101",
                        IsEnable = true,
                        IsSystem = false,
                        Rate = 0,
                        QRCode = "wechat://xxxxx",
                        Memo = "微信 收款"
                    }
                },
                UniqueId = "test",
                StoreId = storeId + "1"
            });
            var supplyList = new List<SupplyModel>();
            supplyList.Add(new SupplyModel
            {
                SupplyId = "SupplyId",
                UserId = 1,
                Category = "",
                Cost = 5,
                Name = "盲盒",
                Memo = "盲盒",
                DeliveryType = EDeliveryType.人工,
                FaceValue = 10,
                IsShow = true
            });
            supplyList.Add(new SupplyModel
            {
                UserId = Config.SupplyUserIdSys,
                Category = "腾讯",
                Cost = 5,
                Name = "QQ币",
                Memo = "请留下您的QQ号码方便我们来充值",
                DeliveryType = EDeliveryType.人工,
                FaceValue = 10,
                IsShow = true,
                SupplyId = Global.Generator.DateId(2)
            });
            supplyList.Add(new SupplyModel
            {
                UserId = Config.SupplyUserIdSys,
                Category = "腾讯",
                Cost = 100,
                Name = "微信号",
                Memo = "请联系客服购买，标价非真实价格",
                DeliveryType = EDeliveryType.人工,
                FaceValue = 100,
                IsShow = true,
                SupplyId = Global.Generator.DateId(2)
            });
            supplyList.Add(new SupplyModel
            {
                UserId = Config.SupplyUserIdSys,
                Category = "腾讯",
                Cost = 100,
                Name = "QQ号",
                Memo = "请联系客服购买，标价非真实价格",
                DeliveryType = EDeliveryType.人工,
                FaceValue = 100,
                IsShow = true,
                SupplyId = Global.Generator.DateId(2)
            });
            supplyList.Add(new SupplyModel
            {
                UserId = Config.SupplyUserIdSys,
                Category = "网易",
                Cost = 35,
                Name = "魔兽点卡",
                Memo = "请联系客服购买，标价非真实价格",
                DeliveryType = EDeliveryType.卡密,
                FaceValue = 50,
                IsShow = true,
                SupplyId = Global.Generator.DateId(2)
            });
            supplyList.Add(new SupplyModel
            {
                UserId = Config.SupplyUserIdSys,
                Category = "网易",
                Cost = 35,
                Name = "魔兽怀旧服点卡",
                Memo = "请联系客服购买，标价非真实价格",
                DeliveryType = EDeliveryType.卡密,
                FaceValue = 50,
                IsShow = true,
                SupplyId = Global.Generator.DateId(2)
            });
            supplyList.Add(new SupplyModel
            {
                SupplyId = Global.Generator.DateId(2),
                UserId = 1,
                Category = "腾讯",
                Cost = 5,
                Name = "QQ币",
                Memo = "请留下您的QQ号码方便我们来充值",
                DeliveryType = EDeliveryType.人工,
                FaceValue = 10,
                IsShow = true
            });
            SupplyBLL.InsertRangeAsync(supplyList);
            for (var i = 0; i < 100; i++)
                StockBLL.InsertAsync(new StockModel
                {
                    StockId = $"StockId{i.ToString()}",
                    UserId = 1,
                    SupplyId = "SupplyId",
                    Name = $"QQ_{i.ToString("000")}_{DateTime.Now.Ticks.ToString()}",
                    Memo = DateTime.Now.Ticks.ToString(),
                    IsShow = true,
                    CreateDate = DateTime.Now,
                    IsDelivery = false,
                    DeliveryDate = DateTime.Now
                });

            var productList = new List<ProductModel>();
            productList.Add(new ProductModel
            {
                UserId = 1,
                Category = "",
                Cost = 5,
                Name = "盲盒",
                Memo = "盲盒",
                DeliveryType = EDeliveryType.人工,
                FaceValue = 10,
                IsShow = true,
                SupplyId = "SupplyId",
                ProductId = "ProductId",
                Amount = 8.5,
                Icon = "#",
                Sort = 0,
                QuantityMin = 1,
                StoreId = storeId
            });
            foreach (SupplyModel supplyModel in supplyList)
                productList.Add(new ProductModel
                {
                    UserId = 1,
                    Category = "",
                    Cost = supplyModel.Cost,
                    Name = supplyModel.Name,
                    Memo = supplyModel.Memo,
                    DeliveryType = supplyModel.DeliveryType,
                    FaceValue = supplyModel.FaceValue,
                    IsShow = true,
                    SupplyId = supplyModel.SupplyId,
                    ProductId = Global.Generator.DateId(2),
                    Amount = supplyModel.FaceValue * 0.85,
                    Icon = "#",
                    Sort = 0,
                    QuantityMin = 1,
                    StoreId = storeId
                });
            ProductBLL.InsertRangeAsync(productList);

            var orderList = new List<OrderModel>();
            orderList.Add(new OrderModel
            {
                OrderId = "test",
                Name = "盲盒",
                Memo = "",
                UserId = 1,
                StoreId = "StoreId",
                SupplyId = "SupplyId",
                ProductId = "ProductId",

                Amount = 18,
                Quantity = 2,
                Cost = 10,

                CreateDate = DateTime.Now,


                ClientIP = "127.0.0.1",
                AcceptLanguage = "",
                UserAgent = "",
                //客户输入数据
                Message = "100088",
                Contact = "test@qq.com",


                IsPay = false,
                PaymentFee = 0,
                PaymentType = "alipay",
                TranId = "123456",


                IsDelivery = false,
                DeliveryDate = DateTime.Now,
                StockList = new List<StockOrder>(),


                IsSettle = false,
                SettleDate = DateTime.Now,


                RefundAmount = 0,
                RefundDate = DateTime.Now,


                LastUpdateDate = DateTime.Now,
                NotifyDate = null
            });
            foreach (ProductModel productModel in productList)
                for (var i = 0; i < 60; i++)
                for (var j = 0; j < Global.Generator.Random.Next(3, 40); j++)
                {
                    var isrefund = Global.Generator.Random.NextDouble() > 0.9;
                    var quantity = Global.Generator.Random.Next(1, 30);
                    orderList.Add(new OrderModel
                    {
                        OrderId = Global.Generator.DateId(2),
                        Name = productModel.Name,
                        Memo = productModel.Memo,
                        UserId = 1,
                        StoreId = "StoreId",
                        SupplyId = productModel.SupplyId,
                        ProductId = productModel.ProductId,

                        Amount = productModel.Amount,
                        Quantity = quantity,
                        Cost = productModel.Cost,

                        CreateDate = DateTime.Now.AddDays(-i),


                        ClientIP = "127.0.0.1",
                        AcceptLanguage = "",
                        UserAgent = "",
                        //客户输入数据
                        Message = "100088",
                        Contact = "test@qq.com",


                        IsPay = true,
                        PaymentFee = 0,
                        PaymentType = "alipay",
                        TranId = Global.Generator.DateId(2),

                        IsDelivery = true,
                        DeliveryDate = DateTime.Now.AddDays(-i),
                        StockList = new List<StockOrder>(),

                        RefundAmount = isrefund ? productModel.Cost * 0.3 : 0,
                        RefundDate = isrefund ? DateTime.Now.AddDays(-i) : null,


                        LastUpdateDate = DateTime.Now.AddDays(-i),
                        NotifyDate = null
                    });
                }

            OrderBLL.InsertRangeAsync(orderList);

            var billList = new List<BillModel>();
            var billTypes = Enum.GetValues<EBillType>().ToList();

            for (var i = 0; i < 50; i++)
            for (var j = 0; j < Global.Generator.Random.Next(3, 40); j++)
            {
                var ischarge = Global.Generator.Random.NextDouble() > 0.7;
                var isplus = Global.Generator.Random.NextDouble() > 0.4;
                var amount = Global.Generator.Random.Next(1, 30);
                EBillType billType = billTypes[Global.Generator.Random.Next(0, 6)];
                billList.Add(new BillModel
                {
                    BillId = Global.Generator.DateId(2),
                    Amount = ischarge ? 0 : isplus ? amount : -amount,
                    AmountCharge = !ischarge ? 0 : isplus ? amount : -amount,
                    Extra = "",
                    CreateDate = DateTime.Now.AddDays(-50 + i),
                    StoreId = "StoreId",
                    UserId = 1,
                    BillType = billType
                });
            }

            BillBLL.InsertRangeAsync(billList);


            var withDrawList = new List<WithDrawModel>();
            for (var i = 0; i < 10; i++)
            {
                var isfinish = Global.Generator.Random.NextDouble() > 0.7;
                var isok = Global.Generator.Random.NextDouble() > 0.7;
                var amount = Global.Generator.Random.Next(1, 30);
                withDrawList.Add(new WithDrawModel
                {
                    WithDrawId = Global.Generator.DateId(2),
                    UserId = 1,
                    Amount = amount,
                    BankType = EBankType.支付宝,
                    BankAccount = "BankAccount",
                    BankPersonName = "BankPersonName",
                    CreateDate = DateTime.Now.AddDays(-30 + i),

                    Memo = "",

                    IsFinish = isfinish,
                    TranId = isfinish ? "TranId" : "",
                    FinishDate = isfinish ? DateTime.Now.AddDays(-25 + i) : null,
                    AmountFinish = isfinish ? isok ? amount : 0 : 0
                });
            }

            WithDrawBLL.InsertRangeAsync(withDrawList);
        }


        public static string IP2Region(string ip)
        {
            //ip = "49.82.194.75";
            using (var searcher = new DbSearcher(AppDomain.CurrentDomain.BaseDirectory + "App_Data/ip2region.db"))
            {
                return searcher.BtreeSearch(ip).Region;
            }
        }


        public class ConfigModel
        {
            [JsonIgnore] public int SupplyUserIdSys => 0;

            [JsonIgnore] public double SysPaymentRate => 0.006;

            [JsonIgnore] public string FormatDate => "yyyy-MM-dd";

            [JsonIgnore] public string FormatDateTime => "yyyy-MM-dd HH:mm";

            [JsonIgnore] public string AppData => AppDomain.CurrentDomain.BaseDirectory + "App_Data/";

            [JsonIgnore] public string UserData => AppDomain.CurrentDomain.BaseDirectory + "User_Data/";


            /// <summary>
            ///     网站域名
            /// </summary>
            public string SiteDomain { get; set; } = "";

            /// <summary>
            ///     网站名称
            /// </summary>
            public string SiteName { get; set; } = "";

            /// <summary>
            ///     客户QQ
            /// </summary>
            public string ServiceQQ { get; set; } = "";

            /// <summary>
            ///     客户邮箱
            /// </summary>
            public string ServiceEmail { get; set; } = "";


            /// <summary>
            ///     订单提醒默认天数,0 即不提醒
            /// </summary>
            public int OrderNotifyDays { get; set; } = 30;

            /// <summary>
            ///     订单提醒到期预先提示天数
            /// </summary>
            public int OrderNotifyPreDays { get; set; } = 7;

            /// <summary>
            ///     提现最低金额
            /// </summary>
            public double WithDrawMin { get; set; } = 10;

            /// <summary>
            ///     提现最高金额
            /// </summary>
            public double WithDrawMax { get; set; } = 10000;


            /// <summary>
            ///     发件邮箱配置
            /// </summary>
            public EmailContext.EmailServer EmailServer { get; set; } = new();


            /// <summary>
            ///     代理等级&amp;折扣
            /// </summary>
            public Dictionary<EUserLevel, double> UserRates { get; set; } = new()
            {
                [EUserLevel.无] = 1,
                [EUserLevel.一星] = 0.999,
                [EUserLevel.二星] = 0.998,
                [EUserLevel.三星] = 0.997,
                [EUserLevel.合作商] = 0.995
            };

            public Dictionary<string, string> WechatPaySettings { get; set; }

            public Dictionary<string, string> AliPaySettings { get; set; }
        }

        public static class Resource
        {
            public const string ResourcePrefix = "Resource";
            public const string Temp = "Temp";
            public const string FileSuffix = ".base64";
            public const string StoreDirectory = "Upload";

            public static ApiResult UploadFiles(string Model, string Id, string Name, Dictionary<string, byte[]> Files)
            {
                if (!string.IsNullOrWhiteSpace(Model) && !string.IsNullOrWhiteSpace(Id) &&
                    !string.IsNullOrWhiteSpace(Name)
                    && Files != null && Files.Count > 0
                    && Files.All(p => !string.IsNullOrWhiteSpace(p.Key))
                    && Files.All(p => p.Value.Length > 0))
                {
                    if (Files.Count == 1)
                    {
                        var uripath = $"/{ResourcePrefix}_{Temp}/{Model}/{Id}/{Name}";


                        SaveTempFile(uripath, Files.First().Key, Files.First().Value);

                        return new ApiResult<string>(uripath);
                    }

                    var namelist = new List<string>();
                    for (var i = 0; i < Files.Count; i++)
                    {
                        var uripath = $"/{ResourcePrefix}_{Temp}/{Model}/{Id}/{Name}_{i.ToString("00")}";

                        SaveTempFile(uripath, Files.ElementAt(i).Key, Files.ElementAt(i).Value);

                        namelist.Add(uripath);
                    }

                    return new ApiResult<List<string>>(namelist);
                }

                return new ApiResult(ApiResult.ECode.Fail);
            }

            //文件存储至本地临时文件夹
            public static void SaveTempFile(string tempuripath, string contenttype, byte[] buffer)
            {
                if (!string.IsNullOrWhiteSpace(tempuripath) && !string.IsNullOrWhiteSpace(contenttype) &&
                    buffer.Length > 0)
                {
                    var result = new FileContentResult(buffer, contenttype)
                        {EnableRangeProcessing = true};
                    MemoryCacher.Set(tempuripath, result, MemoryCacher.CacheItemPriority.Normal,
                        DateTime.Now.AddMinutes(5));


                    var tempfilepath = tempuripath.Replace($"/{ResourcePrefix}_{Temp}", $"/{StoreDirectory}/{Temp}") +
                                       FileSuffix;

                    var base64str = $"data:{contenttype};base64," + Convert.ToBase64String(buffer);
                    Global.FileSave(AppDomain.CurrentDomain.BaseDirectory + tempfilepath, base64str, false); //保存数据到服务器
                }
            }

            public static string MoveTempFile(string tempuripath, bool overwrite = true) //将文件转移到正式目录下
            {
                try
                {
                    if (!string.IsNullOrWhiteSpace(tempuripath) && tempuripath.StartsWith($"/{ResourcePrefix}_{Temp}"))
                    {
                        var uripath = tempuripath.Replace($"/{ResourcePrefix}_{Temp}", $"/{ResourcePrefix}");
                        var filepath = tempuripath.Replace($"/{ResourcePrefix}_{Temp}", $"/{StoreDirectory}") +
                                       FileSuffix;
                        var tempfilepath =
                            tempuripath.Replace($"/{ResourcePrefix}_{Temp}", $"/{StoreDirectory}/{Temp}") + FileSuffix;

                        var tempfile = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + tempfilepath);
                        if (tempfile.Exists)
                        {
                            var destfile = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + filepath);
                            if (destfile.Exists && !overwrite) return uripath;

                            if (!destfile.Directory.Exists) //目标文件夹不存在则创建
                                destfile.Directory.Create();

                            tempfile.MoveTo(destfile.FullName, overwrite); //转移文件导目标路径
                            MemoryCacher.Remove(tempuripath);
                            MemoryCacher.Remove(uripath);
                        }

                        return uripath;
                    }
                }
                catch (Exception ex)
                {
                }

                return tempuripath;
            }

            public static ActionResult Result(string Model, string Id, string Name,
                bool istemp)
            {
                if (!string.IsNullOrWhiteSpace(Model) && !string.IsNullOrWhiteSpace(Id) &&
                    !string.IsNullOrWhiteSpace(Name))
                {
                    var uripath = istemp
                        ? $"/{ResourcePrefix}_{Temp}/{Model}/{Id}/{Name}"
                        : $"/{ResourcePrefix}/{Model}/{Id}/{Name}";
                    var filepath = istemp
                        ? $"/{StoreDirectory}/{Temp}/{Model}/{Id}/{Name}{FileSuffix}"
                        : $"/{StoreDirectory}/{Model}/{Id}/{Name}{FileSuffix}";

                    FileContentResult result = null;
                    if (!MemoryCacher.TryGet(uripath, out result))
                    {
                        var file = new FileInfo(AppDomain.CurrentDomain.BaseDirectory + filepath);
                        if (file.Exists)
                            using (FileStream fs = file.Open(FileMode.Open, FileAccess.Read))
                            {
                                var byteArray = new byte[fs.Length];
                                fs.Read(byteArray, 0, byteArray.Length);
                                var base64str = Encoding.UTF8.GetString(byteArray);
                                var content = base64str.Split(",")[1];
                                var contentbytes = Convert.FromBase64String(content);
                                var contenttype = Global.Regex.FileContentType.Match(base64str).Groups[1].Value;
                                result = new FileContentResult(contentbytes, contenttype)
                                    {LastModified = file.LastWriteTime, EnableRangeProcessing = true};
                                MemoryCacher.Set(uripath, result, MemoryCacher.CacheItemPriority.Normal,
                                    DateTime.Now.AddMinutes(5));
                            }
                    }

                    if (result != null) return result;
                }

                return new NotFoundResult();
            }
        }

        public static class Email
        {
            static Email()
            {
                EmailContext.EmailServer.Instances["default"] = Config.EmailServer;

                EmailContext.EmailTemplate.Instances =
                    new ConcurrentDictionary<string, EmailContext.EmailTemplate>();
                foreach (var filePath in Directory.GetFiles(Config.AppData + "EmailTemplate/"))
                    try
                    {
                        var file = new FileInfo(filePath);
                        var fileContent = File.ReadAllText(file.FullName);
                        var data = Global.Json.Deserialize<EmailContext.EmailTemplate>(fileContent);
                        EmailContext.EmailTemplate.Instances[data.Key] = data;
                    }
                    catch (Exception e)
                    {
                    }
            }

            public static Dictionary<string, EmailContext.EmailTemplate> EmailTemplates =>
                EmailContext.EmailTemplate.Instances
                    .ToDictionary(p => p.Key, p => p.Value);


            public static void Send(string email, string subject, string content, bool isContentHtml = true)
            {
                var mailMessage = new MailMessage
                {
                    Subject = subject,
                    Body = content,
                    IsBodyHtml = isContentHtml,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8,
                    DeliveryNotificationOptions = DeliveryNotificationOptions.OnSuccess
                };
                mailMessage.To.Add(email);
                Send(mailMessage);
            }

            public static void Send(MailMessage p_MailMessage)
            {
                EmailContext.EmailServer emailserver = EmailContext.EmailServer.Instances["default"];

                emailserver.SendMailAsync(p_MailMessage);
            }
        }

        public static class Payment
        {
            private static readonly object _Locker = new();

            private static List<Model.Extend.Payment> _SystemPaymentList;

            public static List<Model.Extend.Payment> SystemPaymentList()
            {
                if (_SystemPaymentList == null)
                    lock (_Locker)
                    {
                        if (_SystemPaymentList == null)
                        {
                            _SystemPaymentList = new List<Model.Extend.Payment>();
                            _SystemPaymentList.Add(new Model.Extend.Payment
                            {
                                BankType = EBankType.支付宝,
                                Subject = "支付宝H5",
                                Name = EPlatform.Alipay + "|" + EChannel.AliPay + "|" + EPayType.H5,
                                Account = "",
                                Memo = "手机端调用",
                                Rate = Config.SysPaymentRate,
                                IsSystem = true,
                                IsEnable = false
                            });
                            _SystemPaymentList.Add(new Model.Extend.Payment
                            {
                                BankType = EBankType.支付宝,
                                Subject = "支付宝扫码",
                                Name = EPlatform.Alipay + "|" + EChannel.AliPay + "|" + EPayType.QRcode,
                                Account = "",
                                Memo = "电脑端调用",
                                Rate = Config.SysPaymentRate,
                                IsSystem = true,
                                IsEnable = false
                            });
                            _SystemPaymentList.Add(new Model.Extend.Payment
                            {
                                BankType = EBankType.支付宝,
                                Subject = "支付宝网关",
                                Name = EPlatform.Alipay + "|" + EChannel.AliPay + "|" + EPayType.PC,
                                Account = "",
                                Memo = "电脑端调用",
                                Rate = Config.SysPaymentRate,
                                IsSystem = true,
                                IsEnable = false
                            });
                            _SystemPaymentList.Add(new Model.Extend.Payment
                            {
                                BankType = EBankType.微信,
                                Subject = "微信H5",
                                Name = EPlatform.WeChat + "|" + EChannel.WeChat + "|" + EPayType.H5,
                                Account = "",
                                Memo = "手机端调用",
                                Rate = Config.SysPaymentRate,
                                IsSystem = true,
                                IsEnable = false
                            });
                            _SystemPaymentList.Add(new Model.Extend.Payment
                            {
                                BankType = EBankType.微信,
                                Subject = "微信扫码",
                                Name = EPlatform.WeChat + "|" + EChannel.WeChat + "|" + EPayType.QRcode,
                                Account = "",
                                Memo = "电脑端调用",
                                Rate = Config.SysPaymentRate,
                                IsSystem = true,
                                IsEnable = false
                            });
                        }
                    }

                return _SystemPaymentList;
            }

            public static string TransferToBank(Model.Extend.Payment payment, double amount = 0)
            {
                if ((int) payment.BankType >= 10)
                {
                    Global.AlipaySchema.EBankMark bankMark = Reflection.Attribute
                        .GetCustomAttribute<BankMarkAttribute>(payment.BankType).First().BankMark;
                    return Global.AlipaySchema.ToBankCard(bankMark, payment.Account, payment.Name, amount);
                }

                return null;
            }

            private static string PayEnum2Name(EPlatform platform, EChannel channel, EPayType payType)
            {
                return platform + "|" + channel + "|" + payType;
            }

            private static void PayName2Enum(string name, out EPlatform platform, out EChannel channel,
                out EPayType payType)
            {
                platform = 0;
                channel = EChannel.AliPay;
                payType = EPayType.PC;

                if (name.Contains('|'))
                {
                    var strs = name.Split('|');
                    if (strs.Length == 3)
                        try
                        {
                            platform = Enum.GetValues<EPlatform>().First(p => p.ToString() == strs[0]);
                            channel = Enum.GetValues<EChannel>().First(p => p.ToString() == strs[1]);
                            payType = Enum.GetValues<EPayType>().First(p => p.ToString() == strs[2]);
                        }
                        catch
                        {
                        }
                }
            }


            public static IPay GetPayment(string name)
            {
                PayName2Enum(name, out EPlatform platform, out EChannel channel, out EPayType payType);
                IPayChannel pay = Context.Get(platform, channel, payType);

                if (pay.Platform.ToString().StartsWith("alipay", StringComparison.OrdinalIgnoreCase))
                {
                    if (Config.AliPaySettings != null)
                    {
                        foreach (var set in Config.AliPaySettings) pay[set.Key] = set.Value;

                        return pay as IPay;
                    }
                }
                else if (pay.Platform.ToString().StartsWith("wechat", StringComparison.OrdinalIgnoreCase))
                {
                    if (Config.WechatPaySettings != null)
                    {
                        foreach (var set in Config.WechatPaySettings) pay[set.Key] = set.Value;

                        return pay as IPay;
                    }
                }

                return null;
            }

            public static string Notify(string payname, IDictionary<string, string> form,
                IDictionary<string, string> query, IDictionary<string, string> header, string body, string notifyIp)
            {
                var msg = "";
                try
                {
                    IPay payment = GetPayment(payname);

                    if (payment != null)
                    {
                        PayResult res = payment.Notify(form, query, header,
                            body, notifyIp);

                        if (res.Status == PayResult.EStatus.Completed)
                            OrderHelper.Pay(res.OrderID, res.Amount, res.TxnID);
                        else
                            Global.FileSave(Config.AppData + "SignError/" +
                                            DateTime.Now.ToString("yyMMddHHmmssfff") + ".log", "OrderName:" +
                                res.OrderName + "; Amount=" + res.Amount
                                + "&Currency=" + res.Currency
                                + "&OrderID=" + res.OrderID
                                + "&PayName=" + payname
                                + "&ClientIp=" + notifyIp + "&Message=" + res.Message +
                                Environment.NewLine, false);

                        msg = res.Message;
                    }
                }
                catch
                {
                    Global.FileSave(Config.AppData + "NotifyBodyError/" +
                                    DateTime.Now.ToString("yyMMddHHmmssfff") + ".log", "Body:" + body, false);
                }

                return msg;
            }
        }

        public static class OrderHelper
        {
            // public static PayTicket GetPayTicket(string PaymentType, string OrderId, double Amount, IPAddress clientIP)
            // {
            //     EPaymentType ePaymentType =
            //         Enum.GetValues<EPaymentType>().FirstOrDefault(p => p.ToString() == PaymentType);
            //     IPayChannel payment = GetPayment(ePaymentType);
            //     if (payment == null) return null;
            //
            //     var notifyurl = "http://pay.gamemakesmoney.com/paynotify/tinystorecore/" + payment.Name;
            //     var returnurl = "http://store.gamemakesmoney.com/order_" + OrderId;
            //
            //     ServicePointManager.SecurityProtocol = (SecurityProtocolType) 3072;
            //
            //     PayTicket payticket = (payment as IPay).Pay(OrderId, Amount, ECurrency.CNY, "订单付款", clientIP,
            //         //order.Url, Config.config.SiteDomain.TrimEnd('/') + "/api/" + order.PaymentType, Config.config.SiteDomain, "");
            //         returnurl, notifyurl, Config.SiteDomain, "");
            //     return payticket;
            // }
            //
            // public static PayTicket GetPayTicket(EPaymentType ePaymentType, string OrderId, double Amount,
            //     IPAddress clientIP)
            // {
            //     IPayChannel payment = GetPayment(ePaymentType);
            //     if (payment == null) return null;
            //
            //     var notifyurl = "https://" + Config.SiteDomain + "/PayNotify/" + payment.Name;
            //     var returnurl = "http://store.gamemakesmoney.com/order_" + OrderId;
            //
            //     ServicePointManager.SecurityProtocol = (SecurityProtocolType) 3072;
            //
            //     PayTicket payticket = (payment as IPay).Pay(OrderId, Amount, ECurrency.CNY, "订单付款", clientIP,
            //         //order.Url, Config.config.SiteDomain.TrimEnd('/') + "/api/" + order.PaymentType, Config.config.SiteDomain, "");
            //         returnurl, notifyurl, Config.SiteDomain, "");
            //     return payticket;
            // }
            //
            //
            //
            // private static IPay GetPayment(string name)
            // {
            //     var strs = name.Split('|');
            //     if (strs.Length == 3)
            //     {
            //         try
            //         {
            //             var platform = Enum.GetValues<EPlatform>().First(p => p.ToString() == strs[0]);
            //             var channel = Enum.GetValues<EChannel>().First(p => p.ToString() == strs[1]);
            //             var paytype = Enum.GetValues<EPayType>().First(p => p.ToString() == strs[2]);
            //             IPayChannel pay = Context.Get(platform, channel, paytype);
            //             
            //             if (pay.Name.StartsWith("alipay", StringComparison.OrdinalIgnoreCase))
            //             {
            //                 if (Config.AliPaySettings != null)
            //                 {
            //                     foreach (var set in Config.AliPaySettings) pay[set.Key] = set.Value;
            //
            //                     return pay as IPay;
            //                 }
            //             }
            //             else if (pay.Name.StartsWith("wechat", StringComparison.OrdinalIgnoreCase))
            //             {
            //                 if (Config.WechatPaySettings != null)
            //                 {
            //                     foreach (var set in Config.WechatPaySettings) pay[set.Key] = set.Value;
            //
            //                     return pay as IPay;
            //                 }
            //             }
            //         }
            //         catch (Exception e)
            //         {
            //         }
            //     }
            //     return null;
            // }


            internal static void Pay(string orderid, double incomme, string txnId)
            {
                if (MemoryCacher.Get(orderid) == null)
                {
                    MemoryCacher.Set(orderid, orderid, MemoryCacher.CacheItemPriority.Normal, null,
                        TimeSpan.FromMinutes(1));
                    OrderModel order = OrderBLL.QueryModelByOrderId(orderid);
                    if (order != null)
                    {
                        if (!order.IsPay && string.Equals(
                            (order.Amount * order.Quantity).ToString("f2"), incomme.ToString("f2"),
                            StringComparison.OrdinalIgnoreCase))
                        {
                            order.TranId = txnId;
                            order.IsPay = true;
                            order.PaymentDate = DateTime.Now;
                            order.PaymentFee = order.Amount * order.Quantity * Config.SysPaymentRate;
                            order.LastUpdateDate = DateTime.Now;
                            OrderBLL.Update(order);

                            BillBLL.Insert(new BillModel
                            {
                                BillId = Global.Generator.DateId(1),
                                UserId = order.UserId,
                                Amount = order.Amount,
                                AmountCharge = 0,
                                BillType = EBillType.收款,
                                CreateDate = DateTime.Now
                            });
                        }

                        if (!order.IsDelivery) Delivery(order);
                    }
                }
            }

            public static void Delivery(OrderModel order)
            {
                StoreModel store = StoreBLL.QueryModelByStoreId(order.StoreId);
                if (store != null)
                {
                    SupplyModel supply = string.IsNullOrWhiteSpace(order.SupplyId)
                        ? null
                        : SupplyBLL.QueryModelById(order.SupplyId);

                    
                    //货源是否为系统货源
                    var supplyUserIdSys = (supply != null && supply.UserId == Config.SupplyUserIdSys);
                    //货源成本是否可以支付
                    var supplyCostVilidate = true;
                    UserExtendModel user = UserExtendBLL.QueryModelById(order.UserId);
                    
                    if (supplyUserIdSys)
                    {
                        if (user.Amount + user.AmountCharge < order.Cost * order.Quantity) 
                            supplyCostVilidate = false;
                    }

                    if (supplyCostVilidate)
                    {
                        ProductModel product =
                            ProductBLL.QueryModelByProductIdAndStoreId(order.ProductId, order.StoreId);
                        var liststock = new List<StockOrder>();
                        if (product != null)
                        {
                            #region 卡密

                            if (product.DeliveryType == EDeliveryType.卡密)
                            {
                                var stocklist =
                                    StockBLL.QueryListBySupplyIdCanUse(product.SupplyId,
                                        product.UserId);
                                if (stocklist.Count >= order.Quantity)
                                {
                                    for (var i = 0; i < order.Quantity; i++)
                                    {
                                        stocklist[i].IsDelivery = true;
                                        stocklist[i].DeliveryDate = DateTime.Now;
                                    }

                                    StockBLL.UpdateRange(stocklist.Where(p => p.IsDelivery).ToList());
                                    foreach (StockModel item in stocklist.Where(p => p.IsDelivery))
                                        liststock.Add(new StockOrder
                                        {
                                            StockId = item.StockId,
                                            Name = item.Name
                                        });

                                    order.StockList = liststock;
                                    //order.DeliveryMessage = string.Empty;
                                    order.IsDelivery = true;
                                    order.DeliveryDate = DateTime.Now;
                                    order.LastUpdateDate = DateTime.Now;

                                    OrderBLL.Update(order);

                                    if (supplyUserIdSys)
                                    {
                                        double amountChange = 0,amountChargeChange = 0;
                                        if (user.AmountCharge >= order.Cost * order.Quantity)
                                        {
                                            amountChargeChange = order.Cost * order.Quantity;
                                        }
                                        else
                                        {
                                            amountChargeChange = user.AmountCharge;
                                            amountChange = order.Cost * order.Quantity - user.AmountCharge;
                                        }

                                        UserExtendBLL.Update(p => p.UserId == order.UserId,
                                            p => new UserExtendModel
                                            {
                                                Amount = p.Amount - amountChange,
                                                AmountCharge = p.AmountCharge - amountChargeChange
                                            });

                                        BillBLL.Insert(new BillModel
                                        {
                                            BillId = Global.Generator.DateId(1),
                                            UserId = order.UserId,
                                            Amount = -amountChange,
                                            AmountCharge = -amountChargeChange,
                                            BillType = EBillType.成本结算,
                                            CreateDate = DateTime.Now
                                        });
                                    }
                                }

                                //else
                                //{
                                //    order.DeliveryMessage = "十分抱歉，你购买的卡密暂时无货，请联系商家解决！";
                                //}
                            }

                            #endregion

                            #region 接口

                            else if (product.DeliveryType == EDeliveryType.接口)
                            {
                            }
                            else if (product.DeliveryType == EDeliveryType.人工)
                            {
                            }

                            #endregion
                            
                            DeliveryEmail(order);
                        }
                    }
                }
            }

            public static void DeliveryEmail(OrderModel order)
            {
                StoreModel store = StoreBLL.QueryModelByStoreId(order.StoreId);
                if (store != null)
                {
                    //var msg_email = SiteContext.Email.TemplateGet("DeliveryEmail");
                    //todo
                    var msg_email = Email.EmailTemplates["DeliveryEmail"].Content;

                    msg_email = msg_email.Replace("{SiteName}", store.Name);
                    msg_email = msg_email.Replace("{DeliveryDate}", order.DeliveryDate?.ToString("yyyy年MM月dd日"));
                    msg_email = msg_email.Replace("{Name}", order.Name);
                    msg_email = msg_email.Replace("{OrderUrl}", "//" + Config.SiteDomain + "/o/" + order.OrderId);
                    msg_email = msg_email.Replace("{OrderId}", order.OrderId);
                    msg_email = msg_email.Replace("{Amount}", (order.Amount * order.Quantity).ToString("f2"));
                    msg_email = msg_email.Replace("{StoreQQ}", store.QQ);
                    var addtemplate = string.Empty;
                    if (order.StockList.Count > 0)
                    {
                        var emailadd = string.Empty;
                        var index = 1;
                        foreach (StockOrder item in order.StockList)
                        {
                            emailadd = Email.EmailTemplates["DeliveryEmail_Stock"].Content;

                            emailadd = emailadd.Replace("{Index}", index.ToString());
                            emailadd = emailadd.Replace("{CardName}", item.Name);
                            addtemplate += emailadd;
                            index++;
                        }
                    }
                    else
                    {
                        addtemplate = File.ReadAllText(Config.AppData + "emails/DeliveryEmail_NoStock.htm");
                    }

                    msg_email = msg_email.Replace("{AddTemplate}", addtemplate);

                    // var product = BLL.ProductBLL.QueryModelByProductIdAndStoreId(order.ProductId, order.StoreId);
                    // if (order.Contact.Contains("@"))
                    // {
                    //     SiteContext.Email.Send(order.Contact,
                    //         store.Name + " 已收到你的订单（" + order.OrderId + "），欢迎您再次购买！", msg_email);
                    // }
                    //
                    // msg_email = "订单号:" + order.OrderId + "<br/><br/>";
                    // msg_email += "订单状态:" + order.State.ToString() + "<br/><br/>";
                    // msg_email += "商品名称:" + order.Name + "<br/><br/>";
                    // if (product != null)
                    //     msg_email += "商品单价:￥" + product.Amount.ToString("f2") + "<br/><br/>";
                    // if (order.Quantity > 0)
                    //     msg_email += "售出份数:" + order.Quantity + "<br/><br/>";
                    // msg_email += "销售金额:￥" + order.Amount.ToString("f2") + "<br/><br/>";
                    // msg_email += "买家" + (order.Contact.Contains("@") ? "邮箱" : "手机号") + ":" +
                    //              order.Contact + "<br/><br/>";
                    // msg_email += "买家联系方式:" + order.Contact + "<br/><br/>";
                    // if (order.StockList.Count > 0)
                    // {
                    //     msg_email += "买家购买的卡号及卡密如下：<br/><br/>";
                    //     var index = 1;
                    //     foreach (var item in order.StockList)
                    //     {
                    //         msg_email += $"卡密{index}：{item.Name}<br/>";
                    //     }
                    // }

                    Email.Send(store.Email, "您有一笔付款通知（" + order.OrderId + "），请尽快处理", msg_email);
                }
            }


            private class PayNotify
            {
                public Dictionary<string, string> Form { get; set; }

                public Dictionary<string, string> Query { get; set; }

                public Dictionary<string, string> Header { get; set; }

                public string Body { get; set; }

                public string NotifyIp { get; set; }

                public string PaymentName { get; set; }
            }

            //人工发货
            // public static ApiResult DeliveryByHand(string storeid, string orderid, string code,
            //     List<Model.Extend.StockOrder> list)
            // {
            //     var order = BLL.OrderBLL.QueryModelByOrderIdAndStoreId(orderid, storeid);
            //     var store = order == null ? null : BLL.StoreBLL.QueryModelByStoreId(order.StoreId);
            //     if (order == null || store == null || !order.IsPay || list.Count != order.Quantity)
            //         return new ApiResult("订单不存在");
            //     if (order.IsDelivery)
            //         return new ApiResult("订单已发货，请勿重复发货");
            //
            //     order.IsDelivery = true;
            //     order.DeliveryDate = DateTime.Now;
            //     order.StockList = list;
            //     order.LastUpdateDate = DateTime.Now;
            //     BLL.OrderBLL.Update(order);
            //     DeliveryEmail(order);
            //     return new ApiResult();
            // }
            //
            // public static ApiResult StateIsChange(string storeid, string orderid)
            // {
            //     var order = BLL.OrderBLL.QueryModelByOrderIdAndStoreId(orderid, storeid);
            //     if (order != null && order.IsPay)
            //         return new ApiResult(SiteContext.Url.OrderInfo(order.OrderId), ApiResult.ECode.Success);
            //     return new ApiResult(ApiResult.ECode.Fail);
            // }
        }
    }
}