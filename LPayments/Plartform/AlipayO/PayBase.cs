using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace LPayments.Plartform.AliPayO
{
    [PayPlatformAttribute("支付宝旧版", "", SiteUrl = "https://www.alipay.com",NotifyProxy = true)]
    [PayChannel(EChannel.AliPay)]
    public class PayBase : IPayChannel,IPay
    {
        public const string PID = "PID";
        public const string Key = "Key";

        public PayBase() : base()
        {
        }

        public PayBase(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = PID, Description = "支付宝的合作伙伴身份（PID）", Regex = @"^\d+$", Requied = true},
                new Setting {Name = Key, Description = "支付宝的安全校验码（Key）MD5密钥", Regex = @"^\w+$", Requied = true}
            };

            Currencies = new List<ECurrency>
            {
                ECurrency.CNY
            };
        }

        public PayResult Notify(IDictionary<string, string> form, IDictionary<string, string> query,
            IDictionary<string, string> head, string body, string notifyip)
        {
            PayResult result = new PayResult
            {
                Status = PayResult.EStatus.Pending,
                Message = "fail"
            };
            var IsVlidate = false;
            if (form["seller_id"] == this[PID] &&
                (form["trade_status"] == "TRADE_FINISHED" || form["trade_status"] == "TRADE_SUCCESS"))
            {
                //获取返回时的签名验证结果
                var IsSign = ValidateStr(form, form["sign"], this[Key]);
                //获取是否是支付宝服务器发来的请求的验证结果
                var ResponseTxt = "true";
                //网络通信错误或者是海外服务器容易返回false，保证key不暴漏的情况下可以不验证
                //if (!string.IsNullOrWhiteSpace(dictionary["notify_id"]))
                //{
                //    ResponseTxt = GetResponseTxt(m_PID, dictionary["notify_id"]);
                //}

                //写日志记录（若要调试，请取消下面两行注释）
                //string sWord = "responseTxt=" + responseTxt + "\n isSign=" + isSign.ToString() + "\n 返回回来的参数：" + GetPreSignStr(inputPara) + "\n ";
                //Core.LogResult(sWord);

                //判断responsetTxt是否为true，isSign是否为true
                //responsetTxt的结果不是true，与服务器设置问题、合作身份者ID、notify_id一分钟失效有关
                //isSign不是true，与安全校验码、请求时的参数格式（如：带自定义参数等）、编码格式有关
                IsVlidate = string.Equals(ResponseTxt, "true", StringComparison.OrdinalIgnoreCase) && IsSign;
            }

            if (IsVlidate)
            {
                result = new PayResult
                {
                    OrderName = form["subject"],
                    OrderID = form["out_trade_no"],
                    Amount = double.Parse(form["total_fee"]),
                    Tax = -1,
                    Currency = ECurrency.CNY,
                    Business = form.ContainsKey("seller_email") ? form["seller_email"] : "",
                    TxnID = form["trade_no"],
                    PaymentName = Name + (form.ContainsKey("out_channel_inst") ? form["out_channel_inst"] : ""),
                    PaymentDate = DateTime.UtcNow,
                    Info = form.ContainsKey("body") ? form["body"] : "",

                    Message = "success",

                    Customer = new PayResult._Customer
                    {
                        Business = form.ContainsKey("buyer_email") ? form["buyer_email"] : "",
                    }
                };
            }

            return result;
        }

        public PayTicket Pay(string p_OrderId, double p_Amount,
            ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
        {
            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");

            if (string.IsNullOrEmpty(this[PID])) throw new ArgumentNullException(PID);
            if (string.IsNullOrEmpty(this[Key])) throw new ArgumentNullException(Key);
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("p_Currency is not allowed!");
            if (p_OrderName.Length > 128) throw new ArgumentException("OrderName must less than 128!");

            var datas = new Dictionary<string, string>();
            //构造签名参数数组
            datas.Add("partner", this[PID]);
            datas.Add("seller_id", this[PID]);
            datas.Add("service", "create_direct_pay_by_user");
            datas.Add("_input_charset", "utf-8");
            datas.Add("payment_type", "1");
            datas.Add("return_url", p_ReturnUrl);
            datas.Add("notify_url", p_NotifyUrl);
            datas.Add("out_trade_no", p_OrderId);
            datas.Add("subject", p_OrderName);
            datas.Add("total_fee", p_Amount.ToString("0.##"));

            //sPara.Add("exter_invoke_ip", p_ClientIP.ToString());

            var pe = extend_params as PayExtend;
            if (pe != null)
            {
                if (pe.QrMode)
                    datas.Add("qr_pay_mode", "0");

                if (pe.PayDetails.Count > 0)
                {
                    datas.Add("royalty_type", "10");
                    datas.Add("royalty_parameters",
                        pe.PayDetails.Aggregate("",
                            (current, payDetail) =>
                                current +
                                string.Format("{0}^{1}^{2}|",
                                    payDetail.Account.StartsWith("2088")
                                        ? "uid" + payDetail.Account
                                        : payDetail.Account,
                                    payDetail.Amount.ToString("0.00"), payDetail.Info)).TrimEnd('|'));
                }

                if (!string.IsNullOrWhiteSpace(pe.Token))
                    datas.Add("token", pe.Token);
                if (!string.IsNullOrWhiteSpace(pe.OpenId))
                    datas.Add("buyer_id", pe.OpenId);
                if (!string.IsNullOrWhiteSpace(pe.Body))
                    datas.Add("body", pe.Body);
            }

            var sign = Build_MD5Sign(datas, this[Key]);
            datas.Add("sign_type", "MD5");
            datas.Add("sign", sign);
            
            // var formhtml =
            //     new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
            //                       "' action='https://mapi.alipay.com/gateway.do?_input_charset=utf-8' method='post' >");
            // foreach (var temp in sPara)
            //     formhtml.Append("<input type='hidden' name='" + temp.Key + "' value='" + temp.Value + "'/>");
            // formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
            // formhtml.Append("</form>");

            // sPara.Remove("_input_charset");
            // var url = "https://mapi.alipay.com/gateway.do?_input_charset=utf-8" +
            //           sPara.Aggregate("", (o, p) => o += "&" + p.Key + "=" + Utils.HttpWebUtility.UriDataEncode(p.Value));
            
            
            //datas.Add("sign", Build_MD5Sign(datas, this[Key]));

            return new PayTicket()
            {
                Action = EAction.UrlPost,
                Uri = "https://mapi.alipay.com/gateway.do?_input_charset=utf-8",
                Datas = datas
            };
        }

        public class PayExtend
        {
            public PayExtend()
            {
                PayDetails = new List<PayDetail>();
                QrMode = false;
            }

            public bool QrMode { get; set; }

            /// <summary>
            ///     快捷登录授权令牌
            /// </summary>
            public string Token { get; set; }

            /// <summary>
            ///     快捷登录OpenID
            /// </summary>
            public string OpenId { get; set; }

            /// <summary>
            ///     订单详细说明
            /// </summary>
            public string Body { get; set; }

            /// <summary>
            ///     分润详细
            /// </summary>
            public List<PayDetail> PayDetails { get; set; }

            public class PayDetail
            {
                /// <summary>
                ///     收款账号或者快捷登录的OpenId
                /// </summary>
                public string Account { get; set; }

                /// <summary>
                ///     收款金额
                /// </summary>
                public double Amount { get; set; }

                /// <summary>
                ///     收款备注
                /// </summary>
                public string Info { get; set; }
            }
        }

        #region 支付宝签名方法

        /// <summary>
        ///     生成签名结果
        /// </summary>
        /// <param name="p_SortedDictionary">要签名的数组</param>
        /// <param name="p_Key">安全校验码</param>
        /// <returns>签名结果字符串</returns>
        internal static string Build_MD5Sign(IDictionary<string, string> p_SortedDictionary, string p_Key)
        {
            return Utils.Core.MD5(CreateLinkString(FilterPara(p_SortedDictionary)) + p_Key);
        }

        /// <summary>
        ///     把数组所有元素，按照“参数=参数值”的模式用“&”字符拼接成字符串
        /// </summary>
        /// <param name="sArray">需要拼接的数组</param>
        /// <returns>拼接完成以后的字符串</returns>
        internal static string CreateLinkString(IDictionary<string, string> dicArray)
        {
            var prestr = new StringBuilder();
            foreach (var temp in dicArray.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value))
                prestr.Append(temp.Key + "=" + temp.Value + "&");
            return prestr.ToString().TrimEnd('&');
        }

        /// <summary>
        ///     除去数组中的空值和签名参数并以字母a到z的顺序排序
        /// </summary>
        /// <param name="dicArrayPre">过滤前的参数组</param>
        /// <returns>过滤后的参数组</returns>
        internal static Dictionary<string, string> FilterPara(IDictionary<string, string> dicArrayPre)
        {
            return
                dicArrayPre.Where(
                    temp =>
                        temp.Key.ToLower() != "sign" && temp.Key.ToLower() != "sign_type" &&
                        !string.IsNullOrEmpty(temp.Value)).ToDictionary(temp => temp.Key, temp => temp.Value);
        }

        internal static bool ValidateStr(IDictionary<string, string> p_SortedDictionary, string p_VerifySign,
            string p_Key)
        {
            if (!string.IsNullOrWhiteSpace(p_VerifySign))
                return string.Equals(Build_MD5Sign(p_SortedDictionary, p_Key), p_VerifySign,
                    StringComparison.OrdinalIgnoreCase);
            return false;
        }

        internal static string GetResponseTxt(string p_Partner, string p_NotifyId)
        {
            //获取远程服务器ATN结果，验证是否是支付宝服务器发来的请求
            return Get_Http(
                "https://mapi.alipay.com/gateway.do?service=notify_verify&partner=" + p_Partner + "&notify_id=" +
                p_NotifyId,
                120000);
        }

        /// <summary>
        ///     获取远程服务器ATN结果
        /// </summary>
        /// <param name="strUrl">指定URL路径地址</param>
        /// <param name="timeout">超时时间设置</param>
        /// <returns>服务器ATN结果</returns>
        internal static string Get_Http(string strUrl, int timeout)
        {
            string strResult;
            try
            {
                var myReq = (HttpWebRequest) WebRequest.Create(strUrl);
                myReq.Timeout = timeout;
                var HttpWResp = (HttpWebResponse) myReq.GetResponse();
                var myStream = HttpWResp.GetResponseStream();
                var sr = new StreamReader(myStream, Encoding.Default);
                var strBuilder = new StringBuilder();
                while (-1 != sr.Peek())
                    strBuilder.Append(sr.ReadLine());

                strResult = strBuilder.ToString();
            }
            catch (Exception exp)
            {
                strResult = "错误：" + exp.Message;
            }

            return strResult;
        }

        ///// <summary>
        ///// 防钓鱼时间戳
        ///// </summary>
        ///// <param name="p_Partner"></param>
        ///// <returns></returns>
        //public static string Query_timestamp(string p_Partner)
        //{
        //    string url = GATEWAY + "?service=query_timestamp&partner=" + p_Partner + "&_input_charset=utf-8";
        //    string encrypt_key = "";

        //    XmlTextReader Reader = new XmlTextReader(url);
        //    XmlDocument xmlDoc = new XmlDocument();
        //    xmlDoc.Load(Reader);

        //    encrypt_key = xmlDoc.SelectSingleNode("/alipay/response/timestamp/encrypt_key").InnerText;

        //    return encrypt_key;
        //}

        #endregion 支付宝签名方法
    }
}