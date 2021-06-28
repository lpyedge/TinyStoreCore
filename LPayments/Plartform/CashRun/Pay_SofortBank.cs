using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;

namespace LPayments.Plartform.CashRun
{
    [PayChannel(EChannel.SofortBank)]
    public class Pay_SofortBank : IPayChannel, IPay
    {
        public const string UserId = "UserId";
        public const string ProjectId = "ProjectId";
        public const string SiteId = "SiteId";
        public const string NotificationPassword = "NotificationPassword";

        public Pay_SofortBank():base()
        {
            Platform = EPlatform.CashRun;
        }

        public Pay_SofortBank(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = UserId, Description = "SofortBank提供的User Id", Regex = @"^\w+$", Requied = true},
                new Setting
                {
                    Name = ProjectId,
                    Description = "SofortBank提供的Project ID",
                    Regex = @"^\w+$",
                    Requied = true
                },
                new Setting {Name = SiteId, Description = "SofortBank的Site ID", Regex = @"^\w+$", Requied = true},
                new Setting
                {
                    Name = NotificationPassword,
                    Description =
                        "SofortBank提供的Notification_Password,还要在对应的Project里设置 Notifications » HTTP notification url 的值才能获取付款完成通知",
                    Regex = @"^\w+$",
                    Requied = false
                }
            };

            Currencies = new List<ECurrency>
            {
                ECurrency.EUR,
                ECurrency.CHF,
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
            bool IsVlidate;
            if (string.IsNullOrWhiteSpace(this[NotificationPassword]))
            {
                IsVlidate = this[UserId] == dictionary["user_id"] && this[ProjectId] == dictionary["project_id"];
            }
            else
            {
                var Input = dictionary["transaction"] + "|" +
                            dictionary["user_id"] + "|" +
                            dictionary["project_id"] + "|" +
                            dictionary["sender_holder"] + "|" +
                            dictionary["sender_account_number"] + "|" +
                            dictionary["sender_bank_code"] + "|" +
                            dictionary["sender_bank_name"] + "|" +
                            dictionary["sender_bank_bic"] + "|" +
                            dictionary["sender_iban"] + "|" +
                            dictionary["sender_country_id"] + "|" +
                            dictionary["recipient_holder"] + "|" +
                            dictionary["recipient_account_number"] + "|" +
                            dictionary["recipient_bank_code"] + "|" +
                            dictionary["recipient_bank_name"] + "|" +
                            dictionary["recipient_bank_bic"] + "|" +
                            dictionary["recipient_iban"] + "|" +
                            dictionary["recipient_country_id"] + "|" +
                            dictionary["international_transaction"] + "|" +
                            dictionary["amount"] + "|" +
                            dictionary["currency_id"] + "|" +
                            dictionary["reason_1"] + "|" +
                            dictionary["reason_2"] + "|" +
                            dictionary["security_criteria"] + "|" +
                            dictionary["user_variable_0"] + "|" +
                            dictionary["user_variable_1"] + "|" +
                            dictionary["user_variable_2"] + "|" +
                            dictionary["user_variable_3"] + "|" +
                            dictionary["user_variable_4"] + "|" +
                            dictionary["user_variable_5"] + "|" +
                            dictionary["created "] + "|" +
                            this[NotificationPassword];

                IsVlidate = string.Equals(Utils.Core.MD5(Input),
                                dictionary["hash"], StringComparison.OrdinalIgnoreCase) &&
                            this[UserId] == dictionary["user_id"] &&
                            this[ProjectId] == dictionary["project_id"];
            }

            if (IsVlidate)
            {
                result = new PayResult
                {
                    OrderName = dictionary["reason_2"],
                    OrderID = dictionary["reason_1"],
                    Amount = double.Parse(dictionary["amount"]),
                    Tax = -1,
                    Currency = Utils.Core.Parse<ECurrency>(dictionary["currency_id"]),
                    Business = dictionary["user_id"] + "_" + dictionary["project_id"],
                    TxnID = dictionary["transaction"],
                    PaymentName = Name,
                    PaymentDate = DateTime.UtcNow,

                    Message = "",

                    Customer = new PayResult._Customer
                    {
                        Name = dictionary["sender_holder"],
                        Business = dictionary["sender_account_number"],
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

            if (string.IsNullOrEmpty(this[UserId])) throw new ArgumentNullException("UserId");
            if (string.IsNullOrEmpty(this[ProjectId])) throw new ArgumentNullException("ProjectId");
            if (string.IsNullOrEmpty(this[SiteId])) throw new ArgumentNullException("SiteId");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            // var formhtml =
            //     new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
            //                       "' action='https://www.directebanking.com/payment/start' method='post'>");
            //
            // formhtml.AppendFormat("<input type='hidden' name='user_id' value='{0}' />", this[UserId]);
            // formhtml.AppendFormat("<input type='hidden' name='language_id' value='{0}' />", "en");
            // formhtml.AppendFormat("<input type='hidden' name='amount' value='{0}' />", p_Amount.ToString("0.##"));
            // formhtml.AppendFormat("<input type='hidden' name='currency_id' value='{0}' />", p_Currency);
            // formhtml.AppendFormat("<input type='hidden' name='project_id' value='{0}' />", this[ProjectId]);
            // formhtml.AppendFormat("<input type='hidden' name='reason_1' value='{0}' />", p_OrderId);
            // formhtml.AppendFormat("<input type='hidden' name='reason_2' value='{0}' />",
            //     p_OrderName.Length > 27 ? p_OrderName.Substring(0, 27) : p_OrderName);
            // formhtml.AppendFormat("<input type='hidden' name='user_variable_0' value='{0};{1}' />", this[SiteId],
            //     p_OrderId);
            // formhtml.AppendFormat("<input type='hidden' name='user_variable_1' value='{0}' />", p_ReturnUrl);
            // formhtml.AppendFormat("<input type='hidden' name='user_variable_2' value='{0}' />", p_CancelUrl);
            // formhtml.AppendFormat("<input type='hidden' name='user_variable_3' value='{0}' />", p_NotifyUrl);
            // formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
            // formhtml.Append("</form>");
            
            var datas = new Dictionary<string, string>()
            {
                ["user_id"] = this[UserId] ,
                ["language_id"] = "en",
                ["amount"] = p_Amount.ToString("0.##"),
                ["currency_id"] = p_Currency.ToString(),
                ["project_id"] = this[ProjectId],
                ["reason_1"] = p_OrderId,
                ["reason_2"] = p_OrderName.Length > 27 ? p_OrderName.Substring(0, 27) : p_OrderName,
                ["user_variable_0"] = this[SiteId],
                ["user_variable_1"] = p_ReturnUrl,
                ["user_variable_2"] = p_CancelUrl,
                ["user_variable_3"] = p_NotifyUrl,
            };

            return new PayTicket()
            {
                PayType = PayChannnel.ePayType,
                Action = EAction.UrlPost,
                Uri = "https://www.directebanking.com/payment/start",
                Datas = datas
            };
        }
    }
}