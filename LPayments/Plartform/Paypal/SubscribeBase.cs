using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace LPayments.Plartform.Paypal
{
    [PayChannel(EChannel.PaypalSubscribe)]
    public class SubscribeBase : _Paypal, IPay
    {
        public SubscribeBase() : base()
        {
        }

        public SubscribeBase(string p_SettingsJson) : base(p_SettingsJson)
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

            var dictionary = Utils.Core.UnLinkStr(body);
            //todo Subscribe的notify数据字段要修正
            //AccountCheck
            var accountcheckres = true;
            // if (bool.Parse(this[AccountCheck]))
            // {
            //     var businessaccount = dictionary.ContainsKey("receiver_email")
            //         ? dictionary["receiver_email"]
            //         : dictionary["business"];
            //     accountcheckres = string.Equals(businessaccount, this[Account], StringComparison.OrdinalIgnoreCase);
            // }

            var validate = ValidateStr(body);

            if (accountcheckres && validate &&
                string.Equals(dictionary["payment_status"], "Completed", StringComparison.OrdinalIgnoreCase))
            {
                result = new PayResult
                {
                    OrderName = dictionary.ContainsKey("item_name")
                        ? dictionary["item_name"]
                        : (dictionary.ContainsKey("item_name1") ? dictionary["item_name1"] : ""),
                    OrderID = dictionary.ContainsKey("item_number")
                        ? dictionary["item_number"]
                        : (dictionary.ContainsKey("invoice")
                            ? dictionary["invoice"]
                            : (dictionary.ContainsKey("item_number1") ? dictionary["item_number1"] : "")),
                    Amount = dictionary.ContainsKey("mc_amount3") ? double.Parse(dictionary["mc_amount3"]) : 0,
                    Tax = dictionary.ContainsKey("mc_fee") ? double.Parse(dictionary["mc_fee"]) : 0,
                    Currency = Utils.Core.Parse<ECurrency>(dictionary["mc_currency"]),
                    Business =
                        dictionary.ContainsKey("receiver_email")
                            ? dictionary["receiver_email"]
                            : dictionary["business"],
                    TxnID = dictionary["subscr_id"],
                    PaymentName = Name,
                    PaymentDate = DateTime.UtcNow,
                    DueDate = DateTime.UtcNow,

                    Message = "",

                    Customer = new PayResult._Customer
                    {
                        Name = (dictionary.ContainsKey("first_name") ? dictionary["first_name"] : "") + " " +
                               (dictionary.ContainsKey("last_name") ? dictionary["last_name"] : ""),
                        Street = (dictionary.ContainsKey("address_street") ? dictionary["address_street"] : ""),
                        City = (dictionary.ContainsKey("address_city") ? dictionary["address_city"] : ""),
                        State = (dictionary.ContainsKey("address_state") ? dictionary["address_state"] : ""),
                        Zip = dictionary.ContainsKey("address_zip") ? dictionary["address_zip"] : "",
                        Country = dictionary.ContainsKey("address_country")
                            ? dictionary["address_country"]
                            : (dictionary.ContainsKey("residence_country") ? dictionary["residence_country"] : ""),
                        Email = dictionary.ContainsKey("payer_email") ? dictionary["payer_email"] : "",
                        Phone = dictionary.ContainsKey("contact_phone") ? dictionary["contact_phone"] : "",
                        Business = dictionary.ContainsKey("payer_id") ? dictionary["payer_id"] : "",
                        Status = dictionary.ContainsKey("payer_status") && string.Equals(dictionary["payer_status"],
                            "verified", StringComparison.OrdinalIgnoreCase),
                    },

                    Status = Utils.Core.Parse<PayResult.EStatus>(dictionary["payment_status"])
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

            if (extend_params == null) throw new ArgumentNullException("extend_params");
            if (!(extend_params is PayExtend)) throw new ArgumentException("extend_params must be PayExtend");

            var pe = extend_params as PayExtend;

            // var formhtml =
            //     new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
            //                       "' action='https://www.paypal.com/cgi-bin/webscr' method='post' >");
            // formhtml.Append("<input type='hidden' name='cmd' value='_xclick-subscriptions' />");
            //
            // formhtml.AppendFormat("<input type='hidden' name='business' value='{0}' />", this[Account]);
            // formhtml.AppendFormat("<input type='hidden' name='item_name' value='{0}' />", p_OrderName);
            // formhtml.AppendFormat("<input type='hidden' name='currency_code' value='{0}' />", p_Currency);
            // formhtml.AppendFormat("<input type='hidden' name='lc' value='{0}' />", "en");
            //
            // formhtml.AppendFormat("<input type='hidden' name='a3' value='{0}' />", p_Amount.ToString("0.##"));
            // formhtml.AppendFormat("<input type='hidden' name='p3' value='{0}' />", pe.Value);
            // formhtml.AppendFormat("<input type='hidden' name='t3' value='{0}' />", pe.Unit);
            //
            // formhtml.Append("<input type='hidden' name='no_shipping' value='1' />");
            //
            // formhtml.AppendFormat("<input type='hidden' name='image_url' value='{0}' />", pe.Logo);
            // // Display the total payment amount to buyers during checkou
            // // Y — display the total
            // // N — do not display the total
            // formhtml.AppendFormat("<input type='hidden' name='disp_tot' value='{0}' />", "Y");
            //
            // formhtml.AppendFormat("<input type='hidden' name='return' value='{0}' />", p_ReturnUrl);
            // formhtml.AppendFormat("<input type='hidden' name='notify_url' value='{0}' />", p_NotifyUrl);
            // formhtml.AppendFormat("<input type='hidden' name='cancel_return' value='{0}' />", p_CancelUrl);
            //
            // //formhtml.AppendFormat("<input type='hidden' name='bn' value='{0}' />", "DesignerFotos_Subscribe_WPS_US");
            // //formhtml.Append("<input type='hidden' name='no_note' value='1' />");
            // //formhtml.Append("<input type='hidden' name='charset' value='UTF-8' />");
            // formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
            // formhtml.Append("</form>");

            var datas = new Dictionary<string, string>()
            {
                ["cmd"] = "_xclick-subscriptions",
                ["business"] = this[Account],
                ["item_name"] = p_OrderName,
                ["currency_code"] = p_Currency.ToString(),
                ["lc"] = "en",
                ["a3"] = p_Amount.ToString("0.##"),
                ["p3"] = pe.Value.ToString(),
                ["t3"] = pe.Unit.ToString(),
                ["disp_tot"] = "Y",
                ["return"] = p_ReturnUrl,
                ["notify_url"] = p_NotifyUrl,
                ["cancel_return"] = p_CancelUrl,
                ["charset"] = "UTF-8",
                ["no_shipping"] = "0",
            };

            if (!string.IsNullOrWhiteSpace(pe.Logo))
                datas["image_url"] = pe.Logo;
            
            return new PayTicket()
            {
                Name = this.Name,
                DataFormat = EPayDataFormat.Form,
                DataContent = "https://www.paypal.com/cgi-bin/webscr??" + Utils.Core.LinkStr(datas,encode:true),
            };
        }

        private static bool ValidateStr(string body)
        {
            bool res = false;

            //tls12 加密
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var param = Encoding.ASCII.GetBytes(body + "&cmd=_notify-validate");
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
                    using (var sr = new StreamReader(resStream, Encoding.ASCII))
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
            ///     订阅周期单位
            /// </summary>
            public enum Units
            {
                Y,
                M,
                W,
                D
            }

            /// <summary>
            ///     订阅周期单位
            /// </summary>
            public Units Unit { get; set; }

            /// <summary>
            ///     订阅周期值
            ///     D — for days; allowable range is 1 to 90
            ///     W — for weeks; allowable range is 1 to 52
            ///     M — for months; allowable range is 1 to 24
            ///     Y — for years; allowable range is 1 to 5
            /// </summary>
            public int Value { get; set; }

            /// <summary>
            ///     订阅说明
            /// </summary>
            public string Description { get; set; }

            /// <summary>
            ///     Logo
            /// </summary>
            public string Logo { get; set; }
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

//Webhook Event Validation
//https://github.com/paypal/PayPal-NET-SDK/wiki/Webhook-Event-Validation

//客户设置IPN文档
//https://developer.paypal.com/docs/classic/ipn/integration-guide/IPNSetup/

//支持的货币类型
//https://developer.paypal.com/docs/classic/api/currency_codes/#id09A6G0U0GYK

//技术支持网站
//https://cn.paypal-techsupport.com