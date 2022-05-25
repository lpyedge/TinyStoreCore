// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net;
// using System.Text;
//
// namespace Payments.Plartform.ZhiHFpay
// {
//     public abstract class _ZhiHFpay : IPayChannel
//     {
//         public const string MerchantCode = "MerchantCode";
//         public const string PublicRSAXml = "PublicRSAXml";
//         public const string PrivateRSAXml = "PrivateRSAXml";
//
//         protected string m_payType = "";
//         protected string m_bankCode = "";
//
//         protected const string GATEWAY = "https://pay.zhihfpay.com/gateway?input_charset=UTF-8";
//
//         public _ZhiHFpay()
//         {
//             Init();
//         }
//
//         public _ZhiHFpay(string p_SettingsJson)
//         {
//             Init();
//             if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
//         }
//
//         protected override void Init()
//         {
//             Settings = new List<Setting>
//             {
//                 new Setting {Name = MerchantCode, Description = "智汇付的商户号", Regex = @"^\w+$", Requied = true},
//                 new Setting
//                 {
//                     Name = PublicRSAXml, Description = "智汇付的XML公钥", Regex = @"^[\w\-\.\\:;&+=/<>]+$", Requied = true
//                 },
//                 new Setting
//                 {
//                     Name = PrivateRSAXml, Description = "智汇付商户的XML私钥", Regex = @"^[\w\-\.\\:;&+=/<>]+$", Requied = true
//                 },
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
//             if (form.ContainsKey("merchant_code")
//                 && form.ContainsKey("notify_type")
//                 && form.ContainsKey("notify_id")
//                 && form.ContainsKey("trade_status")
//                 && form.ContainsKey("sign")
//             )
//             {
//                 var signstr = form.OrderBy(p => p.Key).Where(p => p.Key != "sign_type" && p.Key != "sign").Aggregate("",
//                     (x, y) => string.IsNullOrWhiteSpace(y.Value) || string.IsNullOrWhiteSpace(y.Key)
//                         ? x + ""
//                         : x + y.Key + "=" + y.Value.Trim() + "&").TrimEnd('&');
//
//                 var provider = Utils.RSACrypto.FromXmlKey(this[PublicRSAXml]);
//                 var isverify = Utils.RSACrypto.VerifyData(provider, Utils.HASHCrypto.CryptoEnum.MD5,
//                     Convert.FromBase64String(form["sign"]), Encoding.UTF8.GetBytes(signstr));
//
//                 if (form["merchant_code"] == this[MerchantCode]
//                     && string.Equals(form["trade_status"], "SUCCESS", StringComparison.OrdinalIgnoreCase)
//                     && isverify
//                 )
//                 {
//                     result = new PayResult
//                     {
//                         OrderName = form["extra_return_param"],
//                         OrderID = form["order_no"],
//                         Amount = double.Parse(form["order_amount"]),
//                         Tax = -1,
//                         Currency = ECurrency.CNY,
//                         Business = this[MerchantCode],
//                         TxnID = form["trade_no"],
//                         PaymentName = Name,
//                         PaymentDate = DateTime.UtcNow,
//
//                         Message = "success",
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
//             if (string.IsNullOrEmpty(this[MerchantCode])) throw new ArgumentNullException("MerchantCode");
//             if (string.IsNullOrEmpty(this[PublicRSAXml])) throw new ArgumentNullException("PublicRSAXml");
//             if (string.IsNullOrEmpty(this[PrivateRSAXml])) throw new ArgumentNullException("PrivateRSAXml");
//             if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");
//
//             var dic = new Dictionary<string, string>
//             {
//                 ["merchant_code"] = this[MerchantCode],
//                 ["service_type"] = "direct_pay",
//                 ["notify_url"] = p_NotifyUrl,
//                 ["interface_version"] = "V3.0",
//                 ["input_charset"] = "UTF-8",
//                 ["sign_type"] = "RSA-S", //"RSA-S"  pem 证书  "RSA"  pfx证书
//                 ["return_url"] = p_ReturnUrl,
//                 ["pay_type"] = m_payType, //b2c,weixin,alipay_scan,tenpay_scan,yl_scan,sn_scan
//
//                 ["order_no"] = p_OrderId,
//                 ["order_time"] = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
//                 ["order_amount"] = p_Amount.ToString("0.00"),
//                 //["bank_code"] = m_bank_code,
//                 //["redo_flag"] = "1", //是否允许重复订单 当值为1时不允许商户订单号重复提交；当值为 0或空时允许商户订单号重复提交
//                 ["product_name"] = p_OrderName,
//                 ["extra_return_param"] = p_OrderName,
//             };
//
//             if (!string.IsNullOrWhiteSpace(m_bankCode) && m_payType == "b2c")
//                 dic["bank_code"] = m_bankCode;
//             if (p_ClientIP != null && p_ClientIP != IPAddress.None)
//             {
//                 dic["client_ip"] = p_ClientIP.ToString();
//                 dic["client_ip_check"] = "1";
//             }
//
//             var signstr = dic.OrderBy(p => p.Key).Where(p => p.Key != "sign_type" && p.Key != "sign").Aggregate("",
//                 (x, y) => string.IsNullOrWhiteSpace(y.Value) ? x + "" : x + y.Key + "=" + y.Value + "&").TrimEnd('&');
//
//             var provider = Utils.RSACrypto.FromXmlKey(this[PrivateRSAXml]);
//             dic["sign"] = Convert.ToBase64String(Utils.RSACrypto.SignData(provider, Utils.HASHCrypto.CryptoEnum.MD5,
//                 Encoding.UTF8
//                     .GetBytes(signstr))); // Utils.HASHCrypto.Generate( Utils.HASHCrypto.CryptoEnum.MD5).Encrypt(signstr).ToLowerInvariant();
//
//             var formhtml = new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
//                                              "' action='" + GATEWAY + "' method='post' >");
//             foreach (var item in dic)
//             {
//                 formhtml.AppendFormat("<input type='hidden' name='{0}' value='{1}' />", item.Key, item.Value);
//             }
//
//             formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
//             formhtml.Append("</form>");
//
//             var pt = new PayTicket();
//             pt.FormHtml = formhtml.ToString();
//             return pt;
//         }
//     }
// }