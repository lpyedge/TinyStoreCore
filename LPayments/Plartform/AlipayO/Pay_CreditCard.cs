using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;
using LPayments.Utils;

namespace LPayments.Plartform.AliPayO
{
    [PayChannel(EChannel.CreditCard)]
    public class AliPayO_CreditCard : IPayChannel, IPay
    {
        public const string UserId = "UserId";
        public const string Password = "Password";
        public const string EntityId = "EntityId";
        public const string Brands = "Brands";

        public AliPayO_CreditCard() : base()
        {
            Platform = EPlatform.AlipayO;
        }

        public AliPayO_CreditCard(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = UserId, Description = "支付宝的（UserId）", Regex = @"^\w+$", Requied = true},
                new Setting {Name = Password, Description = "支付宝的（Password）", Regex = @"^\w+$", Requied = true},
                new Setting {Name = EntityId, Description = "支付宝的（EntityId）", Regex = @"^\w+$", Requied = true},
                new Setting {Name = Brands, Description = "支付宝的（Brands）", Regex = @"^[\w ]+$", Requied = true}
            };

            Currencies = new List<ECurrency>
            {
                ECurrency.CNY,
                ECurrency.USD,
                ECurrency.EUR,
                ECurrency.AUD,
                ECurrency.GBP,
                ECurrency.RUB,
                ECurrency.HKD
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

            var uri =
                new Uri(
                    string.Format(
#if DEBUG
                        "https://test.oppwa.com/v1/checkouts/{0}/payment?authentication.userId={1}&authentication.password={2}&authentication.entityId={3}",
#else
                        "https://oppwa.com/v1/checkouts/{0}/payment?authentication.userId={1}&authentication.password={2}&authentication.entityId={3}",
#endif
                        dictionary["id"], this[UserId], this[Password], this[EntityId]));

            var res = _HWU.Response(uri);
            if (res.Contains("code\":\"000"))
            {
                var json = Utils.DynamicJson.Parse(res);
                result = new PayResult
                {
                    OrderName = json.customParameters.ALIRISK_item1ItemProductName,
                    OrderID = json.merchantTransactionId,
                    Amount = double.Parse(json.amount),
                    Tax = -1,
                    Currency = json.currency,
                    Business = this[UserId],
                    TxnID = json.id,
                    PaymentName = Name,
                    PaymentDate = DateTime.UtcNow,

                    Message = "success",
                };
            }

            return result;
        }

