using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace LPayments.Plartform.KuaiQian
{
    [PayChannel(EChannel.ChinaBanks)]
    public class KuaiQian : IPayChannel, IPay
    {
        public const string MerchantID = "MerchantID";
        public const string PublicRSAXml = "PublicRSAXml";
        public const string PrivateRSAXml = "PrivateRSAXml";

        private const string GATEWAY =
#if DEBUG
            "https://sandbox.99bill.com/gateway/recvMerchantInfoAction.htm";
#else
           "https://www.99bill.com/gateway/recvMerchantInfoAction.htm";
#endif

        protected string m_payType = "00";
        protected string m_bankId = "";

        public KuaiQian() : base()
        {
            Platform = EPlatform.KuaiQian;
        }

        public KuaiQian(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = MerchantID, Description = "快钱商户号", Regex = @"^[\w]+$", Requied = true},
                new Setting {Name = PublicRSAXml, Description = "快钱平台的XML公钥", Regex = @"^[\w/+=<>]+$", Requied = true},
                new Setting
                {
                    Name = PrivateRSAXml, Description = "快钱商户商户的XML私钥", Regex = @"^[\w/+=<>]+$", Requied = true
                },
            };

            Currencies = new List<ECurrency>
            {
                ECurrency.CNY
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
            var vstr = new StringBuilder();

            var IsVlidate = false;
            if (query["merchantAcctId"] == this[MerchantID] && query["payResult"] == "10")
            {
                //签名顺序严格要求一致,值非空的才进行签名

                var signstr = "";
                var singnamelist = new List<string>()
                {
                    "merchantAcctId",
                    "version",
                    "language",
                    "signType",
                    "payType",
                    "bankId",
                    "orderId",
                    "orderTime",
                    "orderAmount",
                    "bindCard",
                    "bindMobile",
                    "dealId",
                    "bankDealId",
                    "dealTime",
                    "payAmount",
                    "fee",
                    "ext1",
                    "ext2",
                    "payResult",
                    "errCode",
                };
                foreach (var singname in singnamelist)
                {
                    if (query.ContainsKey(singname) && !string.IsNullOrEmpty(query[singname]))
                    {
                        signstr += (singname + "=" + query[singname] + "&");
                    }
                }

                signstr = signstr.TrimEnd('&');

                query.Where(p => !string.IsNullOrWhiteSpace(p.Value))
                    .Aggregate("", (x, y) => x += y.Key + "=" + y.Value + "&").TrimEnd('&');

                IsVlidate = Utils.RSACrypto.VerifyData(Utils.RSACrypto.FromXmlKey(this[PublicRSAXml]),
                    Utils.HASHCrypto.CryptoEnum.SHA1,
                    Convert.FromBase64String(query["signMsg"]), Encoding.UTF8.GetBytes(signstr));
            }

            if (IsVlidate)
            {
                result = new PayResult
                {
                    OrderName = query["ext1"],
                    OrderID = query["orderId"],
                    Amount = int.Parse(query["orderAmount"]) / 100,
                    Tax = int.Parse(query["fee"]) / 100,
                    Currency = ECurrency.CNY,
                    Business = query["merchantAcctId"],
                    TxnID = query["dealId"],
                    PaymentName = Name + m_bankId,
                    PaymentDate = DateTime.UtcNow,

                    Message = "<result>1</result><redirecturl></redirecturl>",

                    Customer = new PayResult._Customer
                    {
                        Business = query.ContainsKey("BankSerialNo") ? query["BankSerialNo"] : "",
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

            if (string.IsNullOrEmpty(this[MerchantID])) throw new ArgumentNullException("MerchantID");
            if (string.IsNullOrEmpty(this[PrivateRSAXml])) throw new ArgumentNullException("PrivateRSAXml");
            if (string.IsNullOrEmpty(this[PublicRSAXml])) throw new ArgumentNullException("PublicRSAXml");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            var datas = new Dictionary<string, string>();
            //构造签名参数数组,签名方法要求不可变动前后顺序
            datas.Add("inputCharset", "1");
            datas.Add("pageUrl", p_ReturnUrl);
            datas.Add("bgUrl", p_NotifyUrl);
            datas.Add("version", "v2.0");
            datas.Add("language", "1");
            datas.Add("signType", "4");

            datas.Add("merchantAcctId", this[MerchantID]);
            //sPara.Add("payerIP", p_ClientIP.ToString());

            datas.Add("orderId", p_OrderId);
            datas.Add("orderAmount", (p_Amount * 100).ToString("0"));
            datas.Add("orderTime", DateTime.UtcNow.AddHours(8).ToString("yyyyMMddHHmmss"));
            //sPara.Add("orderTimestamp", DateTime.UtcNow.AddHours(8).ToString("yyyyMMddHHmmss"));
            datas.Add("productName", p_OrderName);
            datas.Add("ext1", p_OrderName);
            datas.Add("payType", m_payType);
            //固定选择值：00、10、12、13、14、17、21、22 00代表显示快钱各支付方式列表（默认开通10、12、 13三种支付方式）； 10代表只显示银行卡支付方式； 10 - 1 代表储蓄卡网银支付；10 - 2 代表信用卡网银 支付 12代表只显示快钱账户支付方式； 13代表只显示线下支付方式； 14代表显示企业网银支付； 17预付卡支付; 21 快捷支付 21 - 1 代表储蓄卡快捷；21 - 2 代表信用卡快捷； 23 分期支付 23 - 2代表信用卡快捷分期支付* 其中”-”只允许在半角状态下输入,无字符集限 制. *企业网银支付、信用卡无卡支付 / 快捷支付、手机语 音支付、预付卡支付、分期支付需单独申请，默认不 开通。
            if (m_payType == "10" || m_payType == "10-1" || m_payType == "10-2")
            {
                datas.Add("bankId", m_bankId);
            }

            //签名顺序严格要求一致,值非空的才进行签名
            var signstr = datas
                .Where(p => !string.IsNullOrWhiteSpace(p.Value) && p.Key != "payerIP" && p.Key != "orderTimestamp" &&
                            p.Key != "bankId").Aggregate("", (x, y) => x += y.Key + "=" + y.Value + "&").TrimEnd('&');

            //var xml = Utils.RSACrypto.Cert2Provider(RSACrypto.CertFile(AppDomain.CurrentDomain.BaseDirectory + "app_data/kuaiqian99bill/tester-rsa.pfx", "123456"), true).ToXmlKey(true);

            var signMsgBytes = Utils.RSACrypto.SignData(Utils.RSACrypto.FromXmlKey(this[PrivateRSAXml]),
                Utils.HASHCrypto.CryptoEnum.SHA1,
                Encoding.UTF8.GetBytes(signstr));

            datas.Add("signMsg", Convert.ToBase64String(signMsgBytes));

            // var formhtml =
            //     new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
            //                       "' action='" +
            //                       GATEWAY + "' method='post' >");
            // foreach (var temp in sPara)
            //     formhtml.Append("<input type='hidden' name='" + temp.Key + "' value='" + temp.Value + "'/>");
            // //formhtml.Append("<input type='hidden' name='SignMsg' value='" + Utils.HASHCrypto.Generate( Utils.HASHCrypto.CryptoEnum.MD5).Encrypt(sPara.Aggregate("", (c, p) => c + p.Value) + this[Key]) + "'/>");
            // formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
            // formhtml.Append("</form>");

            return new PayTicket()
            {
                PayType = PayChannnel.ePayType,
                Action =  EAction.UrlPost,
                Uri = GATEWAY,
                Datas = datas
            };
        }
    }
}