using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace LPayments.Plartform.AliPayO
{
    [PayPlatformAttribute("支付宝旧版", "", SiteUrl = "https://www.alipay.com")]
    [PayChannel(EChannel.AliPayBatch)]
    public class Pay_Batch : IPayChannel, IPay
    {
        public const string PID = "PID";
        public const string Key = "Key";

        public Pay_Batch() : base()
        {
        }

        public Pay_Batch(string p_SettingsJson) : this()
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
            var dictionary = form;
            var IsVlidate = false;
            if (dictionary["seller_id"] == this[PID] &&
                (dictionary["trade_status"] == "TRADE_FINISHED" || dictionary["trade_status"] == "TRADE_SUCCESS"))
            {
                //获取返回时的签名验证结果
                var IsSign = ValidateStr(dictionary, dictionary["sign"], this[Key]);
                //获取是否是支付宝服务器发来的请求的验证结果
                var ResponseTxt = "true";
                //网络通信错误或者是海外服务器容易返回false，保证key不暴漏的情况下可以不验证
                //if (!string.IsNullOrWhiteSpace(dictionary["notify_id"]))
                //{
                //    ResponseTxt = GetResponseTxt(m_Partner, dictionary["notify_id"]);
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
                    OrderName = dictionary["subject"],
                    OrderID = dictionary["out_trade_no"],
                    Amount = Double.Parse(dictionary["total_fee"]),
                    Tax = -1,
                    Currency = ECurrency.CNY,
                    Business = dictionary["seller_email"],
                    TxnID = dictionary["trade_no"],
                    PaymentName = Name,
                    PaymentDate = DateTime.UtcNow,

                    Message = "success",

                    Customer = new PayResult._Customer
                    {
                        Business = dictionary.ContainsKey("buyer_email") ? dictionary["buyer_email"] : "",
                    }
                };
            }

            return result;
        }

        public virtual PayTicket Pay(string p_OrderId, double p_Amount,
            ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
        {
            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");

            if (string.IsNullOrEmpty(this[PID])) throw new ArgumentNullException("PID");
            if (string.IsNullOrEmpty(this[Key])) throw new ArgumentNullException("Key");
            if (extend_params == null) throw new ArgumentNullException("extend_params");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");
            if (!(extend_params is PayExtend)) throw new ArgumentException("extend_params must be PayExtend");
            if ((p_OrderId.Length < 11) | (p_OrderId.Length > 32))
                throw new ArgumentException("p_OrderId must between 11 and 32");

            var datas = new Dictionary<string, string>();
            //构造签名参数数组
            datas.Add("service", "batch_trans_notify");
            datas.Add("partner", this[PID]);
            datas.Add("_input_charset", "utf-8");
            datas.Add("notify_url", p_NotifyUrl);
            datas.Add("batch_no", p_OrderId);

            datas.Add("sign_type", "MD5");

            datas.Add("total_fee", p_Amount.ToString("0.##"));
            //sPara.Add("exter_invoke_ip", p_ClientIP.ToString());

            var pe = extend_params as PayExtend;
            if (pe != null)
                if (!string.IsNullOrWhiteSpace(pe.PayName) && !string.IsNullOrWhiteSpace(pe.PayAccount) &&
                    pe.PayDetails.Count > 0)
                {
                    datas.Add("account_name", pe.PayName);
                    datas.Add("Email ", pe.PayAccount);
                    datas.Add("batch_num", pe.PayDetails.Count.ToString());
                    datas.Add("batch_fee", pe.PayDetails.Aggregate(0.00, (x, y) => x + y.Amount).ToString("0.00"));

                    var detail_data = "";
                    for (var i = 0; i < pe.PayDetails.Count; i++)
                        detail_data += string.Format("{0}^{1}^{2}^{3}", i, pe.PayDetails[i].Account,
                            pe.PayDetails[i].Name, pe.PayDetails[i].Amount.ToString("0.00"));

                    datas.Add("detail_data", detail_data);
                }
            
            var sign = Build_MD5Sign(datas, this[Key]);
            datas.Add("sign_type", "MD5");
            datas.Add("sign", sign);

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
            }

            /// <summary>
            ///     付款账户Email
            /// </summary>
            public string PayAccount { get; set; }

            /// <summary>
            ///     付款姓名
            /// </summary>
            public string PayName { get; set; }

            public List<PayDetail> PayDetails { get; set; }

            public class PayDetail
            {
                /// <summary>
                ///     收款账号
                /// </summary>
                public string Account { get; set; }

                /// <summary>
                ///     收款姓名
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                ///     收款金额
                /// </summary>
                public double Amount { get; set; }
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

        #endregion 支付宝签名方法
    }
}