using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace LPayments.Plartform.Xfers
{
    
    [PayChannel(EChannel.Xfers)]
    public class Xfers : IPayChannel, IPay
    {
        public const string ApiKey = "ApiKey";
        public const string ApiSecret = "ApiSecret";

        public Xfers() : base()
        {
            Platform = EPlatform.Xfers;
        }

        public Xfers(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = ApiKey, Description = "Xfers.io提供的API Key", Regex = @"^\w+$", Requied = true},
                new Setting {Name = ApiSecret, Description = "Xfers.io提供的API Secret", Regex = @"^\w+$", Requied = true}
            };

            Currencies = new List<ECurrency>
            {
                ECurrency.SGD
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

            var IsVlidate = form["status"] == "paid" && form["api_key"] == this[ApiKey];
            if (IsVlidate)
                IsVlidate = ValidateStr(form);

            if (IsVlidate)
            {
                result = new PayResult
                {
                    OrderName = "",
                    OrderID = form["order_id"],
                    Amount = double.Parse(form["total_amount"]),
                    Tax = form.Keys.Contains("fees") ? double.Parse(form["fees"]) : 0,
                    Currency = Utils.Core.Parse<ECurrency>(form["currency"]),
                    Business = form["api_key"],
                    TxnID = form["txn_id"],
                    PaymentName = Name,
                    PaymentDate = DateTime.UtcNow,

                    Message = "",
                };
            }

            return result;
        }

        public virtual PayTicket Pay(string p_OrderId, double p_Amount,
            ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
        {
            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");

            if (string.IsNullOrEmpty(this[ApiKey])) throw new ArgumentNullException("ApiKey");
            if (string.IsNullOrEmpty(this[ApiSecret])) throw new ArgumentNullException("ApiSecret");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            // var formhtml =
            //     new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
            //                       "' action='https://www.xfers.io/api/v2/payments' method='post' >");
            // formhtml.AppendFormat("<input type='hidden' name='api_key' value='{0}' />", this[ApiKey]);
            // formhtml.AppendFormat("<input type='hidden' name='order_id' value='{0}' />", p_OrderId);
            // formhtml.AppendFormat("<input type='hidden' name='total_amount' value='{0}' />", p_Amount.ToString("0.##"));
            // formhtml.AppendFormat("<input type='hidden' name='currency' value='{0}' />", p_Currency);
            // formhtml.AppendFormat("<input type='hidden' name='item_name_1' value='{0}' />", p_OrderName);
            // formhtml.AppendFormat("<input type='hidden' name='item_price_1' value='{0}' />", p_Amount.ToString("0.##"));
            // formhtml.Append("<input type='hidden' name='item_quantity_1' value='1' />");
            // formhtml.Append("<input type='hidden' name='refundable' value='false' />");
            // formhtml.AppendFormat("<input type='hidden' name='cancel_url' value='{0}' />", p_CancelUrl);
            // formhtml.AppendFormat("<input type='hidden' name='return_url' value='{0}' />", p_ReturnUrl);
            // formhtml.AppendFormat("<input type='hidden' name='notify_url' value='{0}' />", p_NotifyUrl);
            // formhtml.AppendFormat("<input type='hidden' name='signature' value='{0}' />",
            //     Utils.Core.SHA1(this[ApiKey] + this[ApiSecret] + p_OrderId + p_Amount.ToString("0.##") + p_Currency));
            // formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
            // formhtml.Append("</form>");
            
            var datas = new Dictionary<string, string>()
            {
                ["api_key"] = this[ApiKey],
                ["order_id"] = p_OrderId,
                ["total_amount"] = p_Amount.ToString("0.##"),
                ["currency"] = p_Currency.ToString(),
                ["item_name_1"] = p_OrderName,
                ["item_price_1"] = p_Amount.ToString("0.##"),
                ["item_quantity_1"] = "1",
                ["refundable"] = "false",
                ["cancel_url"] = p_CancelUrl,
                ["return_url"] = p_ReturnUrl,
                ["notify_url"] = p_NotifyUrl,
                ["signature"] = Utils.Core.SHA1(this[ApiKey] + this[ApiSecret] + p_OrderId + p_Amount.ToString("0.##") + p_Currency),
            };
            
            var pt = new PayTicket()
            {
                Action = EAction.UrlPost,
                Uri = "https://www.xfers.io/api/v2/payments",
                Datas = datas,
            };
            
            return pt;
        }

        //todo  所有在paycheck里面的对HttpRequest的操作都要避免
        //private static string ValidateStr(HttpRequest p_Request)
        //{
        //    string validateStr;

        //    var param = p_Request.BinaryRead(p_Request.ContentLength);
        //    var strToSend = Encoding.ASCII.GetString(param);
        //    param = Encoding.ASCII.GetBytes(strToSend);
        //    var myRequest = (HttpWebRequest)WebRequest.Create("https://www.xfers.io/api/v2/payments/validate/");
        //    myRequest.Method = "POST";
        //    myRequest.ContentType = "application/x-www-form-urlencoded";
        //    using (var reqStream = myRequest.GetRequestStream())
        //    {
        //        reqStream.Write(param, 0, param.Length);
        //    }
        //    using (var myResponse = (HttpWebResponse)myRequest.GetResponse())
        //    {
        //        using (var resStream = myResponse.GetResponseStream())
        //        {
        //            using (var sr = new StreamReader(resStream, Encoding.ASCII))
        //            {
        //                validateStr = sr.ReadToEnd();
        //                sr.Close();
        //            }
        //        }
        //    }
        //    myRequest.Abort();
        //    return validateStr;
        //}

        private static bool ValidateStr(IDictionary<string, string> form)
        {
            bool res = false;
            var strToSend = form.Aggregate("", (s, p) => { return s += p.Key + "=" + p.Value + "&"; }).TrimEnd('&');
            var param = Encoding.ASCII.GetBytes(strToSend);
            var myRequest = (HttpWebRequest) WebRequest.Create("https://www.xfers.io/api/v2/payments/validate/");
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
    }
}

//https://docs.xfers.io/
//https://www.xfers.com/sg/developers/xfers-core-api-works/