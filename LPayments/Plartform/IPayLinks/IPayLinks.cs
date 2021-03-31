using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Xml;
using LPayments.Utils;

namespace LPayments.Plartform.IPayLinks
{
    //启赟
    [PayPlatform("IPayLinks", "", SiteUrl = "https://www.ipaylinks.com")]
    [PayChannel(EChannel.CreditCard)]
    public class IPayLinks : IPayChannel, IPay
    {
        public const string PartnerId = "PartnerId";
        public const string SiteId = "SiteId";
        public const string Pkey = "Pkey";

        public IPayLinks() : base()
        {
        }

        public IPayLinks(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting
                {
                    Name = PartnerId,
                    Description = "IPayLinks的会员号(PartnerId)",
                    Regex = @"^\d+$",
                    Requied = true
                },
                new Setting
                {
                    Name = SiteId,
                    Description = "IPayLinks的网站域名(SiteId)",
                    Regex = @"^[\w\.]+$",
                    Requied = true
                },
                new Setting {Name = Pkey, Description = "IPayLinks的MD5密钥(Pkey)", Regex = @"^\w+$", Requied = true}
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

            var xd = new XmlDocument();
            xd.LoadXml(body);
            var dic = new Dictionary<string, string>();
            foreach (XmlNode childNode in xd.ChildNodes[1].ChildNodes)
                if (!string.IsNullOrWhiteSpace(childNode.InnerText))
                    dic[childNode.Name] = childNode.InnerText;

            var signPreStr =
                dic.Where(p => p.Key != "signMsg").Aggregate("", (o, p) => o += p.Key + "=" + p.Value + "&") + "pkey=" +
                this[Pkey];

            if (string.Equals(dic["signMsg"], Utils.Core.MD5(signPreStr), StringComparison.OrdinalIgnoreCase) &&
                dic["resultCode"] == "0000")
            {
                result = new PayResult
                {
                    OrderName = dic["orderId"],
                    OrderID = dic["orderId"],
                    Amount = int.Parse(dic["orderAmount"]) * 0.01,
                    Tax = -1,
                    Currency = Utils.Core.Parse<ECurrency>(dic["currencyCode"]),
                    Business = dic["partnerId"],
                    TxnID = dic["dealId"],
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

            if (string.IsNullOrEmpty(this[SiteId])) throw new ArgumentNullException("SiteId");
            if (string.IsNullOrEmpty(this[PartnerId])) throw new ArgumentNullException("PartnerId");
            if (string.IsNullOrEmpty(this[Pkey])) throw new ArgumentNullException("Pkey");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            if (extend_params == null) throw new ArgumentNullException("extend_params");
            if (!(extend_params is PayExtend)) throw new ArgumentException("extend_params must be PayExtend");
            var pe = extend_params as PayExtend;
            if (pe == null) throw new ArgumentException("extend_params Can not be null");

            var dic = new SortedDictionary<string, string>();
            dic.Add("siteId", this[SiteId]);
            dic.Add("partnerId", this[PartnerId]);

            dic.Add("version", "1.1");
            dic.Add("tradeType", "1001");
            dic.Add("borrowingMarked", "0");
            dic.Add("mcc", "6000");

            dic.Add("orderId", p_OrderId);
            dic.Add("goodsName", p_OrderName);
            dic.Add("goodsDesc", p_OrderName);
            dic.Add("submitTime", DateTime.UtcNow.ToString("yyyyMMddHHmmss"));
            dic.Add("customerIP", p_ClientIP.ToString());
            dic.Add("orderAmount", (p_Amount * 100).ToString("0"));
            dic.Add("currencyCode", p_Currency.ToString());
            dic.Add("payType", "EDC");

            dic.Add("noticeUrl", p_NotifyUrl);

            //支付信息
            dic.Add("payMode", "10");
            dic.Add("cardHolderNumber", pe.cardHolderNumber);
            dic.Add("cardHolderFirstName", pe.cardHolderFirstName);
            dic.Add("cardHolderLastName", pe.cardHolderLastName);
            dic.Add("cardExpirationMonth", pe.cardExpirationMonth);
            dic.Add("cardExpirationYear", pe.cardExpirationYear);
            dic.Add("securityCode", pe.securityCode);
            dic.Add("cardHolderEmail", pe.cardHolderEmail);
            dic.Add("cardHolderPhoneNumber", pe.cardHolderPhoneNumber);

            //安全信息
            dic.Add("deviceFingerprintId", p_OrderId);
            dic.Add("charset", "1");
            dic.Add("signType", "2");

            var signPreStr = dic.Aggregate("", (o, p) => o += p.Key + "=" + p.Value + "&") + "pkey=" + this[Pkey];
            dic.Add("signMsg", Utils.Core.MD5(signPreStr));
            var res = "";

            res = _HWU.Response(new Uri(
#if DEBUG
                    "http://api.test.ipaylinks.com/webgate/crosspay.htm"
#else
                "https://api.ipaylinks.com/webgate/crosspay.htm"
#endif
                ),
                HttpWebUtility.HttpMethod.Post, dic.ToDictionary(p => p.Key, p => p.Value));

            //var rl = Notify(null, null, null , res);
            var pt = new PayTicket();
            pt.Url = p_ReturnUrl;
            pt.Extra = Notify(new Dictionary<string, string>(), new Dictionary<string, string>(),
                new Dictionary<string, string>(), res, p_ClientIP.ToString());
            pt.Sync = false;
            return pt;
        }

        public class PayExtend
        {
            /// <summary>
            ///     卡号
            /// </summary>
            public string cardHolderNumber { get; set; }

            /// <summary>
            ///     持卡人名
            /// </summary>
            public string cardHolderFirstName { get; set; }

            /// <summary>
            ///     持卡人姓
            /// </summary>
            public string cardHolderLastName { get; set; }

            /// <summary>
            ///     有效期月
            /// </summary>
            public string cardExpirationMonth { get; set; }

            /// <summary>
            ///     有效期年
            /// </summary>
            public string cardExpirationYear { get; set; }

            /// <summary>
            ///     持卡人卡片安全码
            /// </summary>
            public string securityCode { get; set; }

            /// <summary>
            ///     持卡人联系邮箱
            /// </summary>
            public string cardHolderEmail { get; set; }

            /// <summary>
            ///     持卡人手机(非必填)
            /// </summary>
            public string cardHolderPhoneNumber { get; set; }
        }
    }
}