using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TinyStore.Model;
using TinyStore.Model.Extend;

namespace TinyStore.Site
{
    public class SiteContext
    {
        public static ServiceProvider ServiceProvider { get; private set; }
        public static IConfiguration Configuration { get; private set; }

        /// <summary>
        /// HttpContext_Current
        /// </summary>
        public static HttpContext Current
        {
            get
            {
                var factory = (HttpContextAccessor) ServiceProvider?.GetService(typeof(IHttpContextAccessor));
                return factory?.HttpContext;
            }
        }

        /// <summary>
        /// 初始化
        /// </summary>
        /// <param name="services"></param>
        /// <param name="configuration"></param>
        public static void ConfigurationService(IServiceCollection services, IConfiguration configuration)
        {
            ServiceProvider = services.BuildServiceProvider();

            Configuration = configuration;

            // var ConnStr1 = Configuration.GetSection("Config:ConnStr").Value;
            // var ConnStr2 = Configuration.GetSection("Config:ConnStr").Get<string>();
            // Configuration.GetSection("Config").Get<ConfigModel>();

            TinyStore.BLL.BaseBLL.Init(SqlSugar.DbType.Sqlite, SiteContext.Config.AppData + "Data.db");

            if (!File.Exists(SiteContext.Config.AppData + "Data.db"))
            {
                TinyStore.BLL.BaseBLL.DbClient.DbMaintenance.CreateDatabase();
                TinyStore.BLL.BaseBLL.DbClient.CodeFirst.InitTables<Model.AdminModel>();
                TinyStore.BLL.BaseBLL.DbClient.CodeFirst.InitTables<Model.AdminLogModel>();
                TinyStore.BLL.BaseBLL.DbClient.CodeFirst.InitTables<Model.OrderModel>();
                TinyStore.BLL.BaseBLL.DbClient.CodeFirst.InitTables<Model.OrderTrashModel>();
                TinyStore.BLL.BaseBLL.DbClient.CodeFirst.InitTables<Model.ProductModel>();
                TinyStore.BLL.BaseBLL.DbClient.CodeFirst.InitTables<Model.StockModel>();
                TinyStore.BLL.BaseBLL.DbClient.CodeFirst.InitTables<Model.StoreModel>();
                TinyStore.BLL.BaseBLL.DbClient.CodeFirst.InitTables<Model.SupplyModel>();
                TinyStore.BLL.BaseBLL.DbClient.CodeFirst.InitTables<Model.UserModel>();
                TinyStore.BLL.BaseBLL.DbClient.CodeFirst.InitTables<Model.UserExtendModel>();
                TinyStore.BLL.BaseBLL.DbClient.CodeFirst.InitTables<Model.UserLogModel>();
                TinyStore.BLL.BaseBLL.DbClient.CodeFirst.InitTables<Model.WithDrawModel>();


#if DEBUG
                InitDevData();
#endif
            }

            if (BLL.AdminBLL.QueryCount(p => p.IsRoot) == 0)
            {
                BLL.AdminBLL.Insert(new Model.AdminModel
                {
                    Account = "admin",
                    ClientKey = Guid.NewGuid().ToString(),
                    CreateDate = DateTime.Now,
                    IsRoot = true,
                    Password = Global.Hash("admin", "tinystorecore"),
                    Salt = "12345678"
                });
            }
        }

