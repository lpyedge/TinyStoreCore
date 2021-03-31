// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net;
// using System.Web;
// using System.Xml;
// using LPayments.Utils;
//
// namespace LPayments.Plartform.Jiuxiao
// {
//     //测试商户号	7551000001	商户调用接口时可用此测试商户号对应参数 mch_id
//     //测试密钥	9d101c97133837e13dde2d32a5054abb 为保证通讯不被篡改，九霄支付与商户之间约定的32 位或 24 位字符串，算签名 sign 时使用
//     //金额  1000	金额，默认为 RMB，以分为单位。1000 表示RMB10.00
//
//     public abstract class _Jiuxiao : IPayChannel
//     {
//         public const string MerchantId = "MerchantId";
//         public const string SecretKey = "SecretKey";
//
//         protected bool m_qrcode = true;
//         protected string m_service = "";
//
//         protected _Jiuxiao()
//         {
//             Init();
//         }
//
//         protected _Jiuxiao(string p_SettingsJson)
//         {
//             Init();
//             if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
//         }
//
//         protected override void Init()
//         {
//             Settings = new List<Setting>
//             {
//                 new Setting {Name = MerchantId, Description = "九霄支付的商户号", Regex = @"^\w+$", Requied = true},
//                 new Setting {Name = SecretKey, Description = "九霄支付的商户密钥", Regex = @"^\w+$", Requied = true}
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
//             var xd = new XmlDocument();
//             xd.LoadXml(body);
//             var tempdic = new Dictionary<string, string>();
//             foreach (XmlNode node in xd.ChildNodes[0].ChildNodes)
//             {
//                 tempdic[node.Name] = node.InnerText;
//             }
//
//             if (body.Contains("status") && tempdic["status"] == "0"
//                                         && tempdic["result_code"] == "0" && tempdic["mch_id"] == this[MerchantId])
//             {
//                 var signstr = tempdic.Where(p => p.Key != "sign").OrderBy(p => p.Key)
//                     .Aggregate("", (x, y) => x + y.Key + "=" + y.Value + "&") + $"key={this[SecretKey]}";
//
//                 if (string.Equals(tempdic["sign"], Utils.Core.MD5(signstr), StringComparison.OrdinalIgnoreCase)
//                     && tempdic["pay_result"] == "0")
//                 {
//                     result = new PayResult
//                     {
//                         OrderName = "",
//                         OrderID = tempdic["out_trade_no"],
//                         Amount = double.Parse(tempdic["total_fee"]) / 100,
//                         Tax = -1,
//                         Currency = ECurrency.CNY,
//                         Business = this[MerchantId],
//                         TxnID = tempdic["transaction_id"],
//                         PaymentName = Name,
//                         PaymentDate = DateTime.UtcNow,
//
//                         Message = "success",
//
//                         Customer = new PayResult._Customer
//                         {
//                             Business = tempdic["openid"],
//                         }
//                     };
//                 }
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
//             var uri = new Uri(
// #if DEBUG
//                 "http://124.74.138.254:8600/api/pay"
// #else
//                 "http://paygateway.566560.com/api/pay"
// #endif
//             );
//             //m_service = "weixin.scan";
//             var dic = new Dictionary<string, string>
//             {
//                 ["service"] = m_service, //"alipay.scan",
//                 ["mch_id"] = this[MerchantId],
//                 ["out_trade_no"] = p_OrderId,
//                 ["body"] = p_OrderName,
//                 ["total_fee"] = (p_Amount * 100).ToString("0"),
//                 ["mch_create_ip"] = p_ClientIP?.ToString(),
//
//                 ["notify_url"] = p_NotifyUrl,
//                 ["nonce_str"] = string.Concat(DateTime.UtcNow.Ticks.ToString().Reverse().Take(6).ToArray()),
//             };
//             var signstr = dic.OrderBy(p => p.Key).Aggregate("",
//                               (x, y) => string.IsNullOrWhiteSpace(y.Value) ? x + "" : x + y.Key + "=" + y.Value + "&") +
//                           $"key={this[SecretKey]}";
//             dic["sign"] = Utils.Core.MD5(signstr).ToUpperInvariant();
//
//             if (m_qrcode)
//             {
//                 XmlDocument xd = new XmlDocument();
//                 XmlElement root = xd.CreateElement("xml");
//                 foreach (var item in dic)
//                 {
//                     var node = xd.CreateElement(item.Key);
//                     node.AppendChild(xd.CreateTextNode(item.Value));
//                     root.AppendChild(node);
//                 }
//
//                 xd.AppendChild(root);
//
//                 var res = _HWU.ResponseBody(uri, HttpWebUtility.HttpMethod.Post, xd.InnerXml);
//
//                 xd = new XmlDocument();
//                 xd.LoadXml(res);
//                 var tempdic = new Dictionary<string, string>();
//                 foreach (XmlNode node in xd.ChildNodes[0].ChildNodes)
//                 {
//                     tempdic[node.Name] = node.InnerText;
//                 }
//
//                 if (res.Contains("status") && tempdic["status"] == "0"
//                                            && tempdic["result_code"] == "0" &&
//                                            tempdic["nonce_str"] == dic["nonce_str"] &&
//                                            tempdic["mch_id"] == this[MerchantId])
//                 {
//                     //<xml><result_code><![CDATA[0]]></result_code><mch_id><![CDATA[7551000001]]></mch_id><status><![CDATA[0]]></status><code_url><![CDATA[https://qr.alipay.com/bax00607jw0ygafvzfdy0072]]></code_url><sign_type><![CDATA[MD5]]></sign_type><code_img_url><![CDATA[https://pay.swiftpass.cn/pay/qrcode?uuid=https%3A%2F%2Fqr.alipay.com%2Fbax00607jw0ygafvzfdy0072]]></code_img_url><nonce_str><![CDATA[981046]]></nonce_str><sign><![CDATA[34d17142a4ee9ee2f42e36841731c29f]]></sign><version><![CDATA[2.0]]></version><charset><![CDATA[UTF-8]]></charset><appid><![CDATA[2016081701763242]]></appid></xml>
//
//                     signstr = tempdic.Where(p => p.Key != "sign").OrderBy(p => p.Key)
//                         .Aggregate("", (x, y) => x + y.Key + "=" + y.Value + "&") + $"key={this[SecretKey]}";
//
//                     if (string.Equals(tempdic["sign"], Utils.Core.MD5(signstr), StringComparison.OrdinalIgnoreCase))
//                     {
//                         pt.Url = tempdic["code_url"];
//                         //string imgbase64 = Core.QR(pt.Url, this.GetType());
//                         //pt.FormHtml = Core.FormQR(imgbase64, p_OrderId, p_Amount, p_OrderName);
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
//             //else
//             //{
//             //    var formhtml =
//             //        new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
//             //                          "' action='"+ uri + "' method='post' " + (Helper.NewWindow ? "target='_blank'" : "") + ">");
//             //    foreach (var item in dic)
//             //    {
//             //        formhtml.AppendFormat("<input type='hidden' name='{0}' value='{1}' />", item.Key,item.Value);
//             //    }
//
//             //    formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
//             //    formhtml.Append("</form>");
//             //    if (Helper.AutoSubmit)
//             //        formhtml.Append("<script>document.forms['" + Core.PaymentFormName + "'].submit();</script>");
//             //    pt.FormHtml = formhtml.ToString();
//             //}
//             return pt;
//         }
//     }
// }