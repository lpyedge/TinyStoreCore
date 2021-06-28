using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace TinyStore
{
    public static class Global
    {
        static Global()
        {
        }
        
        public class AppSettings
        {
            //nuget Microsoft.Extensions.DependencyInjection
            public static ServiceProvider ServiceProvider { get; private set; }
            public static IConfiguration Configuration { get; private set; }

            /// <summary>
            ///     HttpContext_Current
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
            ///     初始化
            /// </summary>
            /// <param name="services"></param>
            /// <param name="configuration"></param>
            public static void ConfigureServices(IServiceCollection services, IConfiguration configuration)
            {
                ServiceProvider = services.BuildServiceProvider();

                Configuration = configuration;

                if (Inited != null)
                    Inited(ServiceProvider,Configuration);
            }

            public static Action<ServiceProvider,IConfiguration> Inited;
        }
        

        // public static class Json
        // {
        //     static Json()
        //     {
        //         JsonConvert.DefaultSettings = Settings.JsonSerializerSettingsFunc;
        //     }
        //
        //
        //     public static string Serialize<T>(T model)
        //     {
        //         return JsonConvert.SerializeObject(model);
        //     }
        //
        //     public static string SerializePretty<T>(T model)
        //     {
        //         using System.IO.StringWriter textWriter = new();
        //         using JsonTextWriter jsonWriter = new(textWriter)
        //         {
        //             //默认为Formatting.None
        //             Formatting = Newtonsoft.Json.Formatting.Indented,
        //             //缩进字符数，默认为2
        //             Indentation = 4,
        //             //缩进字符，默认为' '
        //             IndentChar = ' '
        //         };
        //         JsonSerializer jsonSerializer = Settings.JsonSerializerFunc();
        //         jsonSerializer.Serialize(jsonWriter, model);
        //         return textWriter.ToString();
        //     }
        //
        //     public static T Deserialize<T>(string json)
        //     {
        //         return JsonConvert.DeserializeObject<T>(json);
        //     }
        //
        //     internal class IgnorePropsContractResolver : Newtonsoft.Json.Serialization.DefaultContractResolver
        //     {
        //         private readonly bool ignore;
        //         private readonly string[] props;
        //
        //         /// <summary>
        //         ///     构造函数
        //         /// </summary>
        //         /// <param name="ignore">true:表示props是需要忽略的字段  false：表示props是要保留的字段</param>
        //         /// <param name="props">传入的属性数组</param>
        //         public IgnorePropsContractResolver(bool ignore = true, params string[] props)
        //         {
        //             //指定要序列化属性的清单
        //             this.props = props;
        //
        //             this.ignore = ignore;
        //         }
        //
        //         protected override IList<JsonProperty> CreateProperties(Type type,
        //             MemberSerialization memberSerialization)
        //         {
        //             var list = base.CreateProperties(type, memberSerialization);
        //             //忽略Func和Action属性
        //             return list.Where(p => p.PropertyType.Name != "Func`4" && p.PropertyType.Name != "Action`4")
        //                 .Where(p =>
        //                 {
        //                     if (ignore)
        //                         //忽略清单有列出的属性
        //                         return !props.Contains(p.PropertyName);
        //                     return props.Contains(p.PropertyName);
        //                 }).ToList();
        //         }
        //
        //         protected override string ResolvePropertyName(string propertyName)
        //         {
        //             return propertyName.ToUpperInvariant();
        //         }
        //     }
        //
        //     public static class Settings
        //     {
        //         public static Func<JsonSerializerSettings> JsonSerializerSettingsFunc = () =>
        //         {
        //             var setting = new JsonSerializerSettings();
        //             JsonSerializerSettingsAction(setting);
        //             return setting;
        //         };
        //
        //         public static Func<JsonSerializer> JsonSerializerFunc = () =>
        //         {
        //             var setting = new JsonSerializerSettings();
        //             JsonSerializerSettingsAction(setting);
        //
        //             JsonSerializer jsonSerializer = new()
        //             {
        //                 DateFormatHandling = setting.DateFormatHandling,
        //
        //                 DateFormatString = setting.DateFormatString,
        //                 DateTimeZoneHandling = setting.DateTimeZoneHandling,
        //                 DateParseHandling = setting.DateParseHandling,
        //
        //                 NullValueHandling = setting.NullValueHandling,
        //                 DefaultValueHandling = setting.DefaultValueHandling,
        //                 ContractResolver = setting.ContractResolver ?? new DefaultContractResolver(),
        //
        //                 ConstructorHandling = setting.ConstructorHandling,
        //                 StringEscapeHandling = setting.StringEscapeHandling,
        //                 FloatParseHandling = setting.FloatParseHandling,
        //                 FloatFormatHandling = setting.FloatFormatHandling,
        //                 ObjectCreationHandling = setting.ObjectCreationHandling,
        //                 ReferenceLoopHandling = setting.ReferenceLoopHandling,
        //             };
        //
        //             return jsonSerializer;
        //         };
        //
        //         public static Action<JsonSerializerSettings> JsonSerializerSettingsAction = (setting) =>
        //         {
        //             setting = new JsonSerializerSettings();
        //             setting.DateFormatHandling = DateFormatHandling.IsoDateFormat;
        //             setting.DateFormatString = "yyyy-MM-ddTHH:mm:ss.fffffffzzz";
        //             setting.DateTimeZoneHandling = DateTimeZoneHandling.Local;
        //             setting.DateParseHandling = DateParseHandling.DateTime;
        //             setting.NullValueHandling = NullValueHandling.Include;
        //             setting.DefaultValueHandling = DefaultValueHandling.Include;
        //             setting.ContractResolver = new IgnorePropsContractResolver(true, "StoreId", "storeId");
        //             setting.ConstructorHandling = ConstructorHandling.Default;
        //             setting.StringEscapeHandling = StringEscapeHandling.Default;
        //             setting.FloatParseHandling = FloatParseHandling.Decimal;
        //             setting.FloatFormatHandling = FloatFormatHandling.Symbol;
        //             setting.ObjectCreationHandling = ObjectCreationHandling.Replace;
        //             setting.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
        //         };
        //     }
        // }

        public static class Json
        {
            static Json()
            {
                DefaultSettings = Settings.JsonSerializerSettingsFunc();
            }

            private static readonly System.Text.Json.JsonSerializerOptions DefaultSettings;

            public static string Serialize<T>(T model)
            {
                return System.Text.Json.JsonSerializer.Serialize(model, DefaultSettings);
            }

            public static string SerializePretty<T>(T model)
            {
                var options = Settings.JsonSerializerSettingsFunc();
                options.WriteIndented = true;

                return System.Text.Json.JsonSerializer.Serialize(model, options);
            }

            public static T Deserialize<T>(string json)
            {
                return System.Text.Json.JsonSerializer.Deserialize<T>(json, DefaultSettings);
            }

            public static class Settings
            {
                public static Func<System.Text.Json.JsonSerializerOptions> JsonSerializerSettingsFunc = () =>
                {
                    var setting = new System.Text.Json.JsonSerializerOptions();
                    JsonSerializerSettingsAction(setting);
                    return setting;
                };

                public static Action<System.Text.Json.JsonSerializerOptions> JsonSerializerSettingsAction = (setting) =>
                {
                    //https://docs.microsoft.com/zh-cn/dotnet/api/system.text.json.jsonserializeroptions?view=net-5.0
                    setting = new System.Text.Json.JsonSerializerOptions()
                    {
                        PropertyNamingPolicy = null,
                        DictionaryKeyPolicy = null,
                        WriteIndented = false,
                        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve,
                        NumberHandling = System.Text.Json.Serialization.JsonNumberHandling.Strict,
                        AllowTrailingCommas = true,
                        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.Default,
                        IncludeFields = false,
                        MaxDepth = 0,
                        IgnoreReadOnlyFields = true,
                        IgnoreNullValues = false,
                        IgnoreReadOnlyProperties = true,
                        PropertyNameCaseInsensitive = true,
                        DefaultIgnoreCondition = System.Text.Json.Serialization.JsonIgnoreCondition.Never,
                        ReadCommentHandling = System.Text.Json.JsonCommentHandling.Skip,
                        DefaultBufferSize = new System.Text.Json.JsonSerializerOptions().DefaultBufferSize,
                        Converters = { }
                    };
                };
            }
        }

        private static HashAlgorithm HashSHA1Provider => Utils.HASHCrypto.Generate(Utils.HASHCrypto.CryptoEnum.SHA1);

        public static string Hash(params string[] strs)
        {
            if (strs.Length > 0)
            {
                var res = "";
                foreach (var item in strs)
                {
                    res = Utils.HASHCrypto.Encrypt(HashSHA1Provider, res + item);
                }

                return res;
            }

            return null;
        }


        public static class Generator
        {
            public static Random Random
            {
                get { return new Random(System.Guid.NewGuid().GetHashCode()); }
            }

            public static string Guid()
            {
                return System.Guid.NewGuid().ToString("N");
            }

            public static uint NextUint(uint min, uint max)
            {
                return (uint) Random.Next((int) min, (int) max + 1);
            }

            public static string Number(int length)
            {
                if (length > 0)
                {
                    var formatstr = "";
                    for (int i = 0; i < length; i++)
                    {
                        formatstr += "0";
                    }
                }

                return Random.Next(0, 999999).ToString("000000");
            }

            public static string DateId(int state = 0)
            {
                switch (state)
                {
                    case 0:
                    {
                        var ts = DateTime.Now - DateTime.Parse("2019-01-01");
                        var str = ((int) ts.TotalSeconds).ToString() + Random.Next(0, 10000).ToString("0000");
                        return Utils.Duotricemary.FromInt64(ulong.Parse(str)).StringValue;
                    }
                    case 1:
                    {
                        var str = DateTime.Now.ToString("MMddHHmmssfff") + Random.Next(0, 100).ToString("00");
                        return DateTime.Now.ToString("yy") + Utils.Duotricemary.FromInt64(ulong.Parse(str)).StringValue;
                    }
                    case 2:
                    {
                        var str = DateTime.Now.ToString("MMddHHmmssfff") + Random.Next(0, 100000).ToString("00000");
                        return DateTime.Now.ToString("yy") + Utils.Duotricemary.FromInt64(ulong.Parse(str)).StringValue;
                    }
                    default:
                    {
                        var str = DateTime.Now.ToString("HHmmssfff") + Random.Next(0, 100000000).ToString("00000000");
                        return DateTime.Now.ToString("yyMMdd") +
                               Utils.Duotricemary.FromInt64(ulong.Parse(str)).StringValue;
                    }
                }
            }

            //public static string DateId()
            //{
            //    return DateTime.UtcNow.ToString("yyMMddHHmmssfff")+GetRandom().Next(1000,10000);
            //}
        }

        public static class Regex
        {
            /// <summary>
            /// file base64 => contenttype
            /// </summary>
            public static readonly System.Text.RegularExpressions.Regex FileContentType =
                new System.Text.RegularExpressions.Regex(@"data:([\w-]+/[\w\.\-]+);base64,",
                    RegexOptions.Compiled | RegexOptions.IgnoreCase);

            /// <summary>
            /// 邮箱验证
            /// </summary>
            public static readonly System.Text.RegularExpressions.Regex Email =
                new System.Text.RegularExpressions.Regex("^[A-Za-z0-9._%+-]+@(?:[A-Za-z0-9-]+.)+[A-Za-z]{2,6}$",
                    RegexOptions.Compiled);
        }

        /// <summary>
        /// 首字母
        /// </summary>
        /// <param name="inputText"></param>
        /// <returns>大写字母 A-Z & #</returns>
        public static string Initial(string inputText)
        {
            var initials = NPinyin.Pinyin.GetInitials(inputText);
            if (initials.Length > 0)
            {
                var initial = initials.Substring(0, 1);
                if (System.Text.RegularExpressions.Regex.IsMatch(initial, "[A-Z]"))
                {
                    return initial;
                }

                return "#";
            }

            return null;
        }

        public static class AlipaySchema
        {   
            public enum EBankMark
            {
                /// <summary>
                /// 中国工商银行 Industrial and Commercial Bank of China
                /// </summary>
                ICBC ,
                /// <summary>
                /// 中国建设银行  China Construction Bank
                /// </summary>
                CCB,
                /// <summary>
                /// 中国银行  Bank of China
                /// </summary>
                BOC,//BC,
                /// <summary>
                /// 中国农业银行  Agricultural Bank of China
                /// </summary>
                ABC,
                /// <summary>
                /// 交通银行 BC Bank of Communications
                /// </summary>
                BCM,
                /// <summary>
                /// 中国邮政储蓄银行 POSTAL SAVINGS BANK OF CHINA
                /// </summary>
                PSBC,
                /// <summary>
                /// 招商银行  China Merchants Bank
                /// </summary>
                CMB,
                /// <summary>
                /// 中国民生银行  China Minsheng Bank
                /// </summary>
                CMBC,
                /// <summary>
                /// 上海浦东发展银行 Shanghai Pudong Development Bank
                /// </summary>
                SPDB,
                // 中信银行 China CITIC Bank
                /// <summary>
                /// 中国光大银行  China Everbright Bank
                /// </summary>
                CEB,
                // 华夏银行 HB Huaxia Bank
                /// <summary>
                /// 广东发展银行  Guangdong Development Bank
                /// </summary>
                GDB,
                /// <summary>
                /// 深圳发展银行  Shenzhen Development Bank
                /// </summary>
                SDB,
                /// <summary>
                /// 兴业银行  China's Industrial Bank
                /// </summary>
                CIB,
                /// <summary>
                /// 国家开发银行  China Development Bank
                /// </summary>
                CDB,
                /// <summary>
                /// 中国进出口银行  Export-Import Bank of China
                /// </summary>
                EIBC,
                /// <summary>
                /// 中国农业发展银行  Agricultural Development of China
                /// </summary>
                ADBC,
                /// <summary>
                /// 汇丰银行  Hongkong and Shanghai Banking Corporation
                /// </summary>
                HSBC,
            }

           
            /// <summary>
            /// 唤醒支付宝并传参 跳转到对应支付宝Schema链接 参数需uri编码
            /// 我的客服:
            /// alipays://platformapi/startapp?appId=20000691&url=
            /// 通用浏览器模式:
            /// alipays://platformapi/startapp?appId=20000067&url=
            /// 扫一扫
            /// alipays://platformapi/startapp?saId=10000007&qrcode=
            /// </summary>
            public static string LaunchUri =>"alipays://platformapi/startapp?saId=10000007&qrcode=";

            /// <summary>
            /// 支付宝扫码转账到银行卡
            /// </summary>
            /// <param name="bankMark"></param>
            /// <param name="cardNo"></param>
            /// <param name="bankAccount"></param>
            /// <param name="amount"></param>
            /// <param name="memo"></param>
            /// <returns></returns>
            public static string ToBankCard(EBankMark bankMark,string cardNo,string bankAccount,double amount,string memo = "")
            {
                var toCardUri =
                    $"alipays://platformapi/startapp?appId=09999988&actionType=toCard&cardNo={cardNo}&bankMark={bankMark.ToString()}&amount={amount.ToString("0.00")}&money={amount.ToString("0.00")}&bankAccount={Uri.EscapeDataString(bankAccount)}";

                return toCardUri;
            }

            /// <summary>
            /// 支付宝扫码转账到银行卡
            /// alipayUserId 获取 通过接入 https://opendocs.alipay.com/open/284/106000 
            /// </summary>
            /// <param name="alipayUserId"></param>
            /// <param name="amount"></param>
            /// <param name="memo"></param>
            /// <returns></returns>
            public static string ToBankCard(string alipayUserId,double amount,string memo="")
            {
                var toCardUri =
                    $"alipays://platformapi/startapp?appId=09999988&actionType=toAccount&goBack=NO&amount={amount.ToString("0.00")}&userId={alipayUserId}&memo={Uri.EscapeDataString(memo)}";
                
                return toCardUri;
            }
            #region alipay schema

            // 支付宝：
            // alipay://
            // 收款:
            // alipays://platformapi/startapp?appId=20000123
            // 扫码：
            // alipays://platformapi/startapp?saId=10000007
            // 余额宝:
            // alipays://platformapi/startapp?appId=20000032
            // 收钱码领取：
            // https://qr.alipay.com/ygx06424srpkyt7p84mfia9
            // 转账:
            // alipays://platformapi/startapp?appId=20000221
            // 租房：
            // alipays://platformapi/startapp?appId=60000125
            // 城市服务：
            // alipays://platformapi/startapp?appId=20000178
            // 手机充值：
            // alipays://platformapi/startapp?appId=10000003
            // 快递查询：
            // alipays://platformapi/startapp?appId=20000754
            // 我的快递-寄件平台：
            // alipays://platformapi/startapp?appId=60000146
            // 我的二维码:
            // alipays://platformapi/startapp?appId=20000085
            // 蚂蚁庄园:
            // alipays://platformapi/startapp?appId=66666674
            // 蚂蚁森林：
            // alipays://platformapi/startapp?appId=60000002
            // 我的公益：
            // alipays://platformapi/startapp?appId=66666867
            // 运动：
            // alipays://platformapi/startapp?appId=20000869
            // 蚂蚁借呗：
            // alipays://platformapi/startapp?appId=20000180
            // 个人主页：
            // alipays://platformapi/startapp?appId=20000186
            // 个人名片:
            // alipays://platformapi/startapp?appId=20000228
            // 信用卡还款：
            // alipays://platformapi/startapp?appId=09999999
            // 爱心捐赠：
            // alipays://platformapi/startapp?appId=10000009
            // 彩票：
            // alipays://platformapi/startapp?appId=10000011
            // 转账：
            // alipays://platformapi/startapp?appId=09999988
            // 花呗：
            // alipays://platformapi/startapp?appId=20000199
            // 生活缴费：
            // alipays://platformapi/startapp?appId=20000193
            // 芝麻信用:
            // alipays://platformapi/startapp?appId=20000118
            // 位置:
            // alipays://platformapi/startapp?appId=20000226
            // 卡券:
            // alipays://platformapi/startapp?appId=20000227
            // 饿了么外卖:
            // alipays://platformapi/startapp?appId=20000120
            // 淘票票电影:
            // alipays://platformapi/startapp?appId=20000131
            // 火车票:
            // alipays://platformapi/startapp?appId=20000143
            // 汇率换算:
            // alipays://platformapi/startapp?appId=20000150
            // 理财小工具：
            // alipays://platformapi/startapp?appId=20000161
            // 羊城通充值：
            // alipays://platformapi/startapp?appId=20000162
            // 收货地址:
            // alipays://platformapi/startapp?appId=20000714
            // 隐私：
            // alipays://platformapi/startapp?appId=20000723
            // 通用：
            // alipays://platformapi/startapp?appId=20000724
            // 充值中心：
            // alipays://platformapi/startapp?appId=20000987
            // 校园一卡通：
            // alipays://platformapi/startapp?appId=2013062600000474
            // 淘宝：
            // alipays://platformapi/startapp?appId=2013082800000932
            // 教育缴费：
            // alipays://platformapi/startapp?appId=2014021200003129
            // ofo小黄车：
            // alipays://platformapi/startapp?appId=2017041206668232
            // 高德打车：
            // alipays://platformapi/startapp?appId=2018070960585195
            // 蚂蚁宝卡：
            // alipays://platformapi/startapp?appId=60000057
            // 地铁购票：
            // alipays://platformapi/startapp?appId=60000070
            // AA收款：
            // alipays://platformapi/startapp?appId=60000154
            // AA收款:
            // alipays://platformapi/startapp?appId=66666696
            // AA收款：
            // alipays://platformapi/startapp?appId=9000258
            // 共享单车：
            // alipays://platformapi/startapp?appId=60000155
            // 余利宝:
            // alipays://platformapi/startapp?appId=66666708
            // 收钱码服务:
            // alipays://platformapi/startapp?appId=66666714
            // 大麦演出票：
            // alipays://platformapi/startapp?appId=66666753
            // 口碑签到：
            // alipays://platformapi/startapp?appId=66666776
            // 信用生活：
            // alipays://platformapi/startapp?appId=66666786
            // 支付宝月账单：
            // alipays://platformapi/startapp?appId=66666798
            // 天猫购物：
            // alipays://platformapi/startapp?appId=66666820
            // 绿色城市：
            // alipays://platformapi/startapp?appId=66666824
            // 还贷管家：
            // alipays://platformapi/startapp?appId=66666819
            // 股票:
            // alipays://platformapi/startapp?appId=20000134
            // 淘票票：
            // alipays://platformapi/startapp?appId=68687093
            // 淘票票H5票券：
            // alipays://platformapi/startapp?appId=68687095
            // 淘票票H5购票：
            // alipays://platformapi/startapp?appId=68687096
            // 收款：
            // alipays://platformapi/startapp?appId=20000674
            // 余额宝：
            // alipays://platformapi/startapp?appId=60000126
            // 余额宝:
            // alipays://platformapi/startapp?appId=77700124
            // 话费卡转让：
            // alipays://platformapi/startapp?appId=10000033
            // 关于：
            // alipays://platformapi/startapp?appId=10000110
            // 天猫：
            // alipays://platformapi/startapp?appId=20000000
            // 账单：
            // alipays://platformapi/startapp?appId=20000003
            // 银行卡:
            // alipays://platformapi/startapp?appId=20000014
            // 账户详情:
            // alipays://platformapi/startapp?appId=20000019
            // 支付设置:
            // alipays://platformapi/startapp?appId=20000024
            // 实名认证:
            // alipays://platformapi/startapp?appId=20000038
            // 反馈:
            // alipays://platformapi/startapp?appId=20000049
            // 上银汇款:
            // alipays://platformapi/startapp?appId=20000078
            // 生活号:
            // alipays://platformapi/startapp?appId=20000101
            // 出境:
            // alipays://platformapi/startapp?appId=20000107
            // 安全设置:
            // alipays://platformapi/startapp?appId=20000113
            // 亲情号:
            // alipays://platformapi/startapp?appId=20000132
            // 火车票机票:
            // alipays://platformapi/startapp?appId=20000135
            // 飞猪酒店:
            // alipays://platformapi/startapp?appId=20000139
            // 娱乐宝:
            // alipays://platformapi/startapp?appId=20000142
            // 海外交通卡:
            // alipays://platformapi/startapp?appId=20000152
            // 游戏中心：
            // alipays://platformapi/startapp?appId=20000153
            // 国际机票：
            // alipays://platformapi/startapp?appId=20000157
            // 蚂蚁会员：
            // alipays://platformapi/startapp?appId=20000160
            // 定期：
            // alipays://platformapi/startapp?appId=20000165
            // 记账本：
            // alipays://platformapi/startapp?appId=20000168
            // 手势：
            // alipays://platformapi/startapp?appId=20000184
            // H5公共资源：
            // alipays://platformapi/startapp?appId=20000196
            // H5运营活动资源包：
            // alipays://platformapi/startapp?appId=20000202
            // 亲情圈:
            // alipays://platformapi/startapp?appId=20000205
            // 黄金:
            // alipays://platformapi/startapp?appId=20000218
            // 蚂蚁乐驾:
            // alipays://platformapi/startapp?appId=20000241
            // 总资产:
            // alipays://platformapi/startapp?appId=20000243
            // 收藏:
            // alipays://platformapi/startapp?appId=20000245
            // 活动收款:
            // alipays://platformapi/startapp?appId=20000259
            // 信用卡账单:
            // alipays://platformapi/startapp?appId=20000266
            // 数字证书:
            // alipays://platformapi/startapp?appId=20000298
            // 暗号:
            // alipays://platformapi/startapp?appId=20000307
            // 支付宝账号:
            // alipays://platformapi/startapp?appId=20000308
            // 1688好货源:
            // alipays://platformapi/startapp?appId=20000522
            // 活动群:
            // alipays://platformapi/startapp?appId=20000672
            // 我的客服:
            // alipays://platformapi/startapp?appId=20000691
            // 淘宝会员名
            // alipays://platformapi/startapp?appId=20000710
            // 蚂蚁微客：
            // alipays://platformapi/startapp?appId=20000735
            // 在线理赔：
            // alipays://platformapi/startapp?appId=20000750
            // 悄悄话：
            // alipays://platformapi/startapp?appId=20000752
            // 滴滴出行：
            // alipays://platformapi/startapp?appId=20000778
            // 小视频：
            // alipays://platformapi/startapp?appId=20000780
            // 圈存机：
            // alipays://platformapi/startapp?appId=20000791
            // 基金：
            // alipays://platformapi/startapp?appId=20000793
            // 地铁票购票：
            // alipays://platformapi/startapp?appId=20000796
            // 新的朋友：
            // alipays://platformapi/startapp?appId=20000820
            // 云客服：
            // alipays://platformapi/startapp?appId=20000827
            // 淘票票H5票券：
            // alipays://platformapi/startapp?appId=20000834
            // 人脸识别：
            // alipays://platformapi/startapp?appId=20000841
            // 大学生活：
            // alipays://platformapi/startapp?appId=20000859
            // 国内机票逆向：
            // alipays://platformapi/startapp?appId=20000877
            // 境外上网：
            // alipays://platformapi/startapp?appId=20000895
            // 网商贷：
            // alipays://platformapi/startapp?appId=20000899
            // 充值助手：
            // alipays://platformapi/startapp?appId=20000905
            // 生活号：
            // alipays://platformapi/startapp?appId=20000909
            // 网商银行：
            // alipays://platformapi/startapp?appId=20000913
            // 社交H5：
            // alipays://platformapi/startapp?appId=20000917
            // 车主服务：
            // alipays://platformapi/startapp?appId=20000919
            // 发票管家：
            // alipays://platformapi/startapp?appId=20000920
            // 汽车票：
            // alipays://platformapi/startapp?appId=20000922
            // 口碑卡券：
            // alipays://platformapi/startapp?appId=20000923
            // 蚂蚁保险：
            // alipays://platformapi/startapp?appId=20000936
            // 支付结果页口碑推荐：
            // alipays://platformapi/startapp?appId=20000939
            // 生活圈：
            // alipays://platformapi/startapp?appId=20000943
            // 群聊：
            // alipays://platformapi/startapp?appId=20000951
            // 有财教练：
            // alipays://platformapi/startapp?appId=20000971
            // 口碑我的订单：
            // alipays://platformapi/startapp?appId=20000975
            // 心愿储蓄-余额宝：
            // alipays://platformapi/startapp?appId=20000981
            // 体育服务：
            // alipays://platformapi/startapp?appId=20000988
            // H5在线买单：
            // alipays://platformapi/startapp?appId=20000989
            // 商家动态：
            // alipays://platformapi/startapp?appId=20000991
            // 安全课堂：
            // alipays://platformapi/startapp?appId=20001010
            // 照片：
            // alipays://platformapi/startapp?appId=20001021
            // 拍摄：
            // alipays://platformapi/startapp?appId=20001022
            // 财富交易组件：
            // alipays://platformapi/startapp?appId=20001045
            // 大学充值缴费：
            // alipays://platformapi/startapp?appId=20001091
            // 安全备忘：
            // alipays://platformapi/startapp?appId=20001116
            // 一字千金：
            // alipays://platformapi/startapp?appId=20001121
            // 送福卡：
            // alipays://platformapi/startapp?appId=20002018
            // 小程序收藏：
            // alipays://platformapi/startapp?appId=2018072560844004
            // 专属优惠频道：
            // alipays://platformapi/startapp?appId=60000006
            // 国内机票React正向：
            // alipays://platformapi/startapp?appId=60000007
            // 手艺人：
            // alipays://platformapi/startapp?appId=60000008
            // 社交金融H5：
            // alipays://platformapi/startapp?appId=60000010
            // 安全设备：
            // alipays://platformapi/startapp?appId=60000011
            // 中小学：
            // alipays://platformapi/startapp?appId=60000012
            // 口碑在线购买H5：
            // alipays://platformapi/startapp?appId=60000014
            // 账单关联业务-h5：
            // alipays://platformapi/startapp?appId=60000016
            // 基金组合：
            // alipays://platformapi/startapp?appId=60000018
            // 蚂蚁保险：
            // alipays://platformapi/startapp?appId=60000023
            // 商圈：
            // alipays://platformapi/startapp?appId=60000026
            // 月度榜单：
            // alipays://platformapi/startapp?appId=60000029
            // 电子证件：
            // alipays://platformapi/startapp?appId=60000032
            // in定制印品：
            // alipays://platformapi/startapp?appId=60000033
            // 大牌抢购：
            // alipays://platformapi/startapp?appId=60000039
            // 未来酒店：
            // alipays://platformapi/startapp?appId=60000040
            // 支付成功页权益区：
            // alipays://platformapi/startapp?appId=60000044
            // 社交聚合H5：
            // alipays://platformapi/startapp?appId=60000050
            // 天天有料：
            // alipays://platformapi/startapp?appId=60000071
            // VIP预约服务：
            // alipays://platformapi/startapp?appId=60000076
            // 优酷：
            // alipays://platformapi/startapp?appId=60000077
            // 商家服务：
            // alipays://platformapi/startapp?appId=60000081
            // Mini 花呗：
            // alipays://platformapi/startapp?appId=60000091
            // 电子公交卡：
            // alipays://platformapi/startapp?appId=60000098
            // 奖励金：
            // alipays://platformapi/startapp?appId=60000103
            // 银行卡：
            // alipays://platformapi/startapp?appId=60000105
            // 定期+：
            // alipays://platformapi/startapp?appId=60000119
            // 福员外：
            // alipays://platformapi/startapp?appId=60000120
            // 投票：
            // alipays://platformapi/startapp?appId=60000121
            // 淘票票H5购票：
            // alipays://platformapi/startapp?appId=60000130
            // 质押资产：
            // alipays://platformapi/startapp?appId=60000132
            // 外币兑换：
            // alipays://platformapi/startapp?appId=60000134
            // 飞猪汽车票新版：
            // alipays://platformapi/startapp?appId=60000135
            // 飞猪国内机票：
            // alipays://platformapi/startapp?appId=60000138
            // 医疗健康：
            // alipays://platformapi/startapp?appId=60000141
            // 财富运营承接中间页：
            // alipays://platformapi/startapp?appId=60000142
            // 冻结金额：
            // alipays://platformapi/startapp?appId=60000145
            // h5券详情页面：
            // alipays://platformapi/startapp?appId=60000147
            // 财富号：
            // alipays://platformapi/startapp?appId=60000148
            // 我的口碑：
            // alipays://platformapi/startapp?appId=60000150
            // 快消优惠：
            // alipays://platformapi/startapp?appId=60000151
            // 支付签约中心:
            // alipays://platformapi/startapp?appId=60000157
            // 借呗任务平台:
            // alipays://platformapi/startapp?appId=60000158
            // 周周乐:
            // alipays://platformapi/startapp?appId=60000161
            // 表情搜索:
            // alipays://platformapi/startapp?appId=60000163
            // 小程序:
            // alipays://platformapi/startapp?appId=66666666
            // 会员卡:
            // alipays://platformapi/startapp?appId=66666667
            // 口碑资源加速二:
            // alipays://platformapi/startapp?appId=66666669
            // 国际资源加速一:
            // alipays://platformapi/startapp?appId=66666670
            // 新消息通知:
            // alipays://platformapi/startapp?appId=66666672
            // 风险评测:
            // alipays://platformapi/startapp?appId=66666673
            // 口碑生活圈问答:
            // alipays://platformapi/startapp?appId=66666675
            // 账单详情:
            // alipays://platformapi/startapp?appId=66666676
            // 亚博游戏:
            // alipays://platformapi/startapp?appId=66666677
            // AR:
            // alipays://platformapi/startapp?appId=66666678
            // 新人气榜单:
            // alipays://platformapi/startapp?appId=66666679
            // 福卡回忆:
            // alipays://platformapi/startapp?appId=66666682
            // 集分宝:
            // alipays://platformapi/startapp?appId=66666683
            // 信用借还:
            // alipays://platformapi/startapp?appId=66666684
            // 网银大额充值:
            // alipays://platformapi/startapp?appId=66666685
            // 泛行业频道:
            // alipays://platformapi/startapp?appId=66666686
            // jet离线加速一:
            // alipays://platformapi/startapp?appId=66666687
            // 我的发票抬头:
            // alipays://platformapi/startapp?appId=66666688
            // 附近人气榜:
            // alipays://platformapi/startapp?appId=66666689
            // 店铺弹窗领券:
            // alipays://platformapi/startapp?appId=66666691
            // 小程序资源包:
            // alipays://platformapi/startapp?appId=66666692
            // 标签系统:
            // alipays://platformapi/startapp?appId=66666698
            // 境外当面付店铺码:
            // alipays://platformapi/startapp?appId=66666699
            // 实物黄金:
            // alipays://platformapi/startapp?appId=66666700
            // appraise:
            // alipays://platformapi/startapp?appId=66666702
            // 打开支付宝:
            // alipays://platformapi/startapp?appId=66666706
            // mallcoupon:
            // alipays://platformapi/startapp?appId=66666707
            // 商圈聚合页:
            // alipays://platformapi/startapp?appId=66666710
            // 天猫资源加速:
            // alipays://platformapi/startapp?appId=66666711
            // 芝麻信用:
            // alipays://platformapi/startapp?appId=66666713
            // 信用卡还款H5:
            // alipays://platformapi/startapp?appId=66666715
            // 小程序关于页面:
            // alipays://platformapi/startapp?appId=66666718
            // 功能管理:
            // alipays://platformapi/startapp?appId=66666719
            // 钱包股票-社区资讯:
            // alipays://platformapi/startapp?appId=66666721
            // 钱包股票-行情和提醒:
            // alipays://platformapi/startapp?appId=66666722
            // 统一授权管理:
            // alipays://platformapi/startapp?appId=66666724
            // 区块链:
            // alipays://platformapi/startapp?appId=66666728
            // 口碑红人:
            // alipays://platformapi/startapp?appId=66666729
            // 花呗挖哦:
            // alipays://platformapi/startapp?appId=66666733
            // 基金组合：
            // alipays://platformapi/startapp?appId=66666735
            // 财富社区：
            // alipays://platformapi/startapp?appId=66666741
            // 口碑平台弹层：
            // alipays://platformapi/startapp?appId=66666742
            // 定时转账提醒：
            // alipays://platformapi/startapp?appId=66666743
            // 店铺详情页报错：
            // alipays://platformapi/startapp?appId=66666749
            // 保险号：
            // alipays://platformapi/startapp?appId=66666750
            // 商圈券包：
            // alipays://platformapi/startapp?appId=66666754
            // 我的健康：
            // alipays://platformapi/startapp?appId=66666755
            // 国际支付成功页：
            // alipays://platformapi/startapp?appId=66666757
            // 流量钱包
            // alipays://platformapi/startapp?appId=66666759
            // 消费捐：
            // alipays://platformapi/startapp?appId=66666761
            // 车金融：
            // alipays://platformapi/startapp?appId=66666762
            // 阿里智能：
            // alipays://platformapi/startapp?appId=66666773
            // 商家说：
            // alipays://platformapi/startapp?appId=66666774
            // 境外收款：
            // alipays://platformapi/startapp?appId=66666777
            // 懒人一键理财：
            // alipays://platformapi/startapp?appId=66666779
            // 支付宝刷脸付：
            // alipays://platformapi/startapp?appId=66666781
            // 蚂蚁庄园星星球：
            // alipays://platformapi/startapp?appId=66666782
            // 爱攒油加油站：
            // alipays://platformapi/startapp?appId=66666783
            // 亲情圈：
            // alipays://platformapi/startapp?appId=66666784
            // 飞猪酒店：
            // alipays://platformapi/startapp?appId=66666787
            // 火车票正向主流程：
            // alipays://platformapi/startapp?appId=66666788
            // 商家经营分析：
            // alipays://platformapi/startapp?appId=66666791
            // 人传人转账拉新
            // alipays://platformapi/startapp?appId=66666796
            // 飞猪国际机票WEEX：
            // alipays://platformapi/startapp?appId=66666807
            // 芝麻认证小程序：
            // alipays://platformapi/startapp?appId=66666808
            // 财富通用工具：
            // alipays://platformapi/startapp?appId=66666810
            // 小钱袋：
            // alipays://platformapi/startapp?appId=66666816
            // Tinyjs资源：
            // alipays://platformapi/startapp?appId=66666817
            // 财富标签页：
            // alipays://platformapi/startapp?appId=66666825
            // 泛行业会场：
            // alipays://platformapi/startapp?appId=66666827
            // 小富婆：
            // alipays://platformapi/startapp?appId=66666829
            // 一字千金：
            // alipays://platformapi/startapp?appId=66666831
            // 招牌来了：
            // alipays://platformapi/startapp?appId=66666860
            // 直播频道：
            // alipays://platformapi/startapp?appId=66666861
            // 口碑快消频道页：
            // alipays://platformapi/startapp?appId=66666865
            // 智能设备：
            // alipays://platformapi/startapp?appId=66666877
            // 淘票票H5资讯：
            // alipays://platformapi/startapp?appId=66666881
            // 口碑资源加速包一：
            // alipays://platformapi/startapp?appId=66666884
            // 国际机票交易：
            // alipays://platformapi/startapp?appId=66666888
            // 工资理财：
            // alipays://platformapi/startapp?appId=66666897
            // 银行卡：
            // alipays://platformapi/startapp?appId=68686988
            // 2018五福首页：
            // alipays://platformapi/startapp?appId=68687002
            // 2018新春集五福：
            // alipays://platformapi/startapp?appId=68687028
            // 信用租承接：
            // alipays://platformapi/startapp?appId=68687032
            // appxNativeIOS框架包：
            // alipays://platformapi/startapp?appId=68687035
            // 蚂蚁星愿：
            // alipays://platformapi/startapp?appId=68687049
            // Apple 专区：
            // alipays://platformapi/startapp?appId=68687052
            // 养老金：
            // alipays://platformapi/startapp?appId=68687131
            // 人脸、指纹、声纹：
            // alipays://platformapi/startapp?appId=68687140
            // 安全设置：
            // alipays://platformapi/startapp?appId=68687141
            // 支付宝授权：
            // alipays://platformapi/startapp?appId=68687142
            // 股票发现-支付宝：
            // alipays://platformapi/startapp?appId=68687145
            // 小程序收藏：
            // alipays://platformapi/startapp?appId=68687164
            // 信用受理台：
            // alipays://platformapi/startapp?appId=68687167
            // 星巴克用星说：
            // alipays://platformapi/startapp?appId=77700096
            // 小程序分享二维码：
            // alipays://platformapi/startapp?appId=77700109

            #endregion
        }
        
        public static void FileSave(string p_Path, string p_Content, bool p_Append = true)
        {
            FileSave(p_Path, Encoding.UTF8.GetBytes(p_Content), p_Append);
        }

        public static void FileSave(string p_Path, byte[] p_Bytes, bool p_Append = true)
        {
            Task.Factory.StartNew((state) =>
            {
                var obj = (dynamic) state;
                try
                {
                    if (!obj.fileinfo.Directory.Exists)
                    {
                        obj.fileinfo.Directory.Create();
                    }

                    if (obj.append)
                    {
                        using (var fs = new FileStream(obj.fileinfo.FullName, FileMode.Append))
                        {
                            fs.Seek(0, SeekOrigin.End);
                            fs.Write(obj.bytes, 0, obj.bytes.Length);
                            fs.Flush();
                        }
                    }
                    else
                    {
                        using (var fs = new FileStream(obj.fileinfo.FullName, FileMode.Create))
                        {
                            fs.Seek(0, SeekOrigin.End);
                            fs.Write(obj.bytes, 0, obj.bytes.Length);
                            fs.Flush();
                        }
                    }
                }
                catch
                {
                }
            }, new {fileinfo = new FileInfo(p_Path), bytes = p_Bytes, append = p_Append});
        }


        public static Dictionary<int, string> EnumsDic<T>()
        {
            var res = new Dictionary<int, string>();
            var type = typeof(T);
            if (type.IsEnum)
            {
                foreach (dynamic item in Enum.GetValues(type))
                {
                    res.Add((int) item, item.ToString());
                }
            }

            return res;
        }

        public class enumOption
        {
            public string label { get; set; }
            public int value { get; set; }
        }
        
        public static List<enumOption> EnumsOptions<T>()
        {
            var res = new List<enumOption>();
            var type = typeof(T);
            if (type.IsEnum)
            {
                foreach (dynamic item in Enum.GetValues(type))
                {
                    res.Add(new enumOption {label = item.ToString(), value = (int) item});
                }
            }

            return res;
        }

        public static Dictionary<int, string> EnumsDescDic<T>()
        {
            var desctype = typeof(System.ComponentModel.DescriptionAttribute);
            var res = new Dictionary<int, string>();
            var type = typeof(T);
            if (type.IsEnum)
            {
                foreach (dynamic item in Enum.GetValues(type))
                {
                    System.Reflection.FieldInfo filed = type.GetField(item.ToString());
                    var desc = (System.ComponentModel.DescriptionAttribute) (filed.GetCustomAttributes(desctype, false)
                        .FirstOrDefault());
                    if (desc != null)
                    {
                        res.Add((int) item, desc.Description);
                    }
                }
            }

            return res;
        }
    }
}