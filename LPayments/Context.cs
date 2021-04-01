using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Web;

namespace LPayments
{
    public static class Context
    {
        public static Action<PayResult> NotifyEvent;

        public static readonly HashSet<IPayChannel>
            PayList = new HashSet<IPayChannel>();

        internal static bool Debug =
#if DEBUG
            true;
#else
            false;
#endif

        static Context()
        {
            var extAssembly = Assembly.GetExecutingAssembly();
            PayList = new HashSet<IPayChannel>();
            foreach (var item in extAssembly.GetTypes().Where(p=>p.IsClass && p.IsPublic ))
            {
                if (item.Type2PlatformAttribute() != null && item.Type2ChannelAttribute() != null)
                {
                    PayList.Add(Utils.Core.CreateInstance(item));
                }
            }
        }

        public static PayChannelAttribute Type2ChannelAttribute(this Type p_Type)
        {
            var cas = p_Type.GetCustomAttributes(typeof(PayChannelAttribute), false) as PayChannelAttribute[];
            return cas?.Length == 1 ? cas[0] : null;
        }

        public static PayPlatformAttribute Type2PlatformAttribute(this Type p_Type)
        {
            var cas = p_Type.GetCustomAttributes(typeof(PayPlatformAttribute), true) as PayPlatformAttribute[];
            return cas?.Length == 1 ? cas[0] : null;
        }


        public static IPayChannel Get(string p_PlatformName, EChannel p_EChannel, EPayType p_EPayType = EPayType.PC,
            string p_SettingJson = "")
        {
            var data = PayList.FirstOrDefault(p => p.PayPlatform.Name == p_PlatformName && p.PayChannnel.eChannel == p_EChannel &&
                                                 p.PayChannnel.ePayType == p_EPayType);
            if (data != null)
            {
                data.SettingsJson = p_SettingJson;

                return data;
            }

            return null;
        }

        public static string RequestLog(Microsoft.AspNetCore.Http.HttpRequest p_Request)
        {
            var LogContent = p_Request.Method + " : " + p_Request.Scheme + "://" + p_Request.Host + p_Request.Path +
                             Environment.NewLine + Environment.NewLine;

            LogContent += Environment.NewLine + "Query:" + Environment.NewLine;
            try
            {
                if (p_Request.Query.Count > 0)
                    LogContent = p_Request.Query.Aggregate(LogContent,
                        (current, item) =>
                            current + item.Key + " = " + item.Value + Environment.NewLine);
            }
            catch
            {
            }

            LogContent += Environment.NewLine + "Headers:" + Environment.NewLine;
            try
            {
                if (p_Request.Headers.Count > 0)
                    LogContent = p_Request.Headers.Aggregate(LogContent,
                        (current, item) => current + item.Key + " = " + item.Value + Environment.NewLine);
            }
            catch
            {
            }

            if (p_Request.Method == "POST")
            {
                LogContent += Environment.NewLine + "Form:" + Environment.NewLine;
                try
                {
                    if (p_Request.Form.Count > 0)
                        LogContent = p_Request.Form.Aggregate(LogContent,
                            (current, item) => current + item.Key + " = " + item.Value + Environment.NewLine);
                }
                catch
                {
                }

                if (p_Request.ContentLength > 0)
                {
                    using (var buffer = new MemoryStream())
                    {
                        p_Request.Body.CopyTo(buffer);
                        if (p_Request.Body.CanSeek)
                        {
                            p_Request.Body.Seek(0, SeekOrigin.Begin);
                        }

                        LogContent += Environment.NewLine + "Request.Body:" + Environment.NewLine;
                        LogContent += "UTF8:" + Environment.NewLine;
                        LogContent += Encoding.UTF8.GetString(buffer.ToArray()) + Environment.NewLine;
                    }
                }
            }

            LogContent += Environment.NewLine + "Cookies:" + Environment.NewLine;
            try
            {
                if (p_Request.Cookies.Count > 0)
                    LogContent = p_Request.Cookies.Aggregate(LogContent,
                        (current, item) =>
                            current + item.Key + " = " + item.Value + Environment.NewLine);
            }
            catch
            {
            }


            return LogContent;
        }


//#region 配置读写

//        public static string Config<T>()
//        {
//            return Config(typeof(T).Type2ChannelAttribute().Name);
//        }

//        public static string Config(ChannelAttribute p_ChannelAttribute)
//        {
//            return Config(p_ChannelAttribute.Name);
//        }

//        private static string Config(string p_Name)
//        {
//            return Settings.ContainsKey(p_Name) ? Settings[p_Name] : "";
//        }

//        public static void Config<T>(string p_SettingsJson)
//        {
//            Config(typeof(T).Type2ChannelAttribute().Name, p_SettingStr);
//        }

//        public static void Config(ChannelAttribute p_ChannelAttribute, string p_SettingStr)
//        {
//            Config(p_ChannelAttribute.Name, p_SettingStr);
//        }

//        private static void Config(string p_Name, string p_SettingStr)
//        {
//            Settings[p_Name] = p_SettingStr;
//        }

//        public static void WriteConfig()
//        {
//            foreach (var kv in Settings)
//                if (DllConfig.AppSettings.Settings[kv.Key] != null)
//                    DllConfig.AppSettings.Settings[kv.Key].Value = kv.Value;
//                else
//                    DllConfig.AppSettings.Settings.Add(kv.Key, kv.Value);
//            if (!string.IsNullOrWhiteSpace(NotifyDomainStr))
//                DllConfig.AppSettings.Settings["NotifyDomain"].Value = NotifyDomainStr;
//            DllConfig.Save();
//        }

//#endregion 配置读写
    }
}