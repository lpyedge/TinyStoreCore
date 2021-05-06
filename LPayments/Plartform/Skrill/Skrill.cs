using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;

namespace LPayments.Plartform.Skrill
{
    
    [PayChannel(EChannel.CreditCard)]
    public class Skrill : IPayChannel, IPay
    {
        public const string Account = "Account";
        public const string Key = "Key";

        public Skrill() : base()
        {
            Platform = EPlatform.Skrill;
        }

        public Skrill(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting
                {
                    Name = Account,
                    Description = "Skrill的商家帐号(Email地址)",
                    Regex = @"^[\w\.@]+$",
                    Requied = true
                },
                new Setting
                {
                    Name = Key,
                    Description = "Skrill在Merchant Tools 自己设置的密钥",
                    Regex = @"^\w+$",
                    Requied = false
                }
            };

            Currencies = new List<ECurrency>
            {
                ECurrency.USD,
                ECurrency.EUR,
                ECurrency.GBP,
                ECurrency.HKD,
                ECurrency.SGD,
                ECurrency.JPY,
                ECurrency.CAD,
                ECurrency.CHF,
                ECurrency.DKK,
                ECurrency.SEK,
                ECurrency.NOK,
                ECurrency.ILS,
                ECurrency.MYR,
                ECurrency.NZD,
                ECurrency.TRY,
                ECurrency.AED,
                ECurrency.MAD,
                ECurrency.QAR,
                ECurrency.SAR,
                ECurrency.TWD,
                ECurrency.THB,
                ECurrency.CZK,
                ECurrency.HUF,
                ECurrency.BGN,
                ECurrency.PLN,
                ECurrency.ISK,
                ECurrency.INR,
                ECurrency.KRW,
                ECurrency.ZAR,
                ECurrency.RON,
                ECurrency.HRK,
                ECurrency.JOD,
                ECurrency.OMR,
                ECurrency.RSD,
                ECurrency.TND
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

            bool IsVlidate;
            if (string.IsNullOrWhiteSpace(this[Key]))
                IsVlidate = form["status"] == "2" && form["pay_to_email"] == this[Account];
            else
                IsVlidate = string.Equals(form["md5sig"],
                                Utils.Core.MD5(form["merchant_id"] + form["transaction_id"] +
                                               Utils.Core.MD5(this[Key]).ToUpperInvariant() + form["mb_amount"] +
                                               form["mb_currency"] + form["status"]),
                                StringComparison.OrdinalIgnoreCase)
                            && form["status"] == "2"
                            && form["pay_to_email"] == this[Account];

            if (IsVlidate)
            {
                result = new PayResult
                {
                    OrderName = form.ContainsKey("merchant_fields") ? form["merchant_fields"] : "",
                    OrderID = form["transaction_id"],
                    Amount = double.Parse(form["amount"]),
                    Tax = -1,
                    Currency = Utils.Core.Parse<ECurrency>(form["currency"]),
                    Business = form["pay_to_email"],
                    TxnID = form["mb_transaction_id"],
                    PaymentName = Name,
                    PaymentDate = DateTime.UtcNow,

                    Message = "",

                    Customer = new PayResult._Customer
                    {
                        Email = form.ContainsKey("pay_from_email") ? form["pay_from_email"] : "",
                        Business = form.ContainsKey("pay_from_email") ? form["pay_from_email"] : "",
                    }
                };
            }

            return result;
        }

        public virtual PayTicket Pay(string p_OrderId, double p_Amount,
            ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
        {
            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");

            if (string.IsNullOrEmpty(this[Account])) throw new ArgumentNullException("Account");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            // var formhtml =
            //     new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
            //                       "' action='https://www.skrill.com/app/payment.pl' method='post' >");
            //
            // formhtml.AppendFormat("<input type='hidden' name='pay_to_email' value='{0}' />", this[Account]);
            // formhtml.AppendFormat("<input type='hidden' name='language' value='{0}' />", "en");
            // formhtml.AppendFormat("<input type='hidden' name='amount' value='{0}' />", p_Amount.ToString("0.##"));
            // formhtml.AppendFormat("<input type='hidden' name='currency' value='{0}' />", p_Currency);
            // formhtml.AppendFormat("<input type='hidden' name='transaction_id' value='{0}' />", p_OrderId);
            // formhtml.AppendFormat("<input type='hidden' name='detail1_description' value='{0}' />", p_OrderName);
            // formhtml.AppendFormat("<input type='hidden' name='return_url' value='{0}' />", p_ReturnUrl);
            // formhtml.AppendFormat("<input type='hidden' name='status_url' value='{0}' />", p_NotifyUrl);
            // formhtml.AppendFormat("<input type='hidden' name='cancel_url' value='{0}' />", p_CancelUrl);
            // formhtml.AppendFormat("<input type='hidden' name='merchant_fields' value='{0}' />", p_OrderName);
            // formhtml.Append("<input type='hidden' name='recipient_description' value='" + new Uri(p_ReturnUrl).Scheme +
            //                 "://" + new Uri(p_ReturnUrl).Authority + "' />");
            // formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
            // formhtml.Append("</form>");

            var datas = new Dictionary<string, string>()
            {
                ["pay_to_email"] = this[Account],
                ["language"] = "en",
                ["amount"] = p_Amount.ToString("0.##"),
                ["currency"] = p_Currency.ToString(),
                ["transaction_id"] = p_OrderId,
                ["detail1_description"] = p_OrderName,
                ["return_url"] = p_ReturnUrl,
                ["status_url"] = p_NotifyUrl,
                ["cancel_url"] = p_CancelUrl,
                ["merchant_fields"] = p_OrderName,
                ["recipient_description"] = new Uri(p_ReturnUrl).Scheme + "://" + new Uri(p_ReturnUrl).Authority,
            };

            return new PayTicket()
            {
                Action = EAction.UrlPost,
                Uri = "https://www.skrill.com/app/payment.pl",
                Datas = datas
            };
        }
    }
}

//https://www.skrill.com/cn/skrill%E9%9B%86%E6%88%90/

//https://www.skrill.com/fileadmin/content/pdf/Skrill_Quick_Checkout_Guide.pdf
//https://www.skrill.com/fileadmin/content/pdf/Skrill_Wallet_Checkout_Guide.pdf
//https://www.skrill.com/fileadmin/content/pdf/Skrill_Automated_Payments_Interface_Guide.pdf