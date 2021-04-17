using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Http;

namespace TinyStore.Utils
{
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
}