// using System;
// using System.Collections.Generic;
// using System.Net;
// using System.Text;
// using System.Web;
//
// namespace LPayments.Plartform.HasPay
// {
//     [PayChannel("有付", EChannel.HasPay)]
//     public class HasPay : IPayChannel
//     {
//         public const string Account = "Account";
//         public const string SecretKey = "SecretKey";
//
//         public HasPay()
//         {
//             Init();
//         }
//
//         public HasPay(string p_SettingsJson)
//         {
//             Init();
//             if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
//         }
//
//         protected override void Init()
//         {
//             Settings = new List<Setting>
//             {
//                 new Setting {Name = Account, Description = "HasPay的商户号", Regex = @"^\d+$", Requied = true},
//                 new Setting {Name = SecretKey, Description = "HasPay的MD5密钥Md5Key", Regex = @"^\w+$", Requied = true}
//             };
//
//             Currencies = new List<ECurrency>
//             {
//                 ECurrency.USD,
//                 ECurrency.GBP,
//                 ECurrency.EUR,
//                 ECurrency.JPY,
//                 ECurrency.AUD,
//                 ECurrency.NOK,
//                 ECurrency.CAD,
//                 ECurrency.CNY,
//                 ECurrency.SEK,
//                 ECurrency.DKK,
//                 ECurrency.HKD,
//                 ECurrency.RUB,
//                 ECurrency.SGD,
//                 ECurrency.THB,
//                 ECurrency.TWD,
//                 ECurrency.MYR,
//                 ECurrency.VND,
//                 ECurrency.PHP,
//                 ECurrency.MNT,
//                 ECurrency.NZD,
//                 ECurrency.AED,
//                 ECurrency.MOP,
//                 ECurrency.BRL,
//                 ECurrency.KZT,
//                 ECurrency.SAR,
//                 ECurrency.TRY,
//                 ECurrency.KRW
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
//             var signature =
//                 Utils.Core.MD5(form["merNo"] + form["orderNo"] + form["currency"] + form["amount"] +
//                                form["status"] + this[SecretKey]);
//
//             var IsVlidate = !string.IsNullOrWhiteSpace(signature) && !string.IsNullOrWhiteSpace(form["sign"]) &&
//                             string.Equals(signature, form["sign"], StringComparison.OrdinalIgnoreCase);
//
//             if (IsVlidate && form["status"] == "88")
//             {
//                 var shippinginfo = !string.IsNullOrWhiteSpace(form["shippingAddressList"]) &&
//                                    form["shippingAddressList"].Contains("|")
//                     ? form["shippingAddressList"].Split('|')
//                     : new string[5];
//
//                 result = new PayResult
//                 {
//                     OrderName = form["desc"],
//                     OrderID = form["orderNo"],
//                     Amount = double.Parse(form["amount"]),
//                     Tax = -1,
//                     Currency = Utils.Core.Parse<ECurrency>(form["currency"]),
//                     Business = this[Account],
//                     TxnID = form["serialNo"],
//                     PaymentName = Name,
//                     PaymentDate = DateTime.Parse(form["tradeDate"]),
//
//                     Message = "",
//
//                     Customer = new PayResult._Customer
//                     {
//                         Street = shippinginfo[3],
//                         City = shippinginfo[2],
//                         State = shippinginfo[1],
//                         Zip = shippinginfo[4],
//                         Country = shippinginfo[0],
//                         Name = form["buyerName"],
//                         Email = form["email"],
//                         Phone = form["shippingPhone"],
//                         Business = "",
//                         Status = true
//                     }
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
//             if (string.IsNullOrEmpty(this[Account])) throw new ArgumentNullException("Account");
//             if (string.IsNullOrEmpty(this[SecretKey])) throw new ArgumentNullException("SecretKey");
//             if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");
//
//             var formhtml =
//                 new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
//                                   "' action='https://pay.haspay.com/paymentpre' method='post' >");
//             formhtml.AppendFormat("<input type='hidden' name='MerNo' value='{0}' />", this[Account]);
//             formhtml.AppendFormat("<input type='hidden' name='Language' value='{0}' />", 2);
//             formhtml.AppendFormat("<input type='hidden' name='Amount' value='{0}' />", p_Amount.ToString("0.##"));
//             formhtml.AppendFormat("<input type='hidden' name='Currency' value='{0}' />",
//                 Currencies.IndexOf(p_Currency) + 1);
//             formhtml.AppendFormat("<input type='hidden' name='BillNo' value='{0}' />", p_OrderId);
//             formhtml.AppendFormat("<input type='hidden' name='OrderDesc' value='{0}' />", p_OrderName);
//             formhtml.AppendFormat("<input type='hidden' name='ReturnURL' value='{0}' />", p_ReturnUrl);
//             formhtml.AppendFormat("<input type='hidden' name='NoticeURL' value='{0}' />", p_NotifyUrl);
//
//             formhtml.AppendFormat("<input type='hidden' name='MD5info' value='{0}' />",
//                 Utils.Core.MD5(this[Account] + p_OrderId + (Currencies.IndexOf(p_Currency) + 1) +
//                                p_Amount.ToString("0.##") +
//                                2 + p_ReturnUrl + this[SecretKey]).ToUpper());
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