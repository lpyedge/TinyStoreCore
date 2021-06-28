using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;

namespace LPayments.Plartform.CashRun
{
    
    [PayChannel(EChannel.PaySafeCard)]
    public class Pay_PaySafeCard : IPayChannel, IPay
    {
        public const string MerchantID = "MerchantID";
        public const string PayUrl = "PayUrl";

        public Pay_PaySafeCard() : base()
        {
            Platform = EPlatform.CashRun;
        }

        public Pay_PaySafeCard(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = MerchantID, Description = "PaySafeCard的商家ID", Regex = @"^\w+$", Requied = true},
                new Setting {Name = PayUrl, Description = "CashRun提供的付款地址", Regex = @"^[\w\.:/?=&]+$", Requied = true}
            };

            Currencies = new List<ECurrency>
            {
                ECurrency.EUR,
                ECurrency.USD,
                ECurrency.GBP
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
            var dictionary = form;
            var IsVlidate = dictionary["state"] == "O";

            if (IsVlidate)
            {
                result = new PayResult
                {
                    OrderName = "",
                    OrderID = dictionary["mtid"].Replace(this[MerchantID] + "-", ""),
                    Amount = double.Parse(dictionary["amount"]),
                    Tax = -1,
                    Currency = Utils.Core.Parse<ECurrency>(dictionary["currency"]),
                    Business = dictionary["mid"],
                    TxnID = dictionary["mtid"],
                    PaymentName = Name,
                    PaymentDate = DateTime.UtcNow,

                    Message = "",
                };
            }

            return result;
        }

        public PayTicket Pay(string p_OrderId, double p_Amount,
            ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
        {
            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");

            if (string.IsNullOrEmpty(this[MerchantID])) throw new ArgumentNullException("MerchantID");
            if (string.IsNullOrEmpty(this[PayUrl])) throw new ArgumentNullException("PayUrl");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            // var formhtml =
            //     new StringBuilder(
            //         string.Format(
            //             "<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
            //             "' action='{0}' method='post' >",
            //             this[PayUrl]));
            // formhtml.AppendFormat("<input type='hidden' name='mtid' value='{0}-{1}' />", this[MerchantID], p_OrderId);
            // formhtml.AppendFormat("<input type='hidden' name='language' value='{0}' />", "en");
            // formhtml.AppendFormat("<input type='hidden' name='amount' value='{0}' />", p_Amount.ToString("0.##"));
            // formhtml.AppendFormat("<input type='hidden' name='currency' value='{0}' />", p_Currency);
            // if (p_Currency == ECurrency.USD)
            //     formhtml.Append("<input type='hidden' name='zone' value='US' />");
            // formhtml.AppendFormat("<input type='hidden' name='abort_link' value='{0}' />", p_CancelUrl);
            // formhtml.AppendFormat("<input type='hidden' name='success_link' value='{0}' />", p_ReturnUrl);
            // formhtml.AppendFormat("<input type='hidden' name='notification_link' value='{0}' />", p_NotifyUrl);
            // formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
            // formhtml.Append("</form>");

            var datas = new Dictionary<string, string>()
            {
                ["mtid"] = this[MerchantID] + "-" + p_OrderId,
                ["language"] = "en",
                ["amount"] = p_Amount.ToString("0.##"),
                ["currency"] = p_Currency.ToString(),
                ["abort_link"] = p_CancelUrl,
                ["success_link"] = p_ReturnUrl,
                ["notification_link"] = p_NotifyUrl,
            };

            if (p_Currency == ECurrency.USD)
                datas["zone"] = "US";

            return new PayTicket()
            {
                PayType = PayChannnel.ePayType,
                Action = EAction.UrlPost,
                Uri = this[PayUrl],
                Datas = datas
            };
        }
    }
}