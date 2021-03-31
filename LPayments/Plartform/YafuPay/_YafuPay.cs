// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net;
// using System.Text;
// using System.Web;
// using LPayments.Utils;
//
// namespace LPayments.Plartform.YafuPay
// {
//     //测试商户号:12004
//     //测试商户密钥:FFFFFFFFFFFFFFFFFFFFFFFFFFFFFFFF
//
//     public abstract class _YafuPay : IPayChannel
//     {
//         public const string MerchantId = "MerchantId";
//         public const string SecretKey = "SecretKey";
//
//         protected bool m_qrcode = false;
//         protected string m_service = "";
//         protected string m_bankcode = "";
//
//         protected _YafuPay()
//         {
//             Init();
//         }
//
//         protected _YafuPay(string p_SettingsJson)
//         {
//             Init();
//             if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
//         }
//
//         protected override void Init()
//         {
//             Settings = new List<Setting>
//             {
//                 new Setting {Name = MerchantId, Description = "雅付的商户号", Regex = @"^\w+$", Requied = true},
//                 new Setting {Name = SecretKey, Description = "雅付的商户密钥", Regex = @"^\w+$", Requied = true}
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
//             var uri
// #if DEBUG
//                 = new Uri("http://yf.yafupay.com/yfpay/cs/pay.ac");
// #else
//                 = new Uri("http://yf.yafupay.com/yfpay/cs/pay.ac");
// #endif
//
//             var dic = new Dictionary<string, string>
//             {
//                 ["version"] = "3.0",
//                 ["consumerNo"] = this[MerchantId],
//                 ["merOrderNo"] = p_OrderId,
//                 ["transAmt"] = p_Amount.ToString("0.00"),
//                 ["backUrl"] = p_NotifyUrl,
//                 ["frontUrl"] = p_ReturnUrl,
//
//                 //支付方式:
//                 //0101:网关PC(直连银行)
//                 //0102:网关WAP(直连银行)
//                 //0201:微信扫码(跳转到我方页面)
//                 //0202:微信扫码(接口返回二维码内容)
//                 //0301:支付宝扫码(跳转到我方页面)
//                 //0301:支付宝扫码(接口返回二维码内容)
//                 //0501:QQ钱包扫码(跳转到我方页面)
//                 //0502:QQ钱包扫码(接口返回二维码内容
//                 ["payType"] = m_service,
//
//                 ["goodsName"] = p_OrderName,
//                 ["buyIp"] = p_ClientIP?.ToString(),
//             };
//             //银行网关 必传
//             if (!string.IsNullOrWhiteSpace(m_service) && (m_service == "0101" || m_service == "0102"))
//             {
//                 dic["bankCode"] = m_bankcode;
//             }
//
//             var signstr = dic.OrderBy(p => p.Key).Aggregate("",
//                               (x, y) => string.IsNullOrWhiteSpace(y.Value) ? x + "" : x + y.Key + "=" + y.Value + "&") +
//                           $"key={this[SecretKey]}";
//
//             dic["sign"] = Utils.Core.MD5(signstr);
//             if (m_qrcode)
//             {
//                 var res = _HWU.Response(uri, HttpWebUtility.HttpMethod.Post, dic);
//                 if (res.Contains("\"code\":\"000000\""))
//                 {
//                     //{"busContent":"weixin://wxpay/bizpayurl?pr=TzE7Rjt","contentType":"01","orderNo":"20170615124414956972","merOrderNo":"636331273905756443","consumerNo":"12004","transAmt":"0.50","orderStatus":"0","code":"000000","msg":"success","sign":"668EE27187A6C78A002F67CD75307444"}
//                     var json = Utils.Json.Deserialize<dynamic>(res);
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