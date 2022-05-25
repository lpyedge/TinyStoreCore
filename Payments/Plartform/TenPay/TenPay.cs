//
//
// using System;
// using System.Collections.Generic;
// using System.IO;
// using System.Linq;
// using System.Net;
// using System.Text;
// using System.Web;
//
// namespace Payments.Plartform.TenPay
// {
//     [PayChannel("财付通", EChannel.TenPay)]
//     public class TenPay : IPayChannel
//     {
//         public const string PID = "PID";
//         public const string Key = "Key";
//
//         private const string GATEWAY_Pay = "https://gw.tenpay.com/gateway/pay.htm";
//         private const string GATEWAY_Notify = "https://gw.tenpay.com/gateway/simpleverifynotifyid.xml";
//
//         public TenPay()
//         {
//             Init();
//         }
//
//         public TenPay(string p_SettingsJson)
//         {
//             Init();
//             if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
//         }
//
//         protected override void Init()
//         {
//             Settings = new List<Setting>
//             {
//                 new Setting {Name = PID, Description = "财付通的商户号", Regex = @"^[\w\.@]+$", Requied = true},
//                 new Setting {Name = Key, Description = "财付通的安全检验码(Key)", Regex = @"^\w+$", Requied = true}
//             };
//
//             Currencies = new List<ECurrency>
//             {
//                 ECurrency.CNY
//             };
//         }
//
//         public override PayResult Notify(Microsoft.AspNetCore.Http.HttpContext context)
//         {
//             return base.Notify(context);
//         }
//
//         public override PayResult Notify(IDictionary<string, string> form, IDictionary<string, string> query,
//             IDictionary<string, string> head, string body, string notifyip)
//         {
//             PayResult result = new PayResult
//             {
//                 Status = PayResult.EStatus.Pending,
//                 Message = "fail"
//             };
//
//             var IsVlidate = false;
//             if (form["partner"] == this[PID] &&
//                 (form["trade_state"] == "0" || form["pay_info"] == ""))
//             {
//                 //获取返回时的签名验证结果
//                 var IsSign = ValidateStr(form, form["sign"], this[Key]);
//                 //获取是否是财付通服务器发来的请求的验证结果
//                 var ResponseTxt = "<retcode>0</retcode>";
//                 //网络通信错误或者是海外服务器容易返回false，保证key不暴漏的情况下可以不验证
//                 if (!string.IsNullOrWhiteSpace(form["notify_id"]))
//                     ResponseTxt = GetResponseTxt(this[PID], this[Key], form["notify_id"]);
//
//                 //判断responsetTxt是否包含<retcode>0</retcode>，isSign是否为true
//                 //responsetTxt的结果不是true，与服务器设置问题、合作身份者ID、notify_id一分钟失效有关
//                 //isSign不是true，与安全校验码、请求时的参数格式（如：带自定义参数等）、编码格式有关
//                 IsVlidate = ResponseTxt.Contains("<retcode>0</retcode>") && IsSign;
//             }
//
//             if (IsVlidate)
//             {
//                 result = new PayResult
//                 {
//                     OrderName = form["attach"],
//                     OrderID = form["out_trade_no"],
//                     Amount =  double.Parse(form["total_fee"]) / 100,
//                     Tax = -1,
//                     Currency = ECurrency.CNY,
//                     Business = form["partner"],
//                     TxnID = form["transaction_id"],
//                     PaymentName = Name,
//                     PaymentDate = DateTime.UtcNow,
//
//                     Message = "OK",
//
//                     Customer = new PayResult._Customer
//                     {
//                         Business = form.ContainsKey("buyer_alias") ? form["buyer_alias"] : "",
//                     }
//                 };
//             }
//             return result;
//         }
//
//         public override PayTicket Pay(string p_OrderId, double p_Amount,
//             ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
//             string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
//         {
//             if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");
//
//             if (string.IsNullOrEmpty(this[PID])) throw new ArgumentNullException("PID");
//             if (string.IsNullOrEmpty(this[Key])) throw new ArgumentNullException("Key");
//
//             if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");
//
//             var sPara = new Dictionary<string, string>();
//             //构造签名参数数组
//             sPara.Add("partner", this[PID]); //商户号
//             sPara.Add("out_trade_no", p_OrderId); //商家订单号
//             sPara.Add("total_fee", (p_Amount * 100).ToString("0")); //商品金额,以分为单位
//
//             sPara.Add("return_url",p_ReturnUrl);
//             sPara.Add("notify_url",p_NotifyUrl);
//
//             sPara.Add("body", p_OrderName); //商品描述
//             sPara.Add("bank_type", "DEFAULT"); //银行类型(中介担保时此参数无效)
//             sPara.Add("spbill_create_ip", p_ClientIP.ToString()); //用户的公网ip，不是商户服务器IP
//             sPara.Add("fee_type", "1"); //币种，1人民币
//             sPara.Add("subject", p_OrderName); //商品名称(中介交易时必填)
//             //系统可选参数
//             sPara.Add("sign_type", "MD5");
//             sPara.Add("service_version", "1.0");
//             sPara.Add("input_charset", "UTF-8");
//             sPara.Add("sign_key_index", "1");
//             //业务可选参数
//             sPara.Add("attach", p_OrderName); //附加数据，原样返回
//             sPara.Add("product_fee", (p_Amount * 100).ToString("0")); //商品费用，必须保证transport_fee + product_fee=total_fee
//             sPara.Add("transport_fee", "0"); //物流费用，必须保证transport_fee + product_fee=total_fee
//             sPara.Add("time_start", DateTime.UtcNow.AddHours(8).ToString("yyyyMMddHHmmss")); //订单生成时间，格式为yyyymmddhhmmss
//             sPara.Add("time_expire", ""); //订单失效时间，格式为yyyymmddhhmmss
//             sPara.Add("buyer_id", ""); //买方财付通账号
//             sPara.Add("goods_tag", ""); //商品标记
//             sPara.Add("trade_mode", "1"); //交易模式，1即时到账(默认)，2中介担保，3后台选择（买家进支付中心列表选择）
//             sPara.Add("transport_desc", ""); //物流说明
//             sPara.Add("trans_type", "2"); //交易类型，1实物交易，2虚拟交易
//             sPara.Add("agentid", ""); //平台ID
//             sPara.Add("agent_type", "0"); //代理模式，0无代理(默认)，1表示卡易售模式，2表示网店模式
//             sPara.Add("seller_id", ""); //卖家商户号，为空则等同于partner
//
//             var formhtml =
//                 new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
//                                   "' action='" +
//                                   GATEWAY_Pay + "' method='post' >");
//             foreach (var temp in sPara)
//                 formhtml.Append("<input type='hidden' name='" + temp.Key + "' value='" + temp.Value + "'/>");
//             formhtml.Append("<input type='hidden' name='sign' value='" + Build_MD5Sign(sPara, this[Key]) + "'/>");
//             formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
//             formhtml.Append("</form>");
//
//             var pt = new PayTicket();
//             pt.FormHtml = formhtml.ToString();
//             return pt;
//         }
//
//         #region 财付通签名方法
//
//         /// <summary>
//         ///     生成签名结果
//         /// </summary>
//         /// <param name="p_SortedDictionary">要签名的数组</param>
//         /// <param name="p_Key">安全校验码</param>
//         /// <returns>签名结果字符串</returns>
//         internal static string Build_MD5Sign(IDictionary<string, string> p_SortedDictionary, string p_Key)
//         {
//             return Utils.Core.MD5(CreateLinkString(FilterPara(p_SortedDictionary)) + "&key=" + p_Key);
//         }
//
//         /// <summary>
//         ///     把数组所有元素，按照“参数=参数值”的模式用“&”字符拼接成字符串
//         /// </summary>
//         /// <param name="sArray">需要拼接的数组</param>
//         /// <returns>拼接完成以后的字符串</returns>
//         internal static string CreateLinkString(IDictionary<string, string> dicArray)
//         {
//             var prestr = new StringBuilder();
//             foreach (var temp in dicArray.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value))
//                 prestr.Append(temp.Key + "=" + temp.Value + "&");
//             return prestr.ToString().TrimEnd('&');
//         }
//
//         /// <summary>
//         ///     除去数组中的空值和签名参数并以字母a到z的顺序排序
//         /// </summary>
//         /// <param name="dicArrayPre">过滤前的参数组</param>
//         /// <returns>过滤后的参数组</returns>
//         internal static Dictionary<string, string> FilterPara(IDictionary<string, string> dicArrayPre)
//         {
//             return
//                 dicArrayPre.Where(
//                     temp =>
//                         !string.Equals(temp.Key.ToLower(), "sign", StringComparison.OrdinalIgnoreCase) &&
//                         !string.Equals(temp.Key.ToLower(), "key", StringComparison.OrdinalIgnoreCase) &&
//                         !string.IsNullOrEmpty(temp.Value)).ToDictionary(temp => temp.Key, temp => temp.Value);
//         }
//
//         internal static bool ValidateStr(IDictionary<string, string> p_SortedDictionary, string p_VerifySign,
//             string p_Key)
//         {
//             if (!string.IsNullOrWhiteSpace(p_VerifySign))
//                 return string.Equals(Build_MD5Sign(p_SortedDictionary, p_Key), p_VerifySign,
//                     StringComparison.OrdinalIgnoreCase);
//             return false;
//         }
//
//         internal static string GetResponseTxt(string p_Partner, string p_Key, string p_NotifyId)
//         {
//             //获取远程服务器ATN结果，验证是否是财付通服务器发来的请求
//
//             var sPara = new Dictionary<string, string>();
//             sPara.Add("notify_id", p_NotifyId);
//             sPara.Add("partner", p_Partner);
//
//             return
//                 Get_Http(
//                     GATEWAY_Notify + "?partner=" + HttpUtility.UrlEncode(p_Partner, Encoding.UTF8) + "&notify_id=" +
//                     HttpUtility.UrlEncode(p_NotifyId, Encoding.UTF8) + "&sign=" +
//                     HttpUtility.UrlEncode(Build_MD5Sign(sPara, p_Key), Encoding.UTF8), 120000);
//         }
//
//         /// <summary>
//         ///     获取远程服务器ATN结果
//         /// </summary>
//         /// <param name="strUrl">指定URL路径地址</param>
//         /// <param name="timeout">超时时间设置</param>
//         /// <returns>服务器ATN结果</returns>
//         internal static string Get_Http(string strUrl, int timeout)
//         {
//             string strResult;
//             try
//             {
//                 var myReq = (HttpWebRequest)WebRequest.Create(strUrl);
//                 myReq.Method = "POST";
//                 myReq.Timeout = timeout;
//                 var HttpWResp = (HttpWebResponse)myReq.GetResponse();
//                 var myStream = HttpWResp.GetResponseStream();
//                 var sr = new StreamReader(myStream, Encoding.Default);
//                 var strBuilder = new StringBuilder();
//                 while (-1 != sr.Peek())
//                     strBuilder.Append(sr.ReadLine());
//
//                 strResult = strBuilder.ToString();
//             }
//             catch (Exception exp)
//             {
//                 strResult = "错误：" + exp.Message;
//             }
//
//             return strResult;
//         }
//
//         ///// <summary>
//         ///// 防钓鱼时间戳
//         ///// </summary>
//         ///// <param name="p_Partner"></param>
//         ///// <returns></returns>
//         //public static string Query_timestamp(string p_Partner)
//         //{
//         //    string url = GATEWAY_Pay + "?service=query_timestamp&partner=" + p_Partner + "&_input_charset=utf-8";
//         //    string encrypt_key = "";
//
//         //    XmlTextReader Reader = new XmlTextReader(url);
//         //    XmlDocument xmlDoc = new XmlDocument();
//         //    xmlDoc.Load(Reader);
//
//         //    encrypt_key = xmlDoc.SelectSingleNode("/TenPay/response/timestamp/encrypt_key").InnerText;
//
//         //    return encrypt_key;
//         //}
//
//         #endregion 财付通签名方法
//     }
// }