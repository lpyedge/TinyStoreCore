using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TinyStore
{
    public static class Global
    {
        static Global()
        {
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