        private static void InitDevData()
        {
            TinyStore.BLL.UserBLL.InsertAsync(new UserModel()
            {
                Account = "test",
                Password = Global.Hash("test", "test"),
                Salt = "test",
                ClientKey = "test",
                Amount = 0,
                AmountCharge = 1000,
                Level = EUserLevel.一星,
            });
            TinyStore.BLL.UserExtendBLL.InsertAsync(new UserExtendModel()
            {
                UserId = 1,
                Email = "test@test.com",
                Name = "姓名",
                RegisterDate = DateTime.Now,
                RegisterIP = "127.0.0.1",
                UserAgent = "",
                AcceptLanguage = "",
                
                BankAccount = "",
                BankType =  EBankType.支付宝,
                BankPersonName = "",
                IdCard = "",
                QQ = "",
                TelPhone = "",
            });
            var storeId = Global.Generator.DateId(2);
            storeId = "StoreId";
            TinyStore.BLL.StoreBLL.InsertAsync(new StoreModel()
            {
                UserId = 1,
                Email = "test@test.com",
                Name = "小网店",
                Initial = Global.Initial("小网店"),
                Logo = "#",
                Memo = "自家供货，自家销售",
                Template = EStoreTemplate.模板一,
                IsSingle = true,
                PaymentList = new List<Payment>(SiteContext.SystemPaymentList())
                {
                    new Payment()
                    {
                        Name = "A先生",
                        Account = "13101010101",
                        IsEnable = true,
                        IsSystem = false,
                        Rate = 0,
                        QRCode = "alipay://xxxxx",
                        Memo = "支付宝shoukuan",
                    }
                },
                UniqueId = "test",
                StoreId = storeId
            });
            BLL.SupplyBLL.InsertAsync(new SupplyModel()
            {
                UserId = SiteContext.Config.SupplyUserIdSys,
                Category = "腾讯",
                Cost = 5,
                Name = "QQ币",
                Memo = "请留下您的QQ号码方便我们来充值",
                DeliveryType = EDeliveryType.人工,
                FaceValue = 10,
                IsShow = true,
                SupplyId = Global.Generator.DateId(2)
            });
            BLL.SupplyBLL.InsertAsync(new SupplyModel()
            {
                UserId = SiteContext.Config.SupplyUserIdSys,
                Category = "腾讯",
                Cost = 100,
                Name = "微信号",
                Memo = "请联系客服购买，标价非真实价格",
                DeliveryType = EDeliveryType.人工,
                FaceValue = 100,
                IsShow = true,
                SupplyId = Global.Generator.DateId(2)
            });
            BLL.SupplyBLL.InsertAsync(new SupplyModel()
            {
                UserId = SiteContext.Config.SupplyUserIdSys,
                Category = "腾讯",
                Cost = 100,
                Name = "QQ号",
                Memo = "请联系客服购买，标价非真实价格",
                DeliveryType = EDeliveryType.人工,
                FaceValue = 100,
                IsShow = true,
                SupplyId = Global.Generator.DateId(2)
            });
            BLL.SupplyBLL.InsertAsync(new SupplyModel()
            {
                UserId = SiteContext.Config.SupplyUserIdSys,
                Category = "网易",
                Cost = 35,
                Name = "魔兽点卡",
                Memo = "请联系客服购买，标价非真实价格",
                DeliveryType = EDeliveryType.卡密,
                FaceValue = 50,
                IsShow = true,
                SupplyId = Global.Generator.DateId(2)
            });
            BLL.SupplyBLL.InsertAsync(new SupplyModel()
            {
                UserId = SiteContext.Config.SupplyUserIdSys,
                Category = "网易",
                Cost = 35,
                Name = "魔兽怀旧服点卡",
                Memo = "请联系客服购买，标价非真实价格",
                DeliveryType = EDeliveryType.卡密,
                FaceValue = 50,
                IsShow = true,
                SupplyId = Global.Generator.DateId(2)
            });
            BLL.SupplyBLL.InsertAsync(new SupplyModel()
            {
                UserId = 1,
                Category = "腾讯",
                Cost = 5,
                Name = "QQ币",
                Memo = "请留下您的QQ号码方便我们来充值",
                DeliveryType = EDeliveryType.人工,
                FaceValue = 10,
                IsShow = true,
                SupplyId = "SupplyId"
            });
            BLL.ProductBLL.InsertAsync(new ProductModel()
            {
                UserId = 1,
                Category = "",
                Cost = 5,
                Name = "QQ币",
                Memo = "请留下您的QQ号码方便我们来充值",
                DeliveryType = EDeliveryType.人工,
                FaceValue = 10,
                IsShow = true,
                SupplyId = "SupplyId",
                ProductId = "ProductId",
                Amount = 9,
                Icon = "#",
                Sort = 0,
                QuantityMin = 1,
                StoreId = storeId
            });
            BLL.OrderBLL.Insert(new OrderModel()
            {
                OrderId = "test",
                StoreId = "StoreId",
                UserId = 1,
                ProductId = "ProductId",
                SupplyId = "SupplyId",
                Amount = 18,
                Quantity = 2,
                Cost = 10,
                Discount = 1,
                Memo = "请留下您的QQ号码方便我们来充值",
                Name = "QQ币",
                AcceptLanguage = "",
                UserAgent = "",
                CreateDate = DateTime.Now,
                IsDelivery = false,
                DeliveryDate = DateTime.Now,

                IsPay = false,
                Income = 0,
                PaymentFee = 0,
                PaymentType = "alipay",
                TranId = "123456",

                ReturnAmount = 0,
                ReturnDate = DateTime.Now,


                IsSettle = false,
                SettleDate = DateTime.Now,
                ClientIP = "127.0.0.1",
                
                //客户输入数据
                NoticeAccount = "test@qq.com",
                Contact = "110110",

                LastUpdateDate = DateTime.Now,
                StockList = new List<StockOrder>()
            });
        }

        public class ConfigModel
        {
            public int SupplyUserIdSys => 0;

            public string AppData => AppDomain.CurrentDomain.BaseDirectory + "App_Data/";

            public string UserData => AppDomain.CurrentDomain.BaseDirectory + "User_Data/";

            /// <summary>
            /// 提现最低金额
            /// </summary>
            public double WithDrawMin { get; set; } = 10;

            /// <summary>
            /// 提现最高金额
            /// </summary>
            public double WithDrawMax { get; set; } = 10000;

            public string SiteDomain { get; set; } = "tiny.store";

            public string SiteName { get; set; } = "TinyStore";

            public string FormatDate { get; set; } = "yyyy-MM-dd";

            public string FormatDateTime { get; set; } = "yyyy-MM-dd HH:mm";

            public Dictionary<EUserLevel, double> TaxConfigList { get; set; }

            public Dictionary<EUserLevel, double> SupplyRates { get; set; }

            public List<KeyValuePair<string, string>> WechatPaySettings { get; set; }

            public List<KeyValuePair<string, string>> AliPaySettings { get; set; }
            
            
            public Utils.EmailContext.EmailServer EmailServer { get; set; }
        }

        public static ConfigModel Config => Configuration.GetSection("Config").Get<ConfigModel>();


        public sealed class RequestInfo
        {
            public RequestInfo()
            {
            }

