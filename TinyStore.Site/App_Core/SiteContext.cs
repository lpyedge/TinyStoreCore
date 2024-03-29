﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using IP2Region;
using TinyStore.Model;

namespace TinyStore.Site
{
    public class SiteContext
    {
        public static ConfigModel Config { get; set; }
        
        public static void ConfigSave()
        {
            var configJson = Utils.JsonUtility.SerializePretty(new {Config});
            File.WriteAllText(Config.AppData + "Config.json", configJson);
        }

        /// <summary>
        ///     初始化
        /// </summary>
        public static void Inited(Microsoft.Extensions.DependencyInjection.ServiceProvider services, Microsoft.Extensions.Configuration.IConfiguration configuration)
        {
            // var ConnStr1 = configuration.GetSection("Config:ConnStr").Value;
            // var ConnStr2 = configuration.GetSection("Config:ConnStr").Get<string>();
            // configuration.GetSection("Config").Get<ConfigModel>();
            //Global.AppService.Configuration.GetSection("Config").Bind(Config);

            Config = Microsoft.Extensions.Configuration.ConfigurationBinder.Get<ConfigModel>(configuration.GetSection("Config"));

            if (Config == null)
            {
                Config = new ConfigModel();
                ConfigSave();
            }

            BLL.BaseBLL.Init(SqlSugar.DbType.Sqlite, Config.AppData + "Data.db");


            if (!File.Exists(Config.AppData + "Data.db"))
            {
                BLL.BaseBLL.InitDataBase();
                BLL.BaseBLL.InitDataTables();

#if DEBUG
                InitDevData();
#endif
            }

            if (BLL.AdminBLL.QueryCount(p => p.IsRoot) == 0)
                BLL.AdminBLL.Insert(new AdminModel
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
            BLL.UserBLL.InsertAsync(new UserModel
            {
                Account = "test",
                Password = Global.Hash("test", "test"),
                Salt = "test",
                ClientKey = "test"
            });
            BLL.UserExtendBLL.InsertAsync(new UserExtendModel
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
            var storeId = "StoreId";
            BLL.StoreBLL.InsertAsync(new StoreModel
            {
                UserId = 1,
                Email = "test@test.com",
                Name = "测试网店",
                Initial = Global.Initial("测试网店"),
                Logo = "#",
                Memo = "自家供货，自家销售",
                Template = EStoreTemplate.模板一,
                IsSingle = true,
                PaymentList = new List<Model.PaymentView>(Payment.SystemPaymentList())
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
                UniqueId = "store0",
                StoreId = storeId
            });
            BLL.StoreBLL.InsertAsync(new StoreModel
            {
                UserId = 1,
                Email = "test@test.com",
                Name = "一号网店",
                Initial = Global.Initial("一号网店"),
                Logo = "#",
                Memo = "自家供货，自家销售",
                Template = EStoreTemplate.模板一,
                IsSingle = true,
                PaymentList = new List<Model.PaymentView>(Payment.SystemPaymentList())
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
                UniqueId = "store1",
                StoreId = storeId+"1"
            });
            BLL.StoreBLL.InsertAsync(new StoreModel
            {
                UserId = 1,
                Email = "test@test.com",
                Name = "二号网店",
                Initial = Global.Initial("二号网店"),
                Logo = "#",
                Memo = "自家供货，自家销售",
                Template = EStoreTemplate.模板二,
                IsSingle = false,
                PaymentList = new List<Model.PaymentView>(Payment.SystemPaymentList())
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
                UniqueId = "store2",
                StoreId = storeId + "2"
            });
            BLL.StoreBLL.InsertAsync(new StoreModel
            {
                UserId = 1,
                Email = "test@test.com",
                Name = "三号网店",
                Initial = Global.Initial("三号网店"),
                Logo = "#",
                Memo = "自家供货，自家销售",
                Template = EStoreTemplate.模板三,
                IsSingle = false,
                PaymentList = new List<Model.PaymentView>(Payment.SystemPaymentList())
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
                UniqueId = "store3",
                StoreId = storeId + "3"
            });
            BLL.StoreBLL.InsertAsync(new StoreModel
            {
                UserId = 1,
                Email = "test@test.com",
                Name = "四号网店",
                Initial = Global.Initial("四号网店"),
                Logo = "#",
                Memo = "自家供货，自家销售",
                Template = EStoreTemplate.模板四,
                IsSingle = false,
                PaymentList = new List<Model.PaymentView>(Payment.SystemPaymentList())
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
                UniqueId = "store4",
                StoreId = storeId + "4"
            });
            BLL.StoreBLL.InsertAsync(new StoreModel
            {
                UserId = 1,
                Email = "test@test.com",
                Name = "五号网店",
                Initial = Global.Initial("五号网店"),
                Logo = "#",
                Memo = "自家供货，自家销售",
                Template = EStoreTemplate.模板五,
                IsSingle = false,
                PaymentList = new List<Model.PaymentView>(Payment.SystemPaymentList())
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
                UniqueId = "store5",
                StoreId = storeId + "5"
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
            BLL.SupplyBLL.InsertRangeAsync(supplyList);
            for (var i = 0; i < 100; i++)
                BLL.StockBLL.InsertAsync(new StockModel
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
            BLL.ProductBLL.InsertRangeAsync(productList);

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
                StockList = new List<Model.StockOrderView>(),


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
                        StockList = new List<Model.StockOrderView>(),

                        RefundAmount = isrefund ? productModel.Cost * 0.3 : 0,
                        RefundDate = isrefund ? DateTime.Now.AddDays(-i) : null,


                        LastUpdateDate = DateTime.Now.AddDays(-i),
                        NotifyDate = null
                    });
                }

            BLL.OrderBLL.InsertRangeAsync(orderList);

            var billList = new List<BillModel>();
            var billTypes = Enum.GetValues<EBillType>().ToList();

            for (var i = 0; i < 50; i++)
            for (var j = 0; j < Global.Generator.Random.Next(3, 40); j++)
            {
                var ischarge = Global.Generator.Random.NextDouble() > 0.7;
                var isplus = Global.Generator.Random.NextDouble() > 0.4;
                var amount = Global.Generator.Random.Next(1, 30);
                EBillType billType = billTypes[Global.Generator.Random.Next(0, 5)];
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

            BLL.BillBLL.InsertRangeAsync(billList);


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

            BLL.WithDrawBLL.InsertRangeAsync(withDrawList);
        }

        public class IP2Region
        {
            private string[] dblist = new[]
            {
                "https://github.com/lionsoul2014/ip2region/raw/master/data/ip2region.db",
                "https://raw.githubusercontent.com/lionsoul2014/ip2region/master/data/ip2region.db",
                "https://hub.fastgit.xyz/lionsoul2014/ip2region/raw/master/data/ip2region.db",
                "https://ghproxy.com/https://raw.githubusercontent.com/lionsoul2014/ip2region/master/data/ip2region.db",
            };
            
            public static string Version()
            {
                FileInfo dbFileInfo = new FileInfo(Config.AppData + "ip2region.db");
                if (dbFileInfo.Exists)
                {
                    return dbFileInfo.LastWriteTimeUtc.ToLongDateString();
                }

                return "数据文件不存在";
            }
            
            public static string Search(string ip)
            {
                try
                {
                    //ip = "49.82.194.75";
                    using (var searcher = new DbSearcher(Config.AppData + "ip2region.db"))
                    {
                        return searcher.BtreeSearch(ip).Region;
                    }
                }
                catch (Exception e)
                {
                    return "";
                }
            }
        }

        public class ConfigModel
        {
            [System.Text.Json.Serialization.JsonIgnore] public int SupplyUserIdSys => 0;

            [System.Text.Json.Serialization.JsonIgnore] public double SysPaymentRate => 0.006;

            [System.Text.Json.Serialization.JsonIgnore] public string FormatDate => "yyyy-MM-dd";

            [System.Text.Json.Serialization.JsonIgnore] public string FormatDateTime => "yyyy-MM-dd HH:mm";

            [System.Text.Json.Serialization.JsonIgnore] public string AppData => AppDomain.CurrentDomain.BaseDirectory + "App_Data/";

            [System.Text.Json.Serialization.JsonIgnore] public string UserData => AppDomain.CurrentDomain.BaseDirectory + "User_Data/";


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
            public Utils.EmailContext.EmailServer EmailServer { get; set; } = new();


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
                    var result = new Microsoft.AspNetCore.Mvc.FileContentResult(buffer, contenttype)
                        {EnableRangeProcessing = true};
                    Utils.MemoryCacher.Set(tempuripath, result, Utils.MemoryCacher.CacheItemPriority.Normal,
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
                            Utils.MemoryCacher.Remove(tempuripath);
                            Utils.MemoryCacher.Remove(uripath);
                        }

                        return uripath;
                    }
                }
                catch (Exception ex)
                {
                }

                return tempuripath;
            }

            public static Microsoft.AspNetCore.Mvc.ActionResult Result(string Model, string Id, string Name,
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

                    Microsoft.AspNetCore.Mvc.FileContentResult result = null;
                    if (!Utils.MemoryCacher.TryGet(uripath, out result))
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
                                result = new Microsoft.AspNetCore.Mvc.FileContentResult(contentbytes, contenttype)
                                    {LastModified = file.LastWriteTime, EnableRangeProcessing = true};
                                Utils.MemoryCacher.Set(uripath, result, Utils.MemoryCacher.CacheItemPriority.Normal,
                                    DateTime.Now.AddMinutes(5));
                            }
                    }

