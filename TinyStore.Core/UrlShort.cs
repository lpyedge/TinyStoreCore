using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace TinyStore
{
    public class UrlShort
    {
        public static string Excute(string url, int type = 0)
        {
            switch (type)
            {
                case 0:
                    {
                        using (var wc = new System.Net.WebClient())
                        {
                            return wc.DownloadString("http://sa.sogou.com/gettiny?url=" + Uri.EscapeDataString(url));
                        }
                    }
                case 1:
                    {
                        using (var wc = new System.Net.WebClient())
                        {
                            return wc.DownloadString("http://tinyurl.com/api-create.php?url=" + Uri.EscapeDataString(url));
                        }
                    }
                case 2:
                    {
                        using (var wc = new System.Net.WebClient())
                        {
                            var response = wc.UploadString("http://suo.la", "surl_target=" + url);
                            if (response.Contains("\"Code\":0"))
                            {
                                Regex regex = new Regex("(?<url>http://suo\\.la/\\w+)", RegexOptions.IgnoreCase);
                                var matchs = regex.Matches(response);
                                if (matchs.Count == 0)
                                    return "";
                                return matchs[0].Groups["url"].Value;
                            }
                            return "";
                        }
                    }
                case 7:
                    {
                        using (var wc = new System.Net.WebClient())
                        {
                            var response = wc.DownloadString("http://api.t.sina.com.cn/short_url/shorten.xml?source=3271760578&url_long=" + Uri.EscapeDataString(url));
                            Regex regex = new Regex("<url_short>(?<url>[\\w:/\\.]+)</url_short>", RegexOptions.IgnoreCase);
                            var matchs = regex.Matches(response);
                            if (matchs.Count == 0)
                                return "";
                            return matchs[0].Groups["url"].Value;
                        }
                    }
                case 8:
                    {
                        //百度短网址
                        //https://dwz.cn/console/userinfo
                        using (var wc = new System.Net.WebClient())
                        {
                            wc.Headers["Token"] = "12939d535cb240d53b2ee44da751030c";
                            var response = wc.UploadString("https://dwz.cn/admin/v2/create", "{\"Url\":\"" + url + "\",\"TermOfValidity\":\"1-year\"}");
                            if (response.Contains("\"Code\":0"))
                            {
                                Regex regex = new Regex("\"ShortUrl\":\"(?<url>[\\w:/\\.]+)\",", RegexOptions.IgnoreCase);
                                var matchs = regex.Matches(response);
                                if (matchs.Count == 0)
                                    return "";
                                return matchs[0].Groups["url"].Value;
                            }
                            return "";
                        }
                    }
                default:
                    return "";
            }
        }
    }    
}
