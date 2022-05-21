using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace LPayments.Plartform.PaymentWall
{
    
    [PayChannel(EChannel.PaymentWall)]
    public class PaymentWall : IPayChannel, IPay
    {
        public const string ApiType = "ApiType";
        public const string Widget = "Widget";
        public const string WidgetProjectKey = "WidgetProjectKey";
        public const string WidgetSecretsKey = "WidgetSecretsKey";

        private static readonly string[] ipWhitelist = new string[]
        {
            "174.36.92.186",
            "174.36.96.66",
            "174.36.92.187",
            "174.36.92.192",
            "174.37.14.28"
        };

        protected string m_ps = string.Empty;

        public PaymentWall():base()
        {
        }

        public PaymentWall(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = ApiType, Description = "PaymentWall的支付类型", Regex = @"^\w+$", Requied = true},
                new Setting {Name = Widget, Description = "PaymentWall的Widget值", Regex = @"^\w+$", Requied = true},
                new Setting
                {
                    Name = WidgetProjectKey,
                    Description = "PaymentWall的WidgetProjectKey",
                    Regex = @"^\w+$",
                    Requied = true
                },
                new Setting
                {
                    Name = WidgetSecretsKey,
                    Description = "PaymentWall的WidgetSecretsKey",
                    Regex = @"^\w+$",
                    Requied = true
                }
            };

            Currencies = new List<ECurrency>();
            foreach (var name in Enum.GetNames(typeof(ECurrency)))
                Currencies.Add((ECurrency) Enum.Parse(typeof(ECurrency), name, true));
        }


        public PayResult Notify(IDictionary<string, string> form, IDictionary<string, string> query,
            IDictionary<string, string> head, string body, string notifyip)
        {
            PayResult result = new PayResult
            {
                Status = PayResult.EStatus.Pending,
                Message = "fail"
            };

            var sign = "";
            if (form.ContainsKey("sign_version"))
            {
                var str2sign =
                    form.Where(p => p.Key != "sig").Aggregate("", (x, y) => x += y.Key + "=" + y.Value) +
                    this[WidgetSecretsKey];
                sign = form["sign_version"] == "3"
                    ? Utils.Core.SHA256(str2sign)
                    : Utils.Core.MD5(str2sign);
            }
            else
            {
                var keys = new[] {"uid", "goodsid", "slength", "speriod", "type", "ref"};
                var str2sign = keys.Aggregate("", (current, key) => current + key + "=" + form[key]) +
                               this[WidgetSecretsKey];
                sign = Utils.Core.MD5(str2sign);
            }

            if (string.Equals(sign, form["sig"], StringComparison.OrdinalIgnoreCase)
                && string.Equals("0", form["type"], StringComparison.OrdinalIgnoreCase)
                && ipWhitelist.Contains(notifyip))
            {
                result = new PayResult
                {
                    OrderName = "",
                    OrderID = form.ContainsKey("goodsid") ? form["goodsid"] : "",
                    Amount =
                        form.ContainsKey("PRODUCT_PRICE") ? double.Parse(form["PRODUCT_PRICE"]) : 0,
                    //form.ContainsKey("REVENUE_PAYMENT_LOCAL") ? form["REVENUE_PAYMENT_LOCAL"].Parse<double>() : 0,
                    Tax = -1,
                    Currency =
                        form.ContainsKey("PRODUCT_CURRENCY_CODE")
                            ? Utils.Core.Parse<ECurrency>(form["PRODUCT_CURRENCY_CODE"])
                            : ECurrency.USD,
                    Business = "",
                    TxnID = form.ContainsKey("ref") ? form["ref"] : "",
                    PaymentName = Name,
                    PaymentDate = DateTime.UtcNow,

                    Message = "OK",

                    Customer = new PayResult._Customer
                    {
                        Name = (form.ContainsKey("CC_HOLDER_NAME") ? form["CC_HOLDER_NAME"] : "") + " " +
                               (form.ContainsKey("CC_HOLDER_SURNAME") ? form["CC_HOLDER_SURNAME"] : ""),
                        Street = "",
                        City = "",
                        State = "",
                        Zip = "",
                        Country = form.ContainsKey("COUNTRY_NAME") ? form["COUNTRY_NAME"] : "",
                        Email = form.ContainsKey("uid") ? form["uid"] : "",
                        Phone = "",
                        Business =
                            form.ContainsKey("CC_NUMBER") ? form["CC_NUMBER"].Replace("*", "") : "",
                    }
                };
                //https://docs.paymentwall.com/reference/pingback/custom-parameter
                if (form.ContainsKey("REVENUE_LOCAL"))
                    try
                    {
                        var amount = double.Parse(form["REVENUE_LOCAL"]);
                        if (result.Amount > amount)
                            result.Tax = result.Amount - amount;
                    }
                    catch
                    {
                    }
            }

            return result;
        }

        public PayTicket Pay(string p_OrderId, double p_Amount,
            ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
        {
            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");

            if (string.IsNullOrEmpty(this[ApiType])) throw new ArgumentNullException("ApiType");
            if (string.IsNullOrEmpty(this[Widget])) throw new ArgumentNullException("Widget");
            if (string.IsNullOrEmpty(this[WidgetProjectKey])) throw new ArgumentNullException("WidgetProjectKey");
            if (string.IsNullOrEmpty(this[WidgetSecretsKey])) throw new ArgumentNullException("WidgetSecretsKey");
            if (extend_params == null || extend_params.GetType() != typeof(PayExtend))
                throw new ArgumentException("PayExtend Error");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            var datas = new SortedDictionary<string, string>();
            datas["key"] = this[WidgetProjectKey];
            datas["widget"] = this[Widget];
            datas["lang"] = "en";
            datas["amount"] = p_Amount.ToString("0.##");
            datas["currencyCode"] = p_Currency.ToString();
            datas["ag_external_id"] = p_OrderId;
            datas["ag_name"] = p_OrderName;
            datas["ag_type"] = "fixed";
            datas["sign_version"] = "2";

            if (!string.IsNullOrWhiteSpace(m_ps))
                datas["ps"] = m_ps;

            datas["uid"] = (extend_params as PayExtend).Email; //必填
            datas["email"] = (extend_params as PayExtend).Email; //看情况填
            datas["history[registration_ip]"] = p_ClientIP.ToString(); //看情况填

            datas["success_url"] = p_ReturnUrl;
            datas["pingback_url"] = p_NotifyUrl;

            datas["sign"] =
                Utils.Core.MD5(datas.Aggregate("", (x, y) => x += y.Key + "=" + y.Value) + this[WidgetSecretsKey]);

            // var formhtml =
            //     new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
            //                       "' action='https://api.paymentwall.com/api/" + this[ApiType] + "' method='get' >");
            // foreach (var item in data)
            //     formhtml.AppendFormat("<input type='hidden' name='{0}' value='{1}' />", item.Key, item.Value);
            //
            // formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
            // formhtml.Append("</form>");

            return new PayTicket()
            {
                Name = this.Name,
                DataFormat = EPayDataFormat.Form,
                DataContent = "https://api.paymentwall.com/api/" + this[ApiType] +"??" + Utils.Core.LinkStr(datas,encode:true),
            };
        }

        public class PayExtend
        {
            /// <summary>
            ///     快捷登录授权令牌
            /// </summary>
            public string Email { get; set; }
        }
    }
}

//Parameters Sample
//• SECRET\_KEY = 3b5949e0c26b87767a4752a276de9570
//• uid = 1
//• goodsid = gold\_membership
//• slength = 3
//• speriod = month
//• type = 0
//• ref = 3
//• sig = MD5(uid=[USER_ID]goodsid=[GOODS_ID]slength=[PRODUCT_LENGTH]speriod=[PRODUCT_PERIOD]type=[TYPE]ref=[REF][SECRET_KEY]) = MD5(uid= 1goodsid= gold_membershipslength = 3speriod= monthtype = 0ref= 33b5949e0c26b87767a4752a276de9570) = 84d081d1af73ccdf5f7281a145d03ce6