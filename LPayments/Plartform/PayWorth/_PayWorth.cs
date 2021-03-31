//
//
// using System;
// using System.Collections.Generic;
// using System.Linq;
// using System.Net;
// using System.Text;
// using System.Web;
//
// namespace LPayments.Plartform.PayWorth
// {
//     public abstract class _PayWorth : IPayChannel
//     {
//         public const string MerchantId = "MerchantId";
//         public const string MerchantEmail = "MerchantEmail";
//         public const string SecretKey = "SecretKey";
//
//         protected string m_paymethod = "";
//         protected string m_defaultbank = "";
//
//         public _PayWorth()
//         {
//             Init();
//         }
//
//         public _PayWorth(string p_SettingsJson)
//         {
//             Init();
//             if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
//         }
//
//         protected override void Init()
//         {
//             Settings = new List<Setting>
//             {
//                 new Setting {Name = MerchantId, Description = "华势在线的商户号", Regex = @"^\w+$", Requied = true},
//                 new Setting {Name = MerchantEmail, Description = "华势在线的商户邮箱", Regex = @"^[\w@.]+$", Requied = true},
//                 new Setting {Name = SecretKey, Description = "华势在线的商户密钥", Regex = @"^\w+$", Requied = true}
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
//             if (form.ContainsKey("sign") && form.ContainsKey("is_success") && form.ContainsKey("seller_email") && form.ContainsKey("seller_id"))
//             {
//                 var signstr = form.OrderBy(p => p.Key).Where(p => p.Key != "sign_type" && p.Key != "sign").Aggregate("",
//                                   (x, y) => string.IsNullOrWhiteSpace(y.Value) || string.IsNullOrWhiteSpace(y.Key) ? x + "" : x + y.Key + "=" + y.Value + "&").TrimEnd('&') + this[SecretKey];
//                 if (form["is_success"] == "T"
//                     && string.Equals(form["sign"], Utils.Core.MD5(signstr), StringComparison.OrdinalIgnoreCase)
//                     && form["seller_email"] == this[MerchantEmail]
//                     && form["seller_id"] == this[MerchantId]
//                     && form["trade_status"] == "TRADE_FINISHED"
//                     )
//                 {
//                     result = new PayResult
//                     {
//                         OrderName = form["title"] ?? "",
//                         OrderID = form["order_no"],
//                         Amount =  double.Parse(form["total_fee"]),
//                         Tax = -1,
//                         Currency = ECurrency.CNY,
//                         Business = this[MerchantId],
//                         TxnID = form["trade_no"],
//                         PaymentName = Name,
//                         PaymentDate = DateTime.UtcNow,
//
//                         Message = "success",
//                     };
//                 }
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
//             if (string.IsNullOrEmpty(this[MerchantId])) throw new ArgumentNullException("MerchantId");
//             if (string.IsNullOrEmpty(this[SecretKey])) throw new ArgumentNullException("SecretKey");
//             if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");
//
//             var uri
// #if DEBUG
//                 = new Uri("https://ebank.payworth.net/portal");
// #else
//                 = new Uri("https://ebank.payworth.net/portal");
// #endif
//             var dic = new Dictionary<string, string>
//             {
//                 ["service"] = "online_pay",
//                 ["merchant_ID"] = this[MerchantId],
//                 ["notify_url"] = p_NotifyUrl,
//                 ["return_url"] = p_ReturnUrl,
//                 ["sign_type"] = "MD5",
//                 ["charset"] = "utf-8",
//
//                 ["title"] = p_OrderName,
//                 ["body"] = p_OrderName,
//                 ["order_no"] = p_OrderId,
//                 ["total_fee"] = p_Amount.ToString("0.00"),
//                 ["payment_type"] = "1",
//                 ["seller_email"] = this[MerchantEmail],
//             };
//
//             //paymethod 支付方式 bankPay 收银台  directPay 直连网银
//             dic["paymethod"] = m_paymethod;
//             //defaultbank 仅当paymethod = directPay 时设置
//             if (m_paymethod == "directPay" && !string.IsNullOrWhiteSpace(m_defaultbank))
//                 dic["defaultbank"] = m_defaultbank;
//
//             var signstr = dic.OrderBy(p => p.Key).Where(p => p.Key != "sign_type" && p.Key != "sign").Aggregate("",
//                           (x, y) => string.IsNullOrWhiteSpace(y.Value) ? x + "" : x + y.Key + "=" + y.Value + "&").TrimEnd('&') + this[SecretKey];
//
//             dic["sign"] = Utils.Core.MD5(signstr).ToLowerInvariant();
//
//             var formhtml =
//                 new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
//                                   "' action='" + uri.ToString() + "?charset=utf-8' method='post' >");
//             foreach (var item in dic)
//             {
//                 formhtml.AppendFormat("<input type='hidden' name='{0}' value='{1}' />", item.Key, item.Value);
//             }
//             formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
//             formhtml.Append("</form>");
//
//             var pt = new PayTicket();
//             pt.FormHtml = formhtml.ToString();
//             return pt;
//         }
//     }
// }