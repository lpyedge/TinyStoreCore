using System;
using System.Collections.Generic;
using System.Net;
using System.Text;

namespace LPayments.Plartform.SofortBank
{
    [PayPlatformAttribute("SofortBank", "德国银行支付平台", SiteUrl = "http://www.sofortbank.be")]
    [PayChannel(EChannel.SofortBank)]
    public class SofortBank : IPayChannel, IPay
    {
        //Projects » My projects » Choose a project » Extended settings » passwords and hash algorith 设置 notification password
        //Projects » My projects » Choose a project » Extended settings » Notifications » HTTP 设置 notification url

        public const string UserId = "UserId";

        public const string ProjectId = "ProjectId";

        public const string NotificationPassword = "NotificationPassword";

        public SofortBank() : base()
        {
        }

        public SofortBank(string p_SettingsJson) : this()
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
                //new Setting{ Name=SiteId, Description= "SofortBank的Site ID", Regex=@"^\w+$", Requied=true},
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

            bool IsVlidate;
            if (string.IsNullOrWhiteSpace(this[NotificationPassword]))
            {
                IsVlidate = this[UserId] == form["user_id"] && this[ProjectId] == form["project_id"];
            }
            else
            {
                var Input = form["transaction"] + "|" +
                            form["user_id"] + "|" +
                            form["project_id"] + "|" +
                            form["sender_holder"] + "|" +
                            form["sender_account_number"] + "|" +
                            form["sender_bank_code"] + "|" +
                            form["sender_bank_name"] + "|" +
                            form["sender_bank_bic"] + "|" +
                            form["sender_iban"] + "|" +
                            form["sender_country_id"] + "|" +
                            form["recipient_holder"] + "|" +
                            form["recipient_account_number"] + "|" +
                            form["recipient_bank_code"] + "|" +
                            form["recipient_bank_name"] + "|" +
                            form["recipient_bank_bic"] + "|" +
                            form["recipient_iban"] + "|" +
                            form["recipient_country_id"] + "|" +
                            form["international_transaction"] + "|" +
                            form["amount"] + "|" +
                            form["currency_id"] + "|" +
                            form["reason_1"] + "|" +
                            form["reason_2"] + "|" +
                            form["security_criteria"] + "|" +
                            form["user_variable_0"] + "|" +
                            form["user_variable_1"] + "|" +
                            form["user_variable_2"] + "|" +
                            form["user_variable_3"] + "|" +
                            form["user_variable_4"] + "|" +
                            form["user_variable_5"] + "|" +
                            form["created "] + "|" +
                            this[NotificationPassword];

                IsVlidate = string.Equals(Utils.Core.MD5(Input), form["hash"], StringComparison.OrdinalIgnoreCase) &&
                            this[UserId] == form["user_id"] &&
                            this[ProjectId] == form["project_id"];
            }

            if (IsVlidate)
            {
                result = new PayResult
                {
                    OrderName = form["reason_2"],
                    OrderID = form["reason_1"],
                    Amount = double.Parse(form["amount"]),
                    Tax = -1,
                    Currency = Utils.Core.Parse<ECurrency>(form["currency_id"]),
                    Business = form["user_id"] + "_" + form["project_id"],
                    TxnID = form["transaction"],
                    PaymentName = Name,
                    PaymentDate = DateTime.UtcNow,

                    Message = "",

                    Customer = new PayResult._Customer
                    {
                        Name = form.ContainsKey("sender_holder") ? form["sender_holder"] : "",
                        Business = form.ContainsKey("sender_account_number") ? form["sender_account_number"] : "",
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

            if (string.IsNullOrEmpty(this[UserId])) throw new ArgumentNullException("CustomerId");
            if (string.IsNullOrEmpty(this[ProjectId])) throw new ArgumentNullException("ProjectId");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            var formhtml =
                new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
                                  "' action='https://www.directebanking.com/payment/start' method='post' >");
            formhtml.AppendFormat("<input type='hidden' name='user_id' value='{0}' />", this[UserId]);
            formhtml.AppendFormat("<input type='hidden' name='language_id' value='{0}' />", "en");
            formhtml.AppendFormat("<input type='hidden' name='amount' value='{0}' />", p_Amount.ToString("0.##"));
            formhtml.AppendFormat("<input type='hidden' name='currency_id' value='{0}' />", p_Currency);
            formhtml.AppendFormat("<input type='hidden' name='project_id' value='{0}' />", this[ProjectId]);
            formhtml.AppendFormat("<input type='hidden' name='reason_1' value='{0}' />", p_OrderId);
            formhtml.AppendFormat("<input type='hidden' name='reason_2' value='{0}' />",
                p_OrderName.Length > 27 ? p_OrderName.Substring(0, 27) : p_OrderName);
            formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
            formhtml.Append("</form>");

            var pt = new PayTicket();
            pt.FormHtml = formhtml.ToString();
            return pt;
        }
    }
}

//https://www.sofort.com/eng-GB/merchant/integration/

//https://www.sofort.com/eng-GB/content/download/1027/16737/file/Interface%20description_SOFORT_Banking.pdf