        public PayTicket Pay(string p_OrderId, double p_Amount,
            ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
        {
            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");

            if (string.IsNullOrEmpty(this[Brands])) throw new ArgumentNullException("Brands");
            if (string.IsNullOrEmpty(this[UserId])) throw new ArgumentNullException("UserId");
            if (string.IsNullOrEmpty(this[Password])) throw new ArgumentNullException("Password");
            if (string.IsNullOrEmpty(this[EntityId])) throw new ArgumentNullException("EntityId");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            if (extend_params == null) throw new ArgumentNullException("extend_params");
            if (!(extend_params is PayExtend)) throw new ArgumentException("extend_params must be PayExtend");

            //https://alipay-seller.docs.oppwa.com/?q=integration-guide
            //https://alipay-seller.docs.oppwa.com/parameters

            var checkoutId = string.Empty;
            var data = new Dictionary<string, string>
            {
                {"authentication.userId", this[UserId]},
                {"authentication.password", this[Password]},
                {"authentication.entityId", this[EntityId]},
                {"paymentType", "PA.CP"},
                {"amount", p_Amount.ToString("0.##")},
                {"currency", p_Currency.ToString()},
                {"merchantTransactionId", p_OrderId},
                {"customParameters[ALIRISK_clientIP]", p_ClientIP.ToString()},
                //{"customParameters[ALIRISK_merchantWebsiteLanguage] ",Core.ClientLanguage},
                {"customParameters[ALIRISK_orderNo]", p_OrderId},
                {"customParameters[ALIRISK_item1ItemProductName]", p_OrderName},
                {"customParameters[ALIRISK_item1ItemQuantity]", "1"},
                {"customParameters[ALIRISK_item1ItemUnitPrice]", ((int) (p_Amount * 100)).ToString()},
                {"customParameters[ALIRISK_item1ItemUnitPriceCurrency]", p_Currency.ToString()},
                {"customParameters[ALIRISK_sessionID]", Guid.NewGuid().ToString().Replace("-", "")},
                {"customParameters[ALIRISK_txnAmount]", ((int) (p_Amount * 100)).ToString()},
                {"customParameters[ALIRISK_txnCurrency]", p_Currency.ToString()}
            };

            var pe = extend_params as PayExtend;
            if (pe != null)
            {
                if (!string.IsNullOrWhiteSpace(pe.Email))
                {
                    data.Add("customParameters[ALIRISK_billToEmail]", pe.Email);
                    data.Add("customParameters[ALIRISK_buyerEmail]", pe.Email);
                }

                if (!string.IsNullOrWhiteSpace(pe.Phone))
                {
                    data.Add("customParameters[ALIRISK_billToPhoneNumber]", pe.Phone);
                    data.Add("customParameters[ALIRISK_buyerMobile]", pe.Phone);
                }

                if (!string.IsNullOrWhiteSpace(pe.Firstname) && !string.IsNullOrWhiteSpace(pe.Lastname))
                    data.Add("customParameters[ALIRISK_buyerRealName]", pe.Firstname + " " + pe.Lastname);
                if (!string.IsNullOrWhiteSpace(pe.GameName))
                    data.Add("customParameters[ALIRISK_extendProperties_gameName]", pe.GameName);
                if (!string.IsNullOrWhiteSpace(pe.ServerName))
                    data.Add("customParameters[ALIRISK_extendProperties_gameServer]", pe.ServerName);
                if (!string.IsNullOrWhiteSpace(pe.GameTeam))
                    data.Add("customParameters[ALIRISK_extendProperties_gameTeam]", pe.GameTeam);
                if (!string.IsNullOrWhiteSpace(pe.Character))
                    data.Add("customParameters[ALIRISK_extendProperties_gamePlayer]", pe.Character);
            }

#if DEBUG
            var uri = new Uri("https://test.oppwa.com/v1/checkouts");
#else
            var uri = new Uri("https://oppwa.com/v1/checkouts");
#endif

            var res = _HWU.Response(uri, HttpWebUtility.HttpMethod.Post, data);
            var json = Utils.DynamicJson.Parse(res);
            if (res.Contains("code\":\"000"))
                checkoutId = json.id;

            return new PayTicket()
            {
                Action = EAction.Token,
                Token = checkoutId,
                //Uri = "https://oppwa.com/v1/paymentWidgets.js?checkoutId="
            };

//             var formhtml = new StringBuilder("<div id=\"paydialog\">");
//
//             formhtml.AppendFormat("<form action=\"{1}\" class=\"paymentWidgets\">{0}</form>", this[Brands],
//                 p_NotifyUrl);
//             formhtml.Append("</div>");
//
//             formhtml.Append("<style>" +
//                             "#paydialog {position: absolute; top: 25%;width:100%;}" +
//                             "#paydialog div.wpwl-container {width: 400px;margin:0px auto;background:#ffffff;text-align:left;}" +
//                             "#paydialog form.wpwl-form-card { border-radius: 10px;background: none; background-image: none}" +
//                             "#paydialog div.wpwl-group {clear: both; overflow: hidden;width:100%;padding-right:0px;}" +
//                             "#paydialog div.wpwl-group div {float: left}" +
//                             "#paydialog div.wpwl-wrapper {width: 66.6667%}" +
//                             "#paydialog div.wpwl-label {width:33.3333%;}" +
//                             "#paydialog div.wpwl-control-brand {width: auto}" +
//                             "#paydialog .wpwl-group-expiry, #paydialog .wpwl-group-cvv {width:100%}" +
//                             "#paydialog form.wpwl-form-card div.wpwl-group div.wpwl-wrapper-submit {float: right}" +
//                             "#paydialog div.wpwl-group-cardNumber,#paydialog div.wpwl-group-cardHolder { padding-right: 0px;width: 100%;}" +
//                             "#paydialog div.wpwl-wrapper-brand, #paydialog  div.wpwl-brand-card { width: 33.3333%;}" +
//                             "#paydialog div.wpwl-wrapper-brand{width:110px;}" +
//                             "#paydialog div.wpwl-wrapper-cardHolder,#paydialog div.wpwl-wrapper-cvv{width: 100px;}" +
//                             "#paydialog div.wpwl-message{background:#fff;}" +
//                             "</style>");
//             formhtml.Append("<script>" +
//                             "var script_element = document.createElement('script');" +
//                             "script_element.type = 'text/javascript';" +
// #if DEBUG
//                             "script_element.src = 'https://test.oppwa.com/v1/paymentWidgets.js?checkoutId=" +
//                             checkoutId +
//                             "';" +
// #else
//                 "script_element.src = 'https://oppwa.com/v1/paymentWidgets.js?checkoutId=" + checkoutId + "';" +
// #endif
//                             "document.body.appendChild(script_element);" +
//                             "</script>");
//
//             var pt = new PayTicket();
//             pt.FormHtml = formhtml.ToString();
//             return pt;
        }

        public class PayExtend
        {
            /// <summary>
            ///     当前用户邮箱
            /// </summary>
            public string Email { get; set; }

            /// <summary>
            ///     当前用户电话
            /// </summary>
            public string Phone { get; set; }

            /// <summary>
            ///     当前用户Firstname
            /// </summary>
            public string Firstname { get; set; }

            /// <summary>
            ///     当前用户Lastname
            /// </summary>
            public string Lastname { get; set; }

            /// <summary>
            ///     当前下单游戏名
            /// </summary>
            public string GameName { get; set; }

            /// <summary>
            ///     当前下单服务器名
            /// </summary>
            public string ServerName { get; set; }

            /// <summary>
            ///     角色名称 Fifa14必须传输(球员名)
            /// </summary>
            public string Character { get; set; }

            /// <summary>
            ///     Fifa14必须传输(队伍)
            /// </summary>
            public string GameTeam { get; set; }
        }
    }
}