            public RequestInfo(HttpContext httpContext)
            {
                Get(this, httpContext);
            }

            public System.Net.IPAddress RemoteIP { get; set; }
            public System.Net.IPAddress ClientIP { get; set; }
            public string UserAgent { get; set; }
            public bool IsMobile { get; set; }

            public string ClientOS { get; set; }
            public List<string> ClientLanguages { get; set; }

            private void Get(RequestInfo requestInfo, HttpContext httpContext)
            {
                requestInfo.RemoteIP = httpContext.Connection.RemoteIpAddress;
                requestInfo.ClientIP = _ClientIP(httpContext);
                requestInfo.UserAgent = _UserAgent(httpContext);
                requestInfo.IsMobile = _IsMobile(httpContext);
                requestInfo.ClientOS = _ClientOS(httpContext);
                requestInfo.ClientLanguages = _ClientLanguages(httpContext);
            }


            public static string _UserAgent(HttpContext httpContext)
            {
                if (httpContext != null && httpContext.Request != null &&
                    httpContext.Request.Headers.ContainsKey("User-Agent"))
                {
                    return httpContext.Request.Headers["User-Agent"].ToString();
                }

                return "";
            }

            /// <summary>
            /// 客户IP
            /// </summary>
            /// <param name="httpContext"></param>
            /// <returns></returns>
            public static System.Net.IPAddress _ClientIP(HttpContext httpContext)
            {
                return httpContext.Request != null ? _ClientIP(httpContext.Request) : System.Net.IPAddress.None;
            }

