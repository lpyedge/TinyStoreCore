using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;

namespace LPayments.Plartform.WebMoney
{
    [PayPlatformAttribute("WebMoney", "", SiteUrl = "https://www.wmtransfer.com")]
    [PayChannel(EChannel.WebMoney)]
    public class WebMoney : IPayChannel, IPay
    {
        public const string PID = "PID";
        public const string Key = "Key";

        public WebMoney() : base()
        {
        }

        public WebMoney(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = PID, Description = "WebMoney的商家电子钱包", Regex = @"^[\w\.@]+$", Requied = true},
                new Setting {Name = Key, Description = "WebMoney的密钥(Key)", Regex = @"^\w+$", Requied = true}
            };

            if (string.IsNullOrWhiteSpace(this[PID]))
                Currencies = new List<ECurrency>
                {
                    ECurrency.USD,
                    ECurrency.RUB,
                    ECurrency.EUR,
                    ECurrency.UAH,
                    ECurrency.BYR
                };
            else
                switch (this[PID].Substring(0, 1).ToUpperInvariant())
                {
                    case "Z":
                        Currencies = new List<ECurrency>
                        {
                            ECurrency.USD
                        };
                        break;

                    case "R":
                        Currencies = new List<ECurrency>
                        {
                            ECurrency.RUB
                        };
                        break;

                    case "E":
                        Currencies = new List<ECurrency>
                        {
                            ECurrency.EUR
                        };
                        break;

                    case "U":
                        Currencies = new List<ECurrency>
                        {
                            ECurrency.UAH
                        };
                        break;

                    case "B":
                        Currencies = new List<ECurrency>
                        {
                            ECurrency.BYR
                        };
                        break;

                    default:
                        Currencies = new List<ECurrency>
                        {
                            ECurrency.USD,
                            ECurrency.RUB,
                            ECurrency.EUR,
                            ECurrency.UAH,
                            ECurrency.BYR
                        };
                        break;
                }
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
                IsVlidate = string.IsNullOrWhiteSpace(form["LMI_PREREQUEST"]) && form["LMI_MODE"] == "0" &&
                            form["LMI_PAYEE_PURSE"] == this[PID];
            else
                IsVlidate = string.IsNullOrWhiteSpace(form["LMI_PREREQUEST"]) && form["LMI_MODE"] == "0" &&
                            form["LMI_PAYEE_PURSE"] == this[PID] &&
                            string.Equals(form["LMI_HASH"],
                                Utils.Core.MD5(form["LMI_PAYEE_PURSE"] + form["LMI_PAYMENT_AMOUNT"] +
                                               form["LMI_PAYMENT_NO"] + form["LMI_MODE"] +
                                               form["LMI_SYS_INVS_NO"]
                                               + form["LMI_SYS_TRANS_NO"] + form["LMI_SYS_TRANS_DATE"] +
                                               this[Key] +
                                               form["LMI_PAYER_PURSE"] + form["LMI_PAYER_WM"]),
                                StringComparison.OrdinalIgnoreCase);

            if (IsVlidate)
            {
                result = new PayResult
                {
                    OrderName = form["OrderName"],
                    OrderID = form["OrderId"],
                    Amount = double.Parse(form["LMI_PAYMENT_AMOUNT"]),
                    Tax = -1,
                    Currency = Currencies[0],
                    Business = form["LMI_PAYEE_PURSE"],
                    TxnID = form["LMI_SYS_TRANS_NO"],
                    PaymentName = Name,
                    PaymentDate = DateTime.UtcNow,

                    Message = "",

                    Customer = new PayResult._Customer
                    {
                        Email = string.IsNullOrWhiteSpace(form["LMI_PAYMER_EMAIL"])
                            ? (form["LMI_EURONOTE_EMAIL"] ?? "")
                            : form["LMI_PAYMER_EMAIL"],
                        Phone = form["LMI_TELEPAT_PHONENUMBER"] ?? "",
                        Business = form["LMI_PAYER_WM"],
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

            if (string.IsNullOrEmpty(this[PID])) throw new ArgumentNullException("PID");
            if (string.IsNullOrEmpty(this[Key])) throw new ArgumentNullException("Key");

            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            // var formhtml =
            //     new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
            //                       "' action='https://merchant.wmtransfer.com/lmi/payment.asp' method='post' >");
            // formhtml.AppendFormat("<input type='hidden' name='LMI_PAYEE_PURSE' value='{0}' />", this[PID]);
            // formhtml.AppendFormat("<input type='hidden' name='LMI_PAYMENT_AMOUNT' value='{0}' />",
            //     p_Amount.ToString("0.##"));
            // formhtml.AppendFormat("<input type='hidden' name='OrderId' value='{0}' />", p_OrderId);
            // formhtml.AppendFormat("<input type='hidden' name='OrderName' value='{0}' />", p_OrderName);
            // formhtml.AppendFormat("<input type='hidden' name='LMI_PAYMENT_DESC_BASE64' value='{0}' />",
            //     Convert.ToBase64String(Encoding.UTF8.GetBytes(p_OrderName)));
            // formhtml.AppendFormat("<input type='hidden' name='LMI_SUCCESS_URL' value='{0}' />", p_ReturnUrl);
            // formhtml.AppendFormat("<input type='hidden' name='LMI_RESULT_URL' value='{0}' />", p_NotifyUrl);
            // formhtml.Append("<input type='hidden' name='LMI_SUCCESS_METHOD' value='2' />");
            // //formhtml.Append("<input type='hidden' name='LMI_SIM_MODE' value='0' />");
            // formhtml.Append("<input type='hidden' name='recipient_description' value='" + new Uri(p_ReturnUrl).Scheme +
            //                 "://" + new Uri(p_ReturnUrl).Authority + "' />");
            // formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
            // formhtml.Append("</form>");

            var datas = new Dictionary<string, string>()
            {
                ["LMI_PAYEE_PURSE"]=this[PID],
                ["LMI_PAYMENT_AMOUNT"]= p_Amount.ToString("0.##"),
                ["OrderId"]= p_OrderId,
                ["sssss"]= p_OrderName,
                ["LMI_PAYMENT_DESC_BASE64"]= Convert.ToBase64String(Encoding.UTF8.GetBytes(p_OrderName)),
                ["LMI_SUCCESS_URL"]= p_ReturnUrl,
                ["LMI_RESULT_URL"]= p_NotifyUrl,
                ["LMI_SUCCESS_METHOD"]= "2",
                ["recipient_description"]= new Uri(p_ReturnUrl).Scheme + "://" + new Uri(p_ReturnUrl).Authority ,
            };
            
            return new PayTicket()
            {
                Action = EAction.UrlPost,
                Uri = "https://merchant.wmtransfer.com/lmi/payment.asp",
                Datas = datas
            };
        }
    }
}

//https://wiki.wmtransfer.com/projects/webmoney/wiki/Web_Merchant_Interface

//http://baike.baidu.com/view/2557078.htm