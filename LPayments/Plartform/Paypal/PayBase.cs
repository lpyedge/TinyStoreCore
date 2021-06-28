using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace LPayments.Plartform.Paypal
{
    [PayChannel(EChannel.Paypal, ePayType = EPayType.PC)]
    public class PayBase : _Paypal, IPay
    {
        public PayBase() : base()
        {
        }

        public PayBase(string p_SettingsJson) : base(p_SettingsJson)
        {
        }


        public PayResult Notify(IDictionary<string, string> form, IDictionary<string, string> query,
            IDictionary<string, string> head, string body, string notifyip)
        {
            PayResult result = new PayResult
            {
                Status = PayResult.EStatus.Pending,
                Message = "fail"
            };
            var encoding = Encoding.UTF8;
            try
            {
                encoding = Encoding.GetEncoding(form["charset"]);
            }
            catch
            {
            }

            //AccountCheck
            var accountcheckres = true;
            // if (bool.Parse(this[AccountCheck]))
            // {
            //     var businessaccount = form.ContainsKey("receiver_email")
            //         ? form["receiver_email"]
            //         : form["business"];
            //     accountcheckres = string.Equals(businessaccount, this[Account], StringComparison.OrdinalIgnoreCase);
            // }

            var validate = ValidateStr(form);

            //payment_status

            //完成
            //Completed: The payment has been completed, and the funds have been added successfully to your account balance.

            //创建
            //Created: A German ELV payment is made using Express Checkout.

            //拒绝
            //Denied: The payment was denied. This happens only if the payment was previously pending because of one of the reasons listed for the pending_reason variable or the Fraud_Management_Filters_x variable.

            //过期
            //Expired: This authorization has expired and cannot be captured.

            //失败
            //Failed: The payment has failed.This happens only if the payment was made from your customer's bank account.

            //等待
            //Pending: The payment is pending.See pending_reason for more information.

            //退款
            //Refunded: You refunded the payment.

            //撤款
            //Reversed: A payment was reversed due to a chargeback or other type of reversal.The funds have been removed from your account balance and returned to the buyer.The reason for the reversal is specified in the ReasonCode element.

            //撤款取消
            //Canceled_Reversal: A reversal has been canceled.For example, you won a dispute with the customer, and the funds for the transaction that was reversed have been returned to you.

            //执行
            //Processed: A payment has been accepted.

            //验证
            //Voided: This authorization has been voided.

            if (accountcheckres && validate)
            {
                result = new PayResult
                {
                    OrderName = form.ContainsKey("item_name")
                        ? form["item_name"]
                        : (form.ContainsKey("item_name1") ? form["item_name1"] : ""),
                    OrderID = form.ContainsKey("item_number")
                        ? form["item_number"]
                        : (form.ContainsKey("invoice")
                            ? form["invoice"]
                            : (form.ContainsKey("item_number1") ? form["item_number1"] : "")),
                    Amount = form.ContainsKey("mc_gross") ? double.Parse(form["mc_gross"]) : 0,
                    Tax = form.ContainsKey("mc_fee") ? double.Parse(form["mc_fee"]) : 0,
                    Currency = Utils.Core.Parse<ECurrency>(form["mc_currency"]),
                    Business =
                        form.ContainsKey("receiver_email")
                            ? form["receiver_email"]
                            : form["business"],
                    TxnID = form["txn_id"],
                    PaymentName = Name,
                    PaymentDate = DateTime.UtcNow,

                    Message = "",

                    Customer = new PayResult._Customer
                    {
                        Name = (form.ContainsKey("first_name") ? form["first_name"] : "") + " " +
                               (form.ContainsKey("last_name") ? form["last_name"] : ""),
                        Street = form.ContainsKey("address_street") ? form["address_street"] : "",
                        City = form.ContainsKey("address_city") ? form["address_city"] : "",
                        State = form.ContainsKey("address_state") ? form["address_state"] : "",
                        Zip = form.ContainsKey("address_zip") ? form["address_zip"] : "",
                        Country = form.ContainsKey("address_country")
                            ? form["address_country"]
                            : (form.ContainsKey("residence_country") ? form["residence_country"] : ""),
                        Email = form.ContainsKey("payer_email") ? form["payer_email"] : "",
                        Phone = form.ContainsKey("contact_phone") ? form["contact_phone"] : "",
                        Business = form.ContainsKey("payer_id") ? form["payer_id"] : "",
                        Status = form.ContainsKey("payer_status") && string.Equals(form["payer_status"], "verified",
                            StringComparison.OrdinalIgnoreCase),
                    },

                    Status = Utils.Core.Parse<PayResult.EStatus>(form["payment_status"])
                };
            }

            return result;
        }

        public PayTicket Pay(string p_OrderId, double p_Amount,
            ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
        {
            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");

            if (string.IsNullOrEmpty(this[Account])) throw new ArgumentNullException("Account");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            // var formhtml =
            //     new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
            //                       "' action='https://www.paypal.com/cgi-bin/webscr' method='post' >");
            // formhtml.Append("<input type='hidden' name='cmd' value='_xclick' />");
            // formhtml.AppendFormat("<input type='hidden' name='business' value='{0}' />", this[Account]);
            // formhtml.AppendFormat("<input type='hidden' name='amount' value='{0}' />", p_Amount.ToString("0.##"));
            // formhtml.AppendFormat("<input type='hidden' name='currency_code' value='{0}' />", p_Currency);
            // formhtml.AppendFormat("<input type='hidden' name='item_number' value='{0}' />", p_OrderId);
            // formhtml.AppendFormat("<input type='hidden' name='item_name' value='{0}' />", p_OrderName);
            // formhtml.AppendFormat("<input type='hidden' name='lc' value='{0}' />", "en");
            //
            // var pe = extend_params as PayExtend;
            // if (pe != null)
            // {
            //     formhtml.AppendFormat("<input type='hidden' name='image_url' value='{0}' />", pe.Logo);
            //     //For digital goods, this field is required, and you must set it to 1.
            //     //no_shipping=0,paypal会发送客户送货信息给notify地址
            //     formhtml.Append("<input type='hidden' name='no_shipping' value='" +
            //                     (pe.IsShipping ? "0" : "1") + "' />");
            // }
            //
            // formhtml.Append("<input type='hidden' name='quantity' value='1' />");
            //
            //
            // formhtml.AppendFormat("<input type='hidden' name='notify_url' value='{0}' />", p_NotifyUrl);
            // formhtml.AppendFormat("<input type='hidden' name='return' value='{0}' />", p_ReturnUrl);
            // formhtml.AppendFormat("<input type='hidden' name='cancel_return' value='{0}' />", p_CancelUrl);
            //
            // formhtml.Append("<input type='hidden' name='no_note' value='1' />");
            // formhtml.Append("<input type='hidden' name='charset' value='UTF-8' />");
            // formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
            // formhtml.Append("</form>");

            var datas = new Dictionary<string, string>()
            {
                ["cmd"] = "_xclick",
                ["business"] = this[Account],
                ["amount"] = p_Amount.ToString("0.##"),
                ["currency_code"] = p_Currency.ToString(),
                ["item_number"] = p_OrderId,
                ["item_name"] = p_OrderName,
                ["lc"] = "en",
                ["quantity"] = "1",
                ["notify_url"] = p_NotifyUrl,
                ["return"] = p_ReturnUrl,
                ["cancel_return"] = p_CancelUrl,
                ["no_note"] = "1",
                ["charset"] = "UTF-8",
                ["no_shipping"] = "0",
            };
            var pe = extend_params as PayExtend;
            if (pe != null)
            {
                if (!string.IsNullOrWhiteSpace(pe.Logo))
                    datas["image_url"] = pe.Logo;
                datas["no_shipping"] = (pe.IsShipping ? "0" : "1");
            }

            return new PayTicket()
            {
                PayType = PayChannnel.ePayType,
                Action = EAction.UrlPost,
                Uri = "https://www.paypal.com/cgi-bin/webscr",
                Datas = datas
            };
        }

        private static bool ValidateStr(IDictionary<string, string> form)
        {
            bool res = false;

            //tls12 加密
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var strToSend = form.Aggregate("", (s, p) => { return s += p.Key + "=" + p.Value + "&"; }) +
                            "cmd=_notify-validate";
            var param = Encoding.UTF8.GetBytes(strToSend);
            var myRequest = (HttpWebRequest) WebRequest.Create("https://ipnpb.paypal.com/cgi-bin/webscr");
            myRequest.Method = "POST";
            myRequest.ContentType = "application/x-www-form-urlencoded";
            using (var reqStream = myRequest.GetRequestStream())
            {
                reqStream.Write(param, 0, param.Length);
            }

            using (var myResponse = (HttpWebResponse) myRequest.GetResponse())
            {
                using (var resStream = myResponse.GetResponseStream())
                {
                    using (var sr = new StreamReader(resStream, Encoding.UTF8))
                    {
                        res = string.Equals(sr.ReadToEnd(), "VERIFIED", StringComparison.OrdinalIgnoreCase);
                        sr.Close();
                    }
                }
            }

            myRequest.Abort();
            return res;
        }

        public class PayExtend
        {
            /// <summary>
            ///     Logo
            /// </summary>
            public string Logo { get; set; }

            /// <summary>
            /// 需要发货地址
            /// </summary>
            public bool IsShipping { get; set; } = true;
        }
    }
}
//付款参数文档
//https://developer.paypal.com/docs/classic/paypal-payments-standard/integration-guide/formbasics/
//https://developer.paypal.com/docs/classic/paypal-payments-standard/integration-guide/Appx_websitestandard_htmlvariables/
//IPN参数文档
//https://developer.paypal.com/docs/notifications/
//https://developer.paypal.com/docs/classic/ipn/integration-guide/IPNIntro/
//https://developer.paypal.com/docs/classic/ipn/integration-guide/IPNandPDTVariables/

//退款参数文档
//https://developer.paypal.com/docs/integration/direct/payments/refund-payment/

//Webhook Event Validation
//https://github.com/paypal/PayPal-NET-SDK/wiki/Webhook-Event-Validation

//客户设置IPN文档
//https://developer.paypal.com/docs/classic/ipn/integration-guide/IPNSetup/

//支持的货币类型
//https://developer.paypal.com/docs/classic/api/currency_codes/#id09A6G0U0GYK

//PayPal REST
//http://www.cnblogs.com/feiDD/articles/3179515.html

//技术支持网站
//https://cn.paypal-techsupport.com