                    if (result != null) return result;
                }

                return new Microsoft.AspNetCore.Mvc.NotFoundResult();
            }
        }

        public static class Email
        {
            private static RazorEngineCore.IRazorEngine _razorEngine = new RazorEngineCore.RazorEngine();
            private static ConcurrentDictionary<string, RazorEngineCore.IRazorEngineCompiledTemplate> _emailTemplates = new ();
            
            static Email()
            {
                Utils.EmailContext.EmailServer.Instances["default"] = Config.EmailServer;

                foreach (var filePath in Directory.GetFiles(Config.AppData + "EmailTemplate/","*.html"))
                    try
                    {
                        var file = new FileInfo(filePath);
                        _emailTemplates[file.Name.Substring(0,file.Name.LastIndexOf('.'))] = _razorEngine.Compile(File.ReadAllText(file.FullName));
                    }
                    catch (Exception e)
                    {
                    }
            }

            public static string TemplateRender(string templateName, dynamic model)
            {
                if (_emailTemplates.ContainsKey(templateName))
                {
                    return _emailTemplates[templateName].Run(model);
                }

                return null;
            }

            public static void Send(string email, string subject, string content, bool isContentHtml = true)
            {
                var mailMessage = new System.Net.Mail.MailMessage
                {
                    Subject = subject,
                    Body = content,
                    IsBodyHtml = isContentHtml,
                    BodyEncoding = Encoding.UTF8,
                    SubjectEncoding = Encoding.UTF8,
                    DeliveryNotificationOptions = System.Net.Mail.DeliveryNotificationOptions.OnSuccess
                };
                mailMessage.To.Add(email);
                Send(mailMessage);
            }

            public static void Send(System.Net.Mail.MailMessage p_MailMessage)
            {
                if(!string.IsNullOrWhiteSpace(Utils.EmailContext.EmailServer.Instances["default"].Host ) &&
                   !string.IsNullOrWhiteSpace(Utils.EmailContext.EmailServer.Instances["default"].Username ) &&
                   !string.IsNullOrWhiteSpace(Utils.EmailContext.EmailServer.Instances["default"].Password ) &&
                   Utils.EmailContext.EmailServer.Instances["default"].Port > 0)
                {
                    Utils.EmailContext.SendMailAsync(
                        Utils.EmailContext.EmailServer.Instances["default"], 
                        p_MailMessage);
                }
            }
        }

        public static class Payment
        {
            private static readonly object _Locker = new();

            private static List<Model.PaymentView> _SystemPaymentList;

            public static List<Model.PaymentView> SystemPaymentList()
            {
                if (_SystemPaymentList == null)
                    lock (_Locker)
                    {
                        if (_SystemPaymentList == null)
                        {
                            _SystemPaymentList = new List<Model.PaymentView>();
                            _SystemPaymentList.Add(new Model.PaymentView
                            {
                                BankType = EBankType.支付宝,
                                Subject = "支付宝H5",
                                Name = Payments.EPlatform.AliPay +"_"+Payments.EChannel.AliPay+"_"+Payments.EPayType.H5,
                                Account = "",
                                Memo = "手机端调用",
                                Rate = Config.SysPaymentRate,
                                IsSystem = true,
                                IsEnable = false
                            });
                            _SystemPaymentList.Add(new Model.PaymentView
                            {
                                BankType = EBankType.支付宝,
                                Subject = "支付宝扫码",
                                Name = Payments.EPlatform.AliPay +"_"+Payments.EChannel.AliPay+"_"+Payments.EPayType.QRCode,
                                Account = "",
                                Memo = "电脑端调用",
                                Rate = Config.SysPaymentRate,
                                IsSystem = true,
                                IsEnable = false
                            });
                            _SystemPaymentList.Add(new Model.PaymentView
                            {
                                BankType = EBankType.支付宝,
                                Subject = "支付宝网关",
                                Name = Payments.EPlatform.AliPay +"_"+Payments.EChannel.AliPay+"_"+Payments.EPayType.PC,
                                Account = "",
                                Memo = "电脑端调用",
                                Rate = Config.SysPaymentRate,
                                IsSystem = true,
                                IsEnable = false
                            });
                            _SystemPaymentList.Add(new Model.PaymentView
                            {
                                BankType = EBankType.微信,
                                Subject = "微信H5",
                                Name = Payments.EPlatform.WeChat +"_"+Payments.EChannel.WeChat+"_"+Payments.EPayType.H5,
                                Account = "",
                                Memo = "手机端调用",
                                Rate = Config.SysPaymentRate,
                                IsSystem = true,
                                IsEnable = false
                            });
                            _SystemPaymentList.Add(new Model.PaymentView
                            {
                                BankType = EBankType.微信,
                                Subject = "微信扫码",
                                Name = Payments.EPlatform.WeChat +"_"+Payments.EChannel.WeChat+"_"+Payments.EPayType.QRCode,
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

            public static string TransferToBank(Model.PaymentView payment, double amount = 0)
            {
                if ((int) payment.BankType >= 10)
                {
                    Global.AlipaySchema.EBankMark bankMark = Utils.Reflection.Attribute
                        .GetCustomAttribute<BankMarkAttribute>(payment.BankType).First().BankMark;
                    return Global.AlipaySchema.ToBankCard(bankMark, payment.Account, payment.Name, amount);
                }

                return null;
            }


          
            public static Payments.IPayChannel GetPayment(string name)
            {
                //PayName2Enum(name, out EPlatform platform, out EChannel channel, out EPayType payType);
                Payments.IPayChannel payment = Payments.Context.Get(name);

                if (payment.Platform == Payments.EPlatform.AliPay)
                {
                    if (Config.AliPaySettings != null)
                    {
                        foreach (var set in Config.AliPaySettings) payment[set.Key] = set.Value;

                        return payment;
                    }
                }
                else if (payment.Platform == Payments.EPlatform.WeChat)
                {
                    if (Config.WechatPaySettings != null)
                    {
                        foreach (var set in Config.WechatPaySettings) payment[set.Key] = set.Value;

                        return payment;
                    }
                }

                return null;
            }

            public static bool Notify(string payname, IDictionary<string, string> form,
                IDictionary<string, string> query, IDictionary<string, string> header, string body, string notifyIp,out string msg)
            {
                msg = "";
                try
                {
                    var payment = GetPayment(payname);

                    if (payment != null)
                    {
                        Payments.PayResult res = (payment as Payments.IPay).Notify(form, query, header,
                            body, notifyIp);

                        if (res.Status == Payments.PayResult.EStatus.Completed)
                        {
                            //发起请求payOrderId会生成 _xx 后缀，防止同一支付平台不允许订单重复
                            var payOrderId = res.OrderID.Split("_")[0];
                            OrderModel order = BLL.OrderBLL.QueryModelByPayOrderId(payOrderId);
                            if (order != null)
                            {
                                var user = BLL.BaseBLL<UserExtendModel>.QueryModelById(order.UserId);
                                if (user != null && !order.IsPay && string.Equals(
                                        order.PayAmount.ToString("f2"), res.Amount.ToString("f2"),
                                        StringComparison.OrdinalIgnoreCase))
                                {
                                    order.TranId = res.TxnID;
                                    order.IsPay = true;
                                    order.PaymentDate = DateTime.Now;
                                    order.PaymentFee = res.Amount * Config.SysPaymentRate;
                                    order.LastUpdateDate = DateTime.Now;
                                    
                                    BLL.OrderBLL.Update(order);

                                    var income = res.Amount - order.PaymentFee;

                                    BLL.BillBLL.Insert(new BillModel
                                    {
                                        BillId = Global.Generator.DateId(1),
                                        UserId = order.UserId,
                                        Amount = income,
                                        AmountCharge = 0,
                                        BillType = EBillType.收款,
                                        CreateDate = DateTime.Now
                                    });

                                    BLL.UserExtendBLL.Update(p => p.UserId == order.UserId,
                                        p => new UserExtendModel() {Amount = p.Amount + income});

                                    if (!order.IsDelivery)
                                    {
                                        OrderHelper.Delivery(order);
                                    }
                                }
                            }
                            
                            return true;
                        }
                        else
                            Global.FileSave(Config.AppData + "Error/Sign/" +
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
                catch(Exception ex)
                {
                    Global.FileSave(Config.AppData + "Error/Notify/" +
                                    DateTime.Now.ToString("yyMMddHHmmssfff") + ".log", ex.Message, false);
                    
                }

                return false;
            }
        }

        public static class OrderHelper
        {            
            internal static void Pay(string orderid, double incomme, string txnId)
            {
                if (Utils.MemoryCacher.Get(orderid) == null)
                {
                    Utils.MemoryCacher.Set(orderid, orderid, Utils.MemoryCacher.CacheItemPriority.Normal, null,
                        TimeSpan.FromMinutes(1));
                }
            }

            public static void Delivery(OrderModel order)
            {
                StoreModel store = BLL.StoreBLL.QueryModelByStoreId(order.StoreId);
                if (store != null)
                {
                    SupplyModel supply = string.IsNullOrWhiteSpace(order.SupplyId)
                        ? null
                        : BLL.SupplyBLL.QueryModelById(order.SupplyId);


                    //货源是否为系统货源
                    var supplyUserIdSys = (supply != null && supply.UserId == Config.SupplyUserIdSys);
                    //货源成本是否可以支付
                    var supplyCostVilidate = true;
                    UserExtendModel user = BLL.BaseBLL<UserExtendModel>.QueryModelById(order.UserId);

                    if (supplyUserIdSys)
                    {
                        if (user.Amount + user.AmountCharge < order.Cost * order.Quantity)
                            supplyCostVilidate = false;
                    }

                    if (supplyCostVilidate)
                    {
                        ProductModel product =
                            BLL.ProductBLL.QueryModelByProductId(order.ProductId);
                        var liststock = new List<Model.StockOrderView>();
                        if (product != null)
                        {
                            #region 卡密

                            if (product.DeliveryType == EDeliveryType.卡密)
                            {
                                var stocklist =
                                    BLL.StockBLL.QueryListBySupplyIdCanUse(product.SupplyId,
                                        product.UserId);
                                if (stocklist.Count >= order.Quantity)
                                {
                                    for (var i = 0; i < order.Quantity; i++)
                                    {
                                        stocklist[i].IsDelivery = true;
                                        stocklist[i].DeliveryDate = DateTime.Now;
                                    }

                                    BLL.StockBLL.UpdateRange(stocklist.Where(p => p.IsDelivery).ToList());
                                    foreach (StockModel item in stocklist.Where(p => p.IsDelivery))
                                        liststock.Add(new Model.StockOrderView
                                        {
                                            StockId = item.StockId,
                                            Name = item.Name
                                        });

                                    order.StockList = liststock;
                                    //order.DeliveryMessage = string.Empty;
                                    order.IsDelivery = true;
                                    order.DeliveryDate = DateTime.Now;
                                    order.LastUpdateDate = DateTime.Now;

                                    BLL.OrderBLL.Update(order);

                                    if (supplyUserIdSys)
                                    {
                                        double amountChange = 0, amountChargeChange = 0;
                                        if (user.AmountCharge >= order.Cost * order.Quantity)
                                        {
                                            amountChargeChange = order.Cost * order.Quantity;
                                        }
                                        else
                                        {
                                            amountChargeChange = user.AmountCharge;
                                            amountChange = order.Cost * order.Quantity - user.AmountCharge;
                                        }

                                        BLL.UserExtendBLL.Update(p => p.UserId == order.UserId,
                                            p => new UserExtendModel
                                            {
                                                Amount = p.Amount - amountChange,
                                                AmountCharge = p.AmountCharge - amountChargeChange
                                            });

                                        BLL.BillBLL.Insert(new BillModel
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
                                else
                                {
                                    //卡密方式 支付成功且货源卡密不足时自动发邮件给商户
                                    Email_OrderStoreNotify(order);
                                }
                                
                                //卡密方式 支付成功时自动发邮件给客户
                                Email_OrderDelivery(order);
                            }

                            #endregion

                            #region 接口

                            else if (product.DeliveryType == EDeliveryType.接口)
                            {
                                //todo ? 未实现
                            }
                            else if (product.DeliveryType == EDeliveryType.人工)
                            {
                                //人工方式 支付成功时自动发邮件给商户
                                Email_OrderStoreNotify(order);
                            }

                            #endregion

                        }
                    }
                }
            }

            public static void Email_OrderDelivery(OrderModel order)
            {
                StoreModel store = BLL.StoreBLL.QueryModelByStoreId(order.StoreId);
                if (store != null)
                {
                    var model = new
                    {
                        StoreName = store.Name,
                        StoreUrl = "http://" + Config.SiteDomain + "/s/" + store.UniqueId,
                        StoreLogo = "http://" + Config.SiteDomain + store.Logo,
                        QQ = store.QQ,
                        
                        UserUrl = "http://" + Config.SiteDomain + "/user/",

                        OrderId = order.OrderId,
                        ProductName = order.Name,
                        OrderUrl = "http://" + Config.SiteDomain + "/o/" + order.OrderId,
                        CreateDate = order.CreateDate.ToString("yyyy年MM月dd日 HH时mm分"),

                        Quantity = order.Quantity,
                        Amount = order.Amount,
                        Reduction = order.Reduction, //.ToString("f2"),

                        StockList = order.StockList,
                    };

                    // model.StockList.Add(new StockOrderView(){Name = "你好1"});
                    // model.StockList.Add(new StockOrderView(){Name = "你好2"});
                    // model.StockList.Add(new StockOrderView(){Name = "你好3"});
                    // model.StockList.Add(new StockOrderView(){Name = "你好4"});
                    var mailContent = Email.TemplateRender("OrderDelivery", model);
                    Email.Send(order.Contact, $"您购买了商品[{order.Name}] - {store.Name}", mailContent);
                }
            }


            public static void Email_OrderStoreNotify(OrderModel order)
            {
                try
                {
                    StoreModel store = BLL.StoreBLL.QueryModelByStoreId(order.StoreId);
                    if (store != null )
                    {
                        var model = new
                        {
                            StoreName = store.Name,
                            StoreUrl = "http://" + Config.SiteDomain + "/s/" + store.UniqueId,
                            StoreLogo = "http://" + Config.SiteDomain + store.Logo,
                            QQ = store.QQ,
                        
                            UserUrl = "http://" + Config.SiteDomain + "/user/",

                            OrderId = order.OrderId,
                            ProductName = order.Name,
                            OrderUrl = "http://" + Config.SiteDomain + "/o/" + order.OrderId,
                            CreateDate = order.CreateDate.ToString("yyyy年MM月dd日 HH时mm分"),

                            Quantity = order.Quantity,
                            Amount = order.Amount,
                            Reduction = order.Reduction, //.ToString("f2"),

                            StockList = new List<StockOrderView>(),
                        };

                        var mailContent = Email.TemplateRender("OrderStoreNotify", model);
                        Email.Send(store.Email, $"您售出了商品[{order.Name}] - {store.Name}", mailContent);
                    }
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    throw;
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
            //     List<Model.View.StockOrder> list)
            // {
            //     var order = BLL.BLL.OrderBLL.QueryModelByOrderIdAndStoreId(orderid, storeid);
            //     var store = order == null ? null : BLL.BLL.StoreBLL.QueryModelByStoreId(order.StoreId);
            //     if (order == null || store == null || !order.IsPay || list.Count != order.Quantity)
            //         return new ApiResult("订单不存在");
            //     if (order.IsDelivery)
            //         return new ApiResult("订单已发货，请勿重复发货");
            //
            //     order.IsDelivery = true;
            //     order.DeliveryDate = DateTime.Now;
            //     order.StockList = list;
            //     order.LastUpdateDate = DateTime.Now;
            //     BLL.BLL.OrderBLL.Update(order);
            //     DeliveryEmail(order);
            //     return new ApiResult();
            // }
            //
            // public static ApiResult StateIsChange(string storeid, string orderid)
            // {
            //     var order = BLL.BLL.OrderBLL.QueryModelByOrderIdAndStoreId(orderid, storeid);
            //     if (order != null && order.IsPay)
            //         return new ApiResult(SiteContext.Url.OrderInfo(order.OrderId), ApiResult.ECode.Success);
            //     return new ApiResult(ApiResult.ECode.Fail);
            // }
        }
    }
}