using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace LPayments.Utils
{
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
    internal static class Json
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
}