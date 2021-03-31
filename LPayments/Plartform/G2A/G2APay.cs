using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using LPayments.Utils;

namespace LPayments.Plartform.G2APay
{
    [PayPlatformAttribute("全球数码", "G2A.COM", SiteUrl = "https://pay.g2a.com")]
    [PayChannel(EChannel.G2APay)]
    public class G2APay : IPayChannel, IPay
    {
        public const string APIHash = "APIHash";
        public const string APISecret = "APISecret";

        public G2APay():base()
        {
            
        }

        public G2APay(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = APIHash, Description = "G2APay的API hash", Regex = @"^[\w\-]+$", Requied = true},
                new Setting {Name = APISecret, Description = "G2APay的Your Secret", Regex = @"^[\w\W]+$", Requied = true}
            };

            Currencies = new List<ECurrency>
            {
                ECurrency.USD,
                ECurrency.AUD,
                ECurrency.BRL,
                ECurrency.CAD,
                ECurrency.CZK,
                ECurrency.DKK,
                ECurrency.EUR,
                ECurrency.HKD,
                ECurrency.HUF,
                ECurrency.ILS,
                ECurrency.JPY,
                ECurrency.MYR,
                ECurrency.MXN,
                ECurrency.NOK,
                ECurrency.NZD,
                ECurrency.PHP,
                ECurrency.PLN,
                ECurrency.GBP,
                ECurrency.RUB,
                ECurrency.SGD,
                ECurrency.SEK,
                ECurrency.CHF,
                ECurrency.TWD,
                ECurrency.THB,
                ECurrency.TRY
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

            if (form.ContainsKey("type")
                && form.ContainsKey("transactionId")
                && form.ContainsKey("userOrderId")
                && form.ContainsKey("amount")
                && form.ContainsKey("currency")
                && form.ContainsKey("status")
                && form.ContainsKey("orderCreatedAt")
                && form.ContainsKey("orderCompleteAt")
                && form.ContainsKey("refundedAmount")
                && form.ContainsKey("provisionAmount")
                && form.ContainsKey("hash"))
                if (string.Equals(form["type"], "payment", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(form["status"], "complete", StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(form["hash"],
                        Utils.HASHCrypto.Encrypt(Utils.HASHCrypto.Generate(Utils.HASHCrypto.CryptoEnum.SHA256),
                            form["transactionId"] + form["userOrderId"] + form["amount"] +
                            this[APISecret]), StringComparison.OrdinalIgnoreCase))
                {
                    result = new PayResult
                    {
                        OrderName = "",
                        OrderID = form["userOrderId"],
                        Amount = Double.Parse(form["amount"]),
                        Tax =
                            form["provisionAmount"] != null
                                ? double.Parse(form["provisionAmount"])
                                : -1,
                        Currency = Utils.Core.Parse<ECurrency>(form["currency"]),
                        Business = this[APIHash],
                        TxnID = form["transactionId"],
                        PaymentName = Name + "_" + (form["paymentMethod"] ?? ""),
                        PaymentDate = DateTime.Parse(form["orderCompleteAt"]),

                        Message = "",

                        Customer = new PayResult._Customer
                        {
                            Email = form["email"] ?? "",
                        }
                    };
                }

            return result;
        }

        public PayTicket Pay(string p_OrderId, double p_Amount,
            ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
        {
            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");

            if (string.IsNullOrEmpty(this[APIHash])) throw new ArgumentNullException("APIHash");
            if (string.IsNullOrEmpty(this[APISecret])) throw new ArgumentNullException("APISecret");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            var dic = new Dictionary<string, string>();
            dic.Add("api_hash", this[APIHash]);
            dic.Add("order_id", p_OrderId);
            dic.Add("amount", p_Amount.ToString("0.00"));
            dic.Add("currency", p_Currency.ToString());
            dic.Add("description", p_OrderName);
            dic.Add("url_ok", p_ReturnUrl);
            dic.Add("url_failure", p_CancelUrl);

            dic.Add("items",
                $"[{{\"id\":\"{p_OrderId}\",\"name\":\"{p_OrderName}\",\"sku\":\"{new Random().Next(0, 1000000).ToString("000000")}\",\"price\":{p_Amount.ToString("0.00")},\"qty\":\"{"1"}\",\"amount\":\"{p_Amount.ToString("0.00")}\",\"url\":\"{dic["url_ok"]}\"}}]");

            dic.Add("hash",
                Utils.HASHCrypto.Generate(Utils.HASHCrypto.CryptoEnum.SHA256)
                    .Encrypt(p_OrderId + p_Amount.ToString("0.00") + p_Currency + this[APISecret]));

            dic.Add("notify_url", p_NotifyUrl);

            var res = _HWU.Response(new Uri("https://checkout.pay.g2a.com/index/createQuote"),
                HttpWebUtility.HttpMethod.Post, dic);

            var pt = new PayTicket();

            if (!res.Contains("\"ok\""))
            {
                pt.Message = "生成交易链接失败！" + res;
                return pt;
            }

            var json = Utils.Json.Deserialize<dynamic>(res);

            var formhtml =
                new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
                                  "' action='https://checkout.pay.g2a.com/index/gateway' method='get' >");

            formhtml.AppendFormat("<input type='hidden' name='{0}' value='{1}' />", "token", json.token);

            formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
            formhtml.Append("</form>");

            pt.FormHtml = formhtml.ToString();
            return pt;
        }

        public bool Refund(string email, string transactionId, string userOrderId, double amount, double refundedAmount)
        {
            var headlist = new Dictionary<string, string>();
            var authHash = Utils.Core.SHA256(this[APIHash] + email + this[APISecret]);
            headlist.Add("Authorization", this[APIHash] + ";" + authHash);

            var paramlist = new Dictionary<string, string>();
            var hash = Utils.Core.SHA256(transactionId + userOrderId + amount.ToString("0.00") + this[APISecret]);
            paramlist.Add("action", "refund");
            paramlist.Add("amount", refundedAmount.ToString("0.00"));
            paramlist.Add("hash", hash);

            var uri = new Uri(string.Format(
#if DEBUG
                "https://www.test.pay.g2a.com/rest/transactions/{0}"
#else
                        "https://pay.g2a.com/rest/transactions/{0}"
#endif
                , transactionId));

            var res = _HWU.Response(uri, HttpWebUtility.HttpMethod.Put, paramlist, headlist);

            if (res.Contains("\"ok\""))
                return true;
            return false;
        }
    }
}

//https://pay.g2a.com/documentation/introduction
//https://pay.g2a.com/documentation

//contact@integrations.g2a.com