            /// <summary>
            /// 客户IP
            /// </summary>
            /// <param name="httpContext"></param>
            /// <returns></returns>
            public static System.Net.IPAddress _ClientIP(HttpRequest httpRequest)
            {
                System.Net.IPAddress ip = System.Net.IPAddress.None;
                if (httpRequest != null)
                {
                    string ipAddress = null;

                    ipAddress = httpRequest.Headers["X-Forwarded-For"];
                    if (string.IsNullOrEmpty(ipAddress) ||
                        string.Equals("unknown", ipAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        ipAddress = httpRequest.Headers["X-Real-IP"];
                    }

                    if (string.IsNullOrEmpty(ipAddress) ||
                        string.Equals("unknown", ipAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        ipAddress = httpRequest.Headers["PROXY_FORWARDED_FOR"];
                    }

                    if (string.IsNullOrEmpty(ipAddress) ||
                        string.Equals("unknown", ipAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        ipAddress = httpRequest.Headers["Proxy-Client-IP"];
                    }

                    if (string.IsNullOrEmpty(ipAddress) ||
                        string.Equals("unknown", ipAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        ipAddress = httpRequest.Headers["WL-Proxy-Client-IP"];
                    }

                    if (string.IsNullOrEmpty(ipAddress) ||
                        string.Equals("unknown", ipAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        ipAddress = httpRequest.Headers["HTTP_CLIENT_IP"];
                    }

                    if (string.IsNullOrEmpty(ipAddress) ||
                        string.Equals("unknown", ipAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        ipAddress = httpRequest.Headers["REMOTE_ADDR"];
                    }

                    if (string.IsNullOrEmpty(ipAddress) ||
                        string.Equals("unknown", ipAddress, StringComparison.OrdinalIgnoreCase))
                    {
                        ipAddress = httpRequest.HttpContext.Connection.RemoteIpAddress.ToString();
                    }

                    //对于通过多个代理的情况，第一个IP为客户端真实IP,多个IP按照','分割
                    if (ipAddress != null && ipAddress.Length > 15)
                    {
                        //"***.***.***.***".length() = 15
                        if (ipAddress.IndexOf(",") > 0)
                        {
                            ipAddress = ipAddress.Substring(0, ipAddress.IndexOf(","));
                        }
                    }

                    if (!System.Net.IPAddress.TryParse(ipAddress, out ip))
                    {
                        ip = System.Net.IPAddress.None;
                    }
                }

                return ip;
            }

            /**Wap网关Via头信息中特有的描述信息*/
            private static readonly string[] mobileGateWayHeaders = new string[]
            {
                "ZXWAP", //中兴提供的wap网关的via信息，例如：Via=ZXWAP GateWayZTE Technologies，
                "chinamobile.com", //中国移动的诺基亚wap网关，例如：Via=WTP/1.1 GDSZ-PB-GW003-WAP07.gd.chinamobile.com (Nokia WAP Gateway 4.1 CD1/ECD13_D/4.1.04)
                "monternet.com", //移动梦网的网关，例如：Via=WTP/1.1 BJBJ-PS-WAP1-GW08.bj1.monternet.com. (Nokia WAP Gateway 4.1 CD1/ECD13_E/4.1.05)
                "infoX", //华为提供的wap网关，例如：Via=HTTP/1.1 GDGZ-PS-GW011-WAP2 (infoX-WISG Huawei Technologies)，或Via=infoX WAP Gateway V300R001 Huawei Technologies
                "XMS 724Solutions HTG", //国外电信运营商的wap网关，不知道是哪一家
                "wap.lizongbo.com", //自己测试时模拟的头信息
                "Bytemobile", //貌似是一个给移动互联网提供解决方案提高网络运行效率的，例如：Via=1.1 Bytemobile OSN WebProxy/5.1
            };

            /**电脑上的IE或Firefox浏览器等的User-Agent关键词*/
            private static readonly string[] pcHeaders = new string[]
            {
                "Windows 98",
                "Windows ME",
                "Windows 2000",
                "Windows XP",
                "Windows NT",
                "Ubuntu",
                "Macintosh",
                "Linux"
            };

            /**手机浏览器的User-Agent里的关键词*/
            private static readonly string[] mobileUserAgents = new string[]
            {
                "Nokia", //诺基亚，有山寨机也写这个的，总还算是手机，Mozilla/5.0 (Nokia5800 XpressMusic)UC AppleWebkit(like Gecko) Safari/530
                "SAMSUNG", //三星手机 SAMSUNG-GT-B7722/1.0+SHP/VPP/R5+Dolfin/1.5+Nextreaming+SMM-MMS/1.2.0+profile/MIDP-2.1+configuration/CLDC-1.1
                "MIDP-2", //j2me2.0，Mozilla/5.0 (SymbianOS/9.3; U; Series60/3.2 NokiaE75-1 /110.48.125 Profile/MIDP-2.1 Configuration/CLDC-1.1 ) AppleWebKit/413 (KHTML like Gecko) Safari/413
                "CLDC1.1", //M600/MIDP2.0/CLDC1.1/Screen-240X320
                "SymbianOS", //塞班系统的，
                "MAUI", //MTK山寨机默认ua
                "UNTRUSTED/1.0", //疑似山寨机的ua，基本可以确定还是手机
                "Windows CE", //Windows CE，Mozilla/4.0 (compatible; MSIE 6.0; Windows CE; IEMobile 7.11)
                "iPhone", //iPhone是否也转wap？不管它，先区分出来再说。Mozilla/5.0 (iPhone; U; CPU iPhone OS 4_1 like Mac OS X; zh-cn) AppleWebKit/532.9 (KHTML like Gecko) Mobile/8B117
                "iPad", //iPad的ua，Mozilla/5.0 (iPad; U; CPU OS 3_2 like Mac OS X; zh-cn) AppleWebKit/531.21.10 (KHTML like Gecko) Version/4.0.4 Mobile/7B367 Safari/531.21.10
                "Android", //Android是否也转wap？Mozilla/5.0 (Linux; U; Android 2.1-update1; zh-cn; XT800 Build/TITA_M2_16.22.7) AppleWebKit/530.17 (KHTML like Gecko) Version/4.0 Mobile Safari/530.17
                "BlackBerry", //BlackBerry8310/2.7.0.106-4.5.0.182
                "UCWEB", //ucweb是否只给wap页面？ Nokia5800 XpressMusic/UCWEB7.5.0.66/50/999
                "ucweb", //小写的ucweb貌似是uc的代理服务器Mozilla/6.0 (compatible; MSIE 6.0;) Opera ucweb-squid
                "BREW", //很奇怪的ua，例如：REW-Applet/0x20068888 (BREW/3.1.5.20; DeviceId: 40105; Lang: zhcn) ucweb-squid
                "J2ME", //很奇怪的ua，只有J2ME四个字母
                "YULONG", //宇龙手机，YULONG-CoolpadN68/10.14 IPANEL/2.0 CTC/1.0
                "YuLong", //还是宇龙
                "COOLPAD", //宇龙酷派YL-COOLPADS100/08.10.S100 POLARIS/2.9 CTC/1.0
                "TIANYU", //天语手机TIANYU-KTOUCH/V209/MIDP2.0/CLDC1.1/Screen-240X320
                "TY-", //天语，TY-F6229/701116_6215_V0230 JUPITOR/2.2 CTC/1.0
                "K-Touch", //还是天语K-Touch_N2200_CMCC/TBG110022_1223_V0801 MTK/6223 Release/30.07.2008 Browser/WAP2.0
                "Haier", //海尔手机，Haier-HG-M217_CMCC/3.0 Release/12.1.2007 Browser/WAP2.0
                "DOPOD", //多普达手机
                "Lenovo", // 联想手机，Lenovo-P650WG/S100 LMP/LML Release/2010.02.22 Profile/MIDP2.0 Configuration/CLDC1.1
                "LENOVO", // 联想手机，比如：LENOVO-P780/176A
                "HUAQIN", //华勤手机
                "AIGO-", //爱国者居然也出过手机，AIGO-800C/2.04 TMSS-BROWSER/1.0.0 CTC/1.0
                "CTC/1.0", //含义不明
                "CTC/2.0", //含义不明
                "CMCC", //移动定制手机，K-Touch_N2200_CMCC/TBG110022_1223_V0801 MTK/6223 Release/30.07.2008 Browser/WAP2.0
                "DAXIAN", //大显手机DAXIAN X180 UP.Browser/6.2.3.2(GUI) MMP/2.0
                "MOT-", //摩托罗拉，MOT-MOTOROKRE6/1.0 LinuxOS/2.4.20 Release/8.4.2006 Browser/Opera8.00 Profile/MIDP2.0 Configuration/CLDC1.1 Software/R533_G_11.10.54R
                "SonyEricsson", // 索爱手机，SonyEricssonP990i/R100 Mozilla/4.0 (compatible; MSIE 6.0; Symbian OS; 405) Opera 8.65 [zh-CN]
                "GIONEE", //金立手机
                "HTC", //HTC手机
                "ZTE", //中兴手机，ZTE-A211/P109A2V1.0.0/WAP2.0 Profile
                "HUAWEI", //华为手机，
                "webOS", //palm手机，Mozilla/5.0 (webOS/1.4.5; U; zh-CN) AppleWebKit/532.2 (KHTML like Gecko) Version/1.0 Safari/532.2 Pre/1.0
                "GoBrowser", //3g GoBrowser.User-Agent=Nokia5230/GoBrowser/2.0.290 Safari
                "IEMobile", //Windows CE手机自带浏览器，
                "WAP2.0" //支持wap 2.0的
            };

            /// <summary>
            /// 根据 Agent 判断是否是智能手机
            /// </summary>
            public static bool _IsMobile(HttpContext httpContext)
            {
                if (httpContext != null && httpContext.Request != null &&
                    httpContext.Request.Headers.ContainsKey("User-Agent"))
                {
                    bool pcFlag = false;
                    bool mobileFlag = false;
                    string via = httpContext.Request.Headers.ContainsKey("Via")
                        ? httpContext.Request.Headers["Via"].ToString().ToLowerInvariant()
                        : "";
                    string agent = httpContext.Request.Headers.ContainsKey("User-Agent")
                        ? httpContext.Request.Headers["User-Agent"].ToString().ToLowerInvariant()
                        : "";
                    //String xMsisdn = request.getHeader("X-MSISDN");
                    /*           Map<String, String[]> headers = request.headers();
                               play.Logger.info("xM = "+headers);*/
                    for (int i = 0; agent != null && !agent.Trim().Equals("") && i < pcHeaders.Length; i++)
                    {
                        if (agent.Contains(pcHeaders[i].ToLowerInvariant()))
                        {
                            pcFlag = true;
                            break;
                        }
                    }

                    for (int i = 0; via != null && !via.Trim().Equals("") && i < mobileGateWayHeaders.Length; i++)
                    {
                        if (via.Contains(mobileGateWayHeaders[i].ToLowerInvariant()))
                        {
                            mobileFlag = true;
                            break;
                        }
                    }

                    for (int i = 0;
                        !mobileFlag && agent != null && !agent.Trim().Equals("") && i < mobileUserAgents.Length;
                        i++)
                    {
                        if (agent.Contains(mobileUserAgents[i].ToLowerInvariant()))
                        {
                            mobileFlag = true;
                            break;
                        }
                    }

                    if (mobileFlag == false && pcFlag == true)
                    {
                        return false;
                    }
                }

                return true;
            }

            /// <summary>
            /// 根据 Agent 判断客户系统类型
            /// </summary>
            public static string _ClientOS(HttpContext httpContext)
            {
                string os = "未知";
                if (httpContext != null && httpContext.Request != null &&
                    httpContext.Request.Headers.ContainsKey("User-Agent"))
                {
                    string agent = httpContext.Request.Headers.ContainsKey("User-Agent").ToString();
                    if (agent.Contains("NT 10.0"))
                    {
                        os = "Windows 10";
                    }

                    if (agent.Contains("NT 6.4"))
                    {
                        os = "Windows 10";
                    }
                    else if (agent.Contains("NT 6.3"))
                    {
                        os = "Windows 8.1";
                    }
                    else if (agent.Contains("NT 6.2"))
                    {
                        os = "Windows 8";
                    }
                    else if (agent.Contains("NT 6.1"))
                    {
                        os = "Windows 7";
                    }
                    else if (agent.Contains("NT 6.0"))
                    {
                        os = "Windows Vista/Server 2008";
                    }
                    else if (agent.Contains("NT 5.2"))
                    {
                        os = "Windows Server 2003";
                    }
                    else if (agent.Contains("NT 5.1"))
                    {
                        os = "Windows XP";
                    }
                    else if (agent.Contains("NT 5"))
                    {
                        os = "Windows 2000";
                    }
                    else if (agent.Contains("Mac"))
                    {
                        os = "Mac";
                    }
                    else if (agent.Contains("Unix"))
                    {
                        os = "UNIX";
                    }
                    else if (agent.Contains("Linux"))
                    {
                        os = "Linux";
                    }
                    else if (agent.Contains("SunOS"))
                    {
                        os = "SunOS";
                    }
                }

                return os;
            }

            /// <summary>
            /// 根据 Agent 判断客户系统语言
            /// </summary>
            public static List<string> _ClientLanguages(HttpContext httpContext)
            {
                List<string> languages = new List<string>();
                if (httpContext != null && httpContext.Request != null &&
                    httpContext.Request.Headers.ContainsKey("Accept-Language"))
                {
                    var templanguagearray = httpContext.Request.Headers["Accept-Language"].ToString().Split(';');
                    if (templanguagearray.Length == 2)
                    {
                        languages = templanguagearray[0].Split(',').ToList();
                    }
                }

                return languages;
            }
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
                    else
                    {
                        var namelist = new List<string>();
                        for (int i = 0; i < Files.Count; i++)
                        {
                            var uripath = $"/{ResourcePrefix}_{Temp}/{Model}/{Id}/{Name}_{i.ToString("00")}";

                            SaveTempFile(uripath, Files.ElementAt(i).Key, Files.ElementAt(i).Value);

                            namelist.Add(uripath);
                        }

                        return new ApiResult<List<string>>(namelist);
                    }
                }
                else
                {
                    return new ApiResult(ApiResult.ECode.Fail);
                }
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
                            if (destfile.Exists && !overwrite)
                            {
                                return uripath;
                            }

                            if (!destfile.Directory.Exists) //目标文件夹不存在则创建
                            {
                                destfile.Directory.Create();
                            }

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
                        {
                            using (var fs = file.Open(FileMode.Open, FileAccess.Read))
                            {
                                byte[] byteArray = new byte[fs.Length];
                                fs.Read(byteArray, 0, byteArray.Length);
                                var base64str = Encoding.UTF8.GetString(byteArray);
                                var content = base64str.Split(",")[1];
                                byte[] contentbytes = Convert.FromBase64String(content);
                                var contenttype = Global.Regex.FileContentType.Match(base64str).Groups[1].Value;
                                result = new Microsoft.AspNetCore.Mvc.FileContentResult(contentbytes, contenttype)
                                    {LastModified = file.LastWriteTime, EnableRangeProcessing = true};
                                Utils.MemoryCacher.Set(uripath, result, Utils.MemoryCacher.CacheItemPriority.Normal,
                                    DateTime.Now.AddMinutes(5));
                            }
                        }
                    }

                    if (result != null)
                    {
                        return result;
                    }
                }

                return new Microsoft.AspNetCore.Mvc.NotFoundResult();
            }
        }

        public static class Email
        {
            static Email()
            {
                Utils.EmailContext.EmailServer.Instances["default"] = Config.EmailServer;

                Utils.EmailContext.EmailTemplate.Instances =
                    new ConcurrentDictionary<string, Utils.EmailContext.EmailTemplate>();
                foreach (var filePath in Directory.GetFiles(SiteContext.Config.AppData + "EmailTemplate/"))
                {
                    try
                    {
                        var file = new FileInfo(filePath);
                        var fileContent = File.ReadAllText(file.FullName);
                        var data = Global.Json.Deserialize<Utils.EmailContext.EmailTemplate>(fileContent);
                        Utils.EmailContext.EmailTemplate.Instances[data.Key] = data;
                    }
                    catch (Exception e)
                    {
                    }
                }
            }

            public static Dictionary<string, Utils.EmailContext.EmailTemplate> EmailTemplates =>
                Utils.EmailContext.EmailTemplate.Instances
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
                var emailserver = Utils.EmailContext.EmailServer.Instances["default"];

                Utils.EmailContext.SendMailAsync(emailserver, p_MailMessage);
            }

        }


