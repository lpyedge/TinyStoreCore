//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Net;
//using System.Text;
//using System.Web;
//using System.Xml;
//using Payments.Plartform.AliPay;

//namespace Payments.Plartform.IPS
//{
//    //测试商户号	7551000001	商户调用接口时可用此测试商户号对应参数 mch_id
//    //测试密钥	9d101c97133837e13dde2d32a5054abb 为保证通讯不被篡改，九霄支付与商户之间约定的32 位或 24 位字符串，算签名 sign 时使用
//    //金额  1000	金额，默认为 RMB，以分为单位。1000 表示RMB10.00

//    [Channel(EPlartform.IPS, EChannel.Banks, EPayType.PC)]
//    public class IPS : IPayment
//    {
//        public const string MerchantId = "MerchantId";
//        public const string Account = "Account";
//        public const string SecretKey = "SecretKey";

//        protected bool m_qrcode = true;
//        protected string m_service = "";

//        public IPS()
//        {
//            Init();
//        }

//        public IPS(string p_SettingsJson)
//        {
//            Init();
//            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
//        }

//        protected override void Init()
//        {
//            Settings = new List<Setting>
//            {
//                new Setting {Name = MerchantId, Description = "环迅支付的商户号", Regex = @"^\w+$", Requied = true},
//                new Setting {Name = Account, Description = "环迅支付的账户号(交易账号)", Regex = @"^\w+$", Requied = true},
//                new Setting {Name = SecretKey, Description = "环迅支付的商户密钥", Regex = @"^\w+$", Requied = true}
//            };

//            Currencies = new List<Currency>
//            {
//                Currency.CNY,
//            };
//        }

//        public override NotifyResult Notify()
//        {
//            var res = base.Notify();
//            HttpContext.Current.Response.Write("success");
//            return res;
//        }

//        public override NotifyResult Notify(IDictionary<string, string> form, IDictionary<string, string> query,
//            IDictionary<string, string> head, string body,string notifyip)
//        {
//            var xd = new XmlDocument();
//            xd.LoadXml(body);
//            var tempdic = new Dictionary<string, string>();
//            foreach (XmlNode node in xd.ChildNodes[0].ChildNodes)
//            {
//                tempdic[node.Name] = node.InnerText;
//            }

//            if (body.Contains("status") && tempdic["status"] == "0"
//                && tempdic["result_code"] == "0" && tempdic["mch_id"] == this[MerchantId])
//            {
//                var signstr = tempdic.Where(p => p.Key != "sign").OrderBy(p => p.Key)
//                                  .Aggregate("", (x, y) => x + y.Key + "=" + y.Value + "&") + $"key={this[SecretKey]}";

//                if (string.Equals(tempdic["sign"], Utils.HASHCrypto.Generate( Utils.HASHCrypto.CryptoEnum.MD5).Encrypt(signstr), StringComparison.OrdinalIgnoreCase)
//                    && tempdic["pay_result"] == "0")
//                {
//                    var Result = new NotifyResult
//                    {
//                        OrderName = "",
//                        OrderID = tempdic["out_trade_no"],
//                        Amount = tempdic["total_fee"].Parse<double>() / 100,
//                        Tax = -1,
//                        Currency = Currency.CNY,
//                        Business = this[MerchantId],
//                        TxnID = tempdic["transaction_id"],
//                        PaymentName = Name,
//                        PaymentDate = DateTime.UtcNow,

//                        Message = "success",

//                        Customer_Business = tempdic["openid"],
//                    };
//                    return Result;
//                }
//            }
//            return null;
//        }

//        public override PayTicket Pay(string p_OrderId, double p_Amount,
//            Currency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
//            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
//        {
//            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");

//            if (string.IsNullOrEmpty(this[MerchantId])) throw new ArgumentNullException("ClientID");
//            if (string.IsNullOrEmpty(this[SecretKey])) throw new ArgumentNullException("Secret");
//            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

//            var uri
//#if DEBUG
//                = new Uri("https://newpay.ips.com.cn/psfp-entry/gateway/payment.do");
//#else
//                = new Uri("https://newpay.ips.com.cn/psfp-entry/gateway/payment.do");
//#endif
//            //m_service = "weixin.scan";
//            var headdic = new Dictionary<string, string>
//            {
//                ["MerCode"] = this[MerchantId], //"alipay.scan",
//                ["Account"] = this[Account],
//                ["MsgId"] = p_OrderId,
//                ["ReqDate"] = DateTime.Now.ToString("yyyyMMddHHmmss"),
//            };

//            var bodydic = new Dictionary<string, string>();
//            bodydic["MerBillNo"] = p_OrderId;
//            //01#借记卡 02#信用卡 03#IPS账户支付
//            bodydic["GatewayType"] = "01";
//            bodydic["Date"] = DateTime.Now.ToString("yyyyMMdd");
//            bodydic["CurrencyType"] = "156";
//            bodydic["Amount"] = p_Amount.ToString("0.00");

//            bodydic["Merchanturl"] = p_ReturnUrl;
//            bodydic["FailUrl"] = p_CancelUrl;
//            bodydic["OrderEncodeType"] = "5";
//            bodydic["RetEncodeType"] = "17";
//            bodydic["RetType"] = "1";
//            bodydic["ServerUrl"] = p_NotifyUrl;
//            bodydic["GoodsName"] = p_OrderName;

//            //决定商户是否参用直连方式 1 #直连 如不用直连方式，此参数不用传值
//            bodydic["IsCredit"] = "1";
//            //直连必填，银行编号
//            bodydic["BankCode"] = "1100";
//            //直连必填 1#个人网银  2#企业网银
//            bodydic["ProductType"] = "1";

//            var bodycontent = "";
//            foreach (var item in bodydic)
//            {
//                bodycontent += $"<{item.Key}>{item.Value}</{item.Key}>";
//            }
//            var body = $"<body>{bodycontent}</body>";

//            var signstr = $"{body}{this[MerchantId]}{this[SecretKey]}";

//            headdic["Signature"] = Utils.HASHCrypto.Generate( Utils.HASHCrypto.CryptoEnum.MD5).Encrypt(signstr).ToLowerInvariant();

//            var headcontent = "";
//            foreach (var item in headdic)
//            {
//                headcontent += $"<{item.Key}>{item.Value}</{item.Key}>";
//            }
//            var head = $"<head>{headcontent}</head>";
//            var xml = $"<Ips><GateWayReq>{head}{body}</GateWayReq></Ips>";

//            StringBuilder formhtml = new StringBuilder();
//            formhtml.Append("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" + "' action='" + uri.ToString() + "' method='POST'>");
//            formhtml.AppendFormat("<input type='hidden' name='{0}' value='{1}'/>", "pGateWayReq", xml);
//            formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
//            formhtml.Append("</form>");

//            var pt = new PayTicket();
//            pt.FormHtml = formhtml.ToString();
//            return pt;
//        }
//    }
//}