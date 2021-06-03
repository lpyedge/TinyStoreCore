// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net;
// using System.Text;
// using System.Web;
// using LPayments.Utils;
//
// namespace LPayments.Plartform.VZhiPay
// {
//     //商户号
//     //000010019004000002
//     //密钥
//     //3d1cae5034ad47a2ae6c969ae95680a3
//
//     public abstract class _VZhiPay : IPayChannel
//     {
//         public const string MerchantId = "MerchantId";
//         public const string SecretKey = "SecretKey";
//
//         protected bool m_qrcode = false;
//         protected string m_channel = "";
//         protected string m_bankcode = "";
//         protected string m_scenesType = "";
//
//         protected _VZhiPay()
//         {
//             Init();
//         }
//
//         protected _VZhiPay(string p_SettingsJson)
//         {
//             Init();
//             if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
//         }
//
//         protected override void Init()
//         {
//             Settings = new List<Setting>
//             {
//                 new Setting {Name = MerchantId, Description = "惊鸿云的商户号", Regex = @"^\w+$", Requied = true},
//                 new Setting {Name = SecretKey, Description = "惊鸿云的商户密钥", Regex = @"^\w+$", Requied = true}
//             };
//
//             Currencies = new List<ECurrency>
//             {
//                 ECurrency.CNY,
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
//             var signstr = form.Where(p => p.Key != "sign").OrderBy(p => p.Key)
//                 .Aggregate("", (x, y) => x + y.Key + "=" + y.Value + "&") + $"key={this[SecretKey]}";
//
//             if (string.Equals(form["sign"], Utils.Core.MD5(signstr), StringComparison.OrdinalIgnoreCase) &&
//                 form["orderStatus"] == "1")
//             {
//                 result = new PayResult
//                 {
//                     OrderName = "",
//                     OrderID = form["merOrderNo"],
//                     Amount = double.Parse(form["transAmt"]),
//                     Tax = -1,
//                     Currency = ECurrency.CNY,
//                     Business = this[MerchantId],
//                     TxnID = form["orderNo"],
//                     PaymentName = Name,
//                     PaymentDate = DateTime.UtcNow,
//
//                     Message = "SUCCESS",
//                 };
//             }
//
//             return result;
//         }
//
//         public override PayTicket Pay(string p_OrderId, double p_Amount,
//             ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
//             string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
//         {
//             if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");
//
//             if (string.IsNullOrEmpty(this[MerchantId])) throw new ArgumentNullException("MerchantId");
//             if (string.IsNullOrEmpty(this[SecretKey])) throw new ArgumentNullException("SecretKey");
//             if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");
//
//             var pt = new PayTicket();
//
//             var uri = new Uri("http://mch.vzhipay.com/cloud/cloudplatform/api/trade.html");
//
//             var dic = new Dictionary<string, string>
//             {
//                 ["tradeType"] = "cs.pay.submit",
//                 ["version"] = "1.5",
//                 ["mchId"] = this[MerchantId],
//
//                 //支付方式:
//                 //wxPub    微信公众账号支付
//                 //wxPubQR 微信扫码支付
//                 //wxApp   微信app支付
//                 //wxMicro 微信付款码支付
//                 //wxH5    微信H5支付
//                 //alipayQR    支付宝扫码支付
//                 //alipayApp   支付宝APP支付
//                 //alipayMicro 支付宝付款码支付
//                 //alipayH5    支付宝服务窗
//                 //qpay    快捷支付
//                 //jdPay   京东支付
//                 //jdGateway   京东网关
//                 //jdMicro 京东付款码支付
//                 //jdQR    京东扫码支付
//                 //suningQr    苏宁扫码
//                 //suningMicro 苏宁付款码
//                 //gateway 网关
//                 //qqPub   QQ公众号
//                 //qqH5    QQH5
//                 //qqWap   qqWAP
//                 //qqQr    QQ扫码支付
//                 ["channel"] = m_channel,
//                 ["timePaid"] = DateTime.Now.ToString("yyyyMMddHHmmss"),
//                 ["timeExpire"] = DateTime.Now.AddHours(1).ToString("yyyyMMddHHmmss"),
//
//                 ["outTradeNo"] = p_OrderId,
//                 ["amount"] = p_Amount.ToString("0.00"),
//                 ["notifyUrl"] = p_NotifyUrl,
//                 ["callbackUrl"] = p_ReturnUrl,
//             };
//             if (m_channel == "gateway" || m_channel == "qpay")
//             {
//                 dic["bankType"] = m_bankcode;
//                 dic["accountType"] = "1";
//                 dic["scenesType"] = m_scenesType;
//             }
//
//
//             var signstr = dic.OrderBy(p => p.Key).Aggregate("",
//                               (x, y) => string.IsNullOrWhiteSpace(y.Value) ? x + "" : x + y.Key + "=" + y.Value + "&") +
//                           $"key={this[SecretKey]}";
//             dic["sign"] = Utils.Core.MD5(signstr).ToUpperInvariant();
//             if (m_qrcode)
//             {
//                 var res = _HWU.Response(uri, HttpWebUtility.HttpMethod.Post, dic);
//                 if (res.Contains("\"code\":\"000000\""))
//                 {
//                     //{"busContent":"weixin://wxpay/bizpayurl?pr=TzE7Rjt","contentType":"01","orderNo":"20170615124414956972","merOrderNo":"636331273905756443","consumerNo":"12004","transAmt":"0.50","orderStatus":"0","code":"000000","msg":"success","sign":"668EE27187A6C78A002F67CD75307444"}
//                     var json = Utils.DynamicJson.Parse(res);
//                     var tempdic = new Dictionary<string, string>();
//                     foreach (var item in res.Trim('{', '}', '"').Split(new[] {"\",\""}, StringSplitOptions.None))
//                     {
//                         var kv = item.Split(new[] {"\":\""}, StringSplitOptions.None);
//                         tempdic[kv[0]] = kv[1];
//                     }
//
//                     signstr = tempdic.Where(p => p.Key != "sign").OrderBy(p => p.Key)
//                         .Aggregate("", (x, y) => x + y.Key + "=" + y.Value + "&") + $"key={this[SecretKey]}";
//
//                     if (string.Equals(tempdic["sign"], Utils.Core.MD5(signstr), StringComparison.OrdinalIgnoreCase))
//                     {
//                         pt.Url = json.busContent;
//
//                         //string imgbase64 = Core.QR(pt.Url, this.GetType());
//                         //pt.FormHtml = Core.FormQR(imgbase64, p_OrderId, p_Amount, p_OrderName);
//
//                         //pt.Extra = imgbase64;
//                     }
//                     else
//                     {
//                         pt.Message = "验签失败!!!" + res;
//                     }
//                 }
//                 else
//                 {
//                     pt.Message = res;
//                 }
//             }
//             else
//             {
//                 var res = _HWU.Response(uri, HttpWebUtility.HttpMethod.Post, dic);
//                 var formhtml =
//                     new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
//                                       "' action='" + uri + "' method='post' >");
//                 foreach (var item in dic)
//                 {
//                     formhtml.AppendFormat("<input type='hidden' name='{0}' value='{1}' />", item.Key, item.Value);
//                 }
//
//                 formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
//                 formhtml.Append("</form>");
//                 pt.FormHtml = formhtml.ToString();
//             }
//
//             return pt;
//         }
//     }
// }