        public static List<Model.Extend.Payment> SystemPaymentList()
        {
            var list = new List<Model.Extend.Payment>();

            foreach (EPaymentType item in Enum.GetValues(typeof(EPaymentType)))
            {
                var attr = Utils.Reflection.Attribute.GetCustomAttribute<PaymentAttribute>(item).First();
                var model = new Model.Extend.Payment()
                {
                    Name = item.ToString(),
                    Memo = attr.Memo,
                    Rate = attr.Rate,
                    IsSystem = true,
                    IsEnable = true,
                };

                list.Add(model);
            }

            return list;
        }


        public static class Url
        {
            public static string OrderInfo(string orderid)
            {
                return "http://" + Config.SiteDomain + "/o/" + orderid;
            }
        }


        public static class OrderHelper
        {
            public static YPayments.PayTicket GetPayTicket(string PaymentType, string OrderId, double Amount)
            {
                EPaymentType ePaymentType =
                    Enum.GetValues<EPaymentType>().FirstOrDefault(p => p.ToString() == PaymentType);
                YPayments.IPayment payment = GetPayment(ePaymentType);
                if (payment == null)
                {
                    return null;
                }
                else
                {
                    var notifyurl = "http://pay.gamemakesmoney.com/paynotify/tinystorecore/" + payment.Name;
                    var returnurl = "http://store.gamemakesmoney.com/order_" + OrderId;

                    ServicePointManager.SecurityProtocol = (SecurityProtocolType) 3072;

                    var payticket = payment.Pay(OrderId, Amount, "订单付款",
#if DEBUG
                        IPAddress.None,
#else
                        IPAddress.Parse(order.ClientIP),
#endif
                        //order.Url, Config.config.SiteDomain.TrimEnd('/') + "/api/" + order.PaymentType, Config.config.SiteDomain, "");
                        returnurl, notifyurl, SiteContext.Config.SiteDomain, "");
                    if (payticket != null && payticket.Success)
                    {
                        //return new Msg<dynamic> { Data = payticket, Message = order.OrderId, Result = true };
                        if (!string.IsNullOrWhiteSpace(payticket.Url))
                        {
                            if (!payticket.Url.StartsWith("http"))
                            {
                                return payticket;
                            }
                            else if (!string.IsNullOrWhiteSpace(payticket.FormHtml) &&
                                     payticket.FormHtml.StartsWith("<form"))
                            {
                                try
                                {
                                    var webclient = new System.Net.WebClient();
                                    webclient.Encoding = Encoding.UTF8;
                                    var url =
#if DEBUG
                                        "http://localhost:62340/RechargePre";
#else
                                        "http://pay.gamemakesmoney.com/RechargePre";
#endif
                                    //ServicePointManager.SecurityProtocol = (SecurityProtocolType)3072;
                                    var response = webclient.UploadString(url, Global.Json.Serialize(payticket));
                                    if (!Directory.Exists(SiteContext.Config.AppData + "Notify/"))
                                        Directory.CreateDirectory(SiteContext.Config.AppData + "Notify/");
                                    File.WriteAllText(SiteContext.Config.AppData + "Notify/" +
                                                      DateTime.Now.ToString("yyyyMMddHHmmssfff") + ".log",
                                        Global.Json.Serialize(payticket));
                                    var result = Global.Json.Deserialize<dynamic>(response);
                                    if (result != null)
                                    {
                                        if (!string.IsNullOrWhiteSpace((string) result.Data))
                                        {
                                            return new YPayments.PayTicket()
                                            {
                                                Success = true,
                                                Url = "http://pay.gamemakesmoney.com/Recharge/" + result.Data
                                            };
                                        }

                                        return payticket;
                                    }
                                    else
                                    {
                                        return payticket;
                                    }
                                }
                                catch (Exception ex)
                                {
                                    return payticket;
                                }
                            }
                            else
                            {
                                // 扫码支付  支付链接返回
                                return payticket;
                            }
                        }
                        else if (!string.IsNullOrWhiteSpace(payticket.Extra))
                        {
                            //app支付 支付链接返回
                            return payticket;
                        }
                    }

                    return payticket;
                }
            }

            private static YPayments.IPayment GetPayment(EPaymentType paytype)
            {
                YPayments.IPayment pay = null;
                switch (paytype)
                {
                    //case EPaymentType.AliPayApp:
                    //    pay = new YPayments.AliPayApp();
                    //    break;
                    //case EPaymentType.WechatApp:
                    //    pay = new YPayments.WechatApp();
                    //    break;
                    case EPaymentType.AliPayWap:
                        pay = new YPayments.AliPayWap();
                        break;
                    case EPaymentType.WeChatH5:
                        pay = new YPayments.WeChatH5();
                        break;
                    //case EPaymentType.AliPayQR:
                    //    pay = new YPayments.AliPayQR();
                    //    break;
                    //case EPaymentType.WeChatQR:
                    //    pay = new YPayments.WechatQR();
                    //    break;
                    default:
                        break;
                }

                if (pay.Name.StartsWith("alipay", StringComparison.OrdinalIgnoreCase))
                {
                    if (SiteContext.Config.AliPaySettings != null)
                    {
                        foreach (var set in SiteContext.Config.AliPaySettings)
                        {
                            pay[set.Key] = set.Value;
                        }

                        return pay;
                    }
                }
                else if (pay.Name.StartsWith("wechat", StringComparison.OrdinalIgnoreCase))
                {
                    if (SiteContext.Config.WechatPaySettings != null)
                    {
                        foreach (var set in SiteContext.Config.WechatPaySettings)
                        {
                            pay[set.Key] = set.Value;
                        }

                        return pay;
                    }
                }

                return null;
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

            public static dynamic Notify(string body)
            {
                var msg = "";
                try
                {
                    var paynotify = Global.Json.Deserialize<PayNotify>(body);

                    if (paynotify != null)
                    {
                        YPayments.IPayment payment =
                            GetPayment((EPaymentType) Enum.Parse(typeof(EPaymentType), paynotify.PaymentName,
                                true));

                        if (payment != null)
                        {
                            var res = payment.Notify(paynotify.Form, paynotify.Query, paynotify.Header,
                                paynotify.Body, paynotify.NotifyIp);

                            if (res.Status == YPayments.NotifyResult.EStatus.Completed)
                            {
                                Pay(res.OrderID, res.Amount, res.TxnID);
                            }
                            else
                            {
                                Global.FileSave(SiteContext.Config.AppData + "SignError/" +
                                                DateTime.Now.ToString("yyMMddHHmmssfff") + ".log", "OrderName:" +
                                    res.OrderName + "; Amount=" + res.Amount
                                    + "&Currency=" + res.Currency
                                    + "&OrderID=" + res.OrderID
                                    + "&PaymentName=" + res.PaymentName
                                    + "&ClientIp=" + paynotify.NotifyIp + "&Message=" + res.Message +
                                    Environment.NewLine, false);
                            }

                            msg = res.Message;
                        }
                        else
                        {
                            msg = "没有配置当前支付方式!";
                        }
                    }
                }
                catch
                {
                    Global.FileSave(SiteContext.Config.AppData + "NotifyBodyError/" +
                                    DateTime.Now.ToString("yyMMddHHmmssfff") + ".log", "Body:" + body, false);
                }

                return new {Code = 1, Data = msg};
            }

            internal static void Pay(string orderid, double incomme, string txnId)
            {
                if (Utils.MemoryCacher.Get(orderid) == null)
                {
                    Utils.MemoryCacher.Set(orderid, orderid, Utils.MemoryCacher.CacheItemPriority.Normal, null,
                        TimeSpan.FromMinutes(1));
                    var order = BLL.OrderBLL.QueryModelByOrderId(orderid);
                    if (order != null)
                    {
                        if (order != null && !order.IsPay && string.Equals(
                            order.Amount.ToString("f2"), incomme.ToString("f2"),
                            StringComparison.OrdinalIgnoreCase))
                        {
                            order.TranId = txnId;
                            order.IsPay = true;
                            order.Income = incomme;
                            order.LastUpdateDate = DateTime.Now;
                            //if (order.IsNeedEmail2Supplyer)
                            //    order.SupplyerCode = Global.Generator.Password(); //8位有效数字
                            //else
                            //    order.SupplyerCode = string.Empty;
                            BLL.OrderBLL.Update(order);
                        }

                        if (!order.IsDelivery)
                        {
                            Delivery(order);
                            ;
                        }
                    }
                }
            }

            public static void Delivery(Model.OrderModel order)
            {
                var store = BLL.StoreBLL.QueryModelByStoreId(order.StoreId);
                if (store != null)
                {
                    order.Income = order.Amount;
                    var product = BLL.ProductBLL.QueryModelByProductIdAndStoreId(order.ProductId, order.StoreId);
                    var liststock = new List<Model.Extend.StockOrder>();
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
                                for (int i = 0; i < order.Quantity; i++)
                                {
                                    stocklist[i].IsDelivery = true;
                                    stocklist[i].DeliveryDate = DateTime.Now;
                                }

                                BLL.StockBLL.UpdateRange(stocklist.Where(p => p.IsDelivery == true).ToList());
                                foreach (var item in stocklist.Where(p => p.IsDelivery == true))
                                {
                                    liststock.Add(new Model.Extend.StockOrder
                                    {
                                        StockId = item.StockId,
                                        Name = item.Name,
                                        Memo = item.Memo,
                                    });
                                }

                                order.StockList = liststock;
                                //order.DeliveryMessage = string.Empty;
                                order.IsDelivery = true;
                                order.DeliveryDate = DateTime.Now;
                                order.LastUpdateDate = DateTime.Now;
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
                    }

                    BLL.OrderBLL.Update(order);

                    DeliveryEmail(order);
                }
            }

            public static void DeliveryEmail(Model.OrderModel order)
            {
                var store = BLL.StoreBLL.QueryModelByStoreId(order.StoreId);
                if (store != null)
                {
                    //var msg_email = SiteContext.Email.TemplateGet("DeliveryEmail");
                    //todo
                    var msg_email = SiteContext.Email.EmailTemplates["DeliveryEmail"].Content;
                    
                    msg_email = msg_email.Replace("{SiteName}", store.Name);
                    msg_email = msg_email.Replace("{DeliveryDate}", order.DeliveryDate.ToString("yyyy年MM月dd日"));
                    msg_email = msg_email.Replace("{Name}", order.Name);
                    msg_email = msg_email.Replace("{OrderUrl}", SiteContext.Url.OrderInfo(order.OrderId));
                    msg_email = msg_email.Replace("{OrderId}", order.OrderId);
                    msg_email = msg_email.Replace("{Amount}", order.Amount.ToString("f2"));
                    msg_email = msg_email.Replace("{StoreQQ}", store.QQ);
                    var addtemplate = string.Empty;
                    if (order.StockList.Count > 0)
                    {
                        var emailadd = string.Empty;
                        foreach (var item in order.StockList)
                        {
                            //emailadd = SiteContext.Email.TemplateGet("DeliveryEmail_StockNoIp");
                            //todo
                            emailadd = SiteContext.Email.EmailTemplates["DeliveryEmail_StockNoIp"].Content;

                            emailadd = emailadd.Replace("{CardName}", item.Name);
                            emailadd = emailadd.Replace("{CardPassword}", item.Memo);

                            addtemplate += emailadd;
                        }
                    }
                    else
                    {
                        addtemplate = File.ReadAllText(SiteContext.Config.AppData + "emails/DeliveryEmail_NoStock.htm");
                    }

                    msg_email = msg_email.Replace("{AddTemplate}", addtemplate);
                    var product = BLL.ProductBLL.QueryModelByProductIdAndStoreId(order.ProductId, order.StoreId);
                    if (order.NoticeAccount.Contains("@"))
                    {
                        SiteContext.Email.Send(order.NoticeAccount,
                            store.Name + " 已收到你的订单（" + order.OrderId + "），欢迎您再次购买！", msg_email);
                    }

                    msg_email = "订单号:" + order.OrderId + "<br/><br/>";
                    msg_email += "订单状态:" + order.State.ToString() + "<br/><br/>";
                    msg_email += "商品名称:" + order.Name + "<br/><br/>";
                    if (product != null)
                        msg_email += "商品单价:￥" + product.Amount.ToString("f2") + "<br/><br/>";
                    if (order.Quantity > 0)
                        msg_email += "售出份数:" + order.Quantity + "<br/><br/>";
                    msg_email += "销售金额:￥" + order.Amount.ToString("f2") + "<br/><br/>";
                    msg_email += "买家" + (order.NoticeAccount.Contains("@") ? "邮箱" : "手机号") + ":" +
                                 order.NoticeAccount + "<br/><br/>";
                    msg_email += "买家联系方式:" + order.Contact + "<br/><br/>";
                    if (order.StockList.Count > 0)
                    {
                        msg_email += "买家购买的卡号及卡密如下：<br/><br/>";
                        foreach (var item in order.StockList)
                        {
                            msg_email += "卡号：" + item.Name + " 卡密:" + item.Memo + "<br/>";
                        }
                    }

                    SiteContext.Email.Send(store.Email, "您有一笔付款通知（" + order.OrderId + "），请尽快处理", msg_email);
                }
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