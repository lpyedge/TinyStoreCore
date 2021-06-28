using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;

namespace LPayments.Plartform.UnionPay
{
    //https://open.unionpay.com/ajweb/help/file/toDetailPage?id=496&flag=1
    //http://blog.csdn.net/yulei_qq/article/details/49002281

    //银联入网测试环境地址：
    //前台交易请求地址
    //https://101.231.204.80:5000/gateway/api/frontTransReq.do
    //后台交易请求地址（无卡）
    //https://101.231.204.80:5000/gateway/api/backTransReq.do
    //后台交易请求地址（有卡）
    //https://101.231.204.80:5000/gateway/api/cardTransReq.do
    //单笔查询请求地址
    //https://101.231.204.80:5000/gateway/api/queryTrans.do
    //批量交易请求地址
    //https://101.231.204.80:5000/gateway/api/batchTrans.do
    //文件传输类交易地址
    //https://101.231.204.80:9080/
    //APP交易请求地址
    //https://101.231.204.80:5000/gateway/api/appTransReq.do

    //Q：银联全渠道系统商户接入生产环境地址？
    //A : 可参见11章节生产环境参数配置，配置文件可在开发包中找到（开发包获取地址：https://open.unionpay.com/ajweb/help/file/）。将配置文件中测试地址修改为如下地址：
    //银联入网测试环境地址：
    //前台交易请求地址
    //https://gateway.95516.com/gateway/api/frontTransReq.do
    //后台交易请求地址（无卡）
    //https://gateway.95516.com/gateway/api/backTransReq.do
    //后台交易请求地址（有卡）
    //https://gateway.95516.com/gateway/api/cardTransReq.do
    //单笔查询请求地址
    //https://gateway.95516.com/gateway/api/queryTrans.do
    //批量交易请求地址
    //https://gateway.95516.com/gateway/api/batchTrans.do
    //文件传输类交易地址
    //https://filedownload.95516.com/
    //APP交易请求地址
    //https://gateway.95516.com/gateway/api/appTransReq.do

    
    [PayChannel(EChannel.ChinaBanks)]
    public class UnionPay : IPayChannel, IPay
    {
        public const string MerId = "MerId";
        public const string PublicCert = "PublicCert";
        public const string PrivateCert = "PrivateCert";
        public const string PrivateCertPwd = "PrivateCertPwd";

#if DEBUG
        private const string GATEWAY = "https://101.231.204.80:5000/gateway/api/frontTransReq.do";
#else
        const string GATEWAY = "https://gateway.95516.com/gateway/api/frontTransReq.do";
#endif

        public UnionPay() : base()
        {
            Platform = EPlatform.UnionPay;
        }

        public UnionPay(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = MerId, Description = "银联支付的商户号", Regex = @"^[\w\.@]+$", Requied = true},
                new Setting
                {
                    Name = PublicCert, Description = "银联支付的公钥Base64格式", Regex = @"^[\w\.\\:/+=-]+$$", Requied = true
                },
                new Setting
                {
                    Name = PrivateCert, Description = "银联支付的私钥Base64格式", Regex = @"^[\w\.\\:/+=-]+$", Requied = true
                },
                new Setting {Name = PrivateCertPwd, Description = "银联支付的私钥密码", Regex = @"^\w+$", Requied = true}
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

            var signValue = form["signature"];

            var publiccert = Utils.RSACrypto.CertByte(Convert.FromBase64String(this[PublicCert]));

            var signByte = Convert.FromBase64String(signValue);
            form.Remove("signature");

            var stringSignDigest = Utils.Core.SHA1(Utils.Core.LinkStr(form));

            var IsVlidate = false;

            if (form["certId"] == Utils.RSACrypto.CertId(publiccert))
                IsVlidate = Utils.RSACrypto.VerifyData(Utils.RSACrypto.Cert2Provider(publiccert),
                    Utils.HASHCrypto.CryptoEnum.SHA1, signByte,
                    Encoding.UTF8.GetBytes(stringSignDigest));
            if (IsVlidate && (form["respCode"] == "00" || form["respCode"] == "A6"))
            {
                result = new PayResult
                {
                    OrderName = "",
                    OrderID = form["orderId"],
                    Amount = double.Parse(form["settleAmt"]) / 100,
                    Tax = -1,
                    Currency = ECurrency.CNY,
                    Business = form["merId"],
                    TxnID = form["queryId"],
                    PaymentName = Name + (form["txnType"] ?? string.Empty),
                    PaymentDate = DateTime.Parse(form["txnTime"]),

                    Message = "OK",

                    Customer = new PayResult._Customer
                    {
                        Business = form["accNo"],
                    }
                };
            }

            return result;
        }

        public virtual PayTicket Pay(string p_OrderId, double p_Amount,
            ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
        {
            if (string.IsNullOrWhiteSpace(p_OrderId)) throw new ArgumentNullException("p_OrderId");

            if (string.IsNullOrEmpty(this[MerId])) throw new ArgumentNullException("MerId");
            if (string.IsNullOrEmpty(this[PublicCert])) throw new ArgumentNullException("PublicCert");
            if (string.IsNullOrEmpty(this[PrivateCert])) throw new ArgumentNullException("PrivateCert");
            if (string.IsNullOrEmpty(this[PrivateCertPwd])) throw new ArgumentNullException("PrivateCertPwd");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            var privatecert =
                Utils.RSACrypto.CertByte(Convert.FromBase64String(this[PrivateCert]), this[PrivateCertPwd]);

            var datas = new Dictionary<string, string>();
            //以下信息非特殊情况不需要改动
            datas["version"] = "5.0.0"; //版本号
            datas["encoding"] = "UTF-8"; //编码方式
            datas["txnType"] = "01"; //交易类型
            datas["txnSubType"] = "01"; //交易子类
            datas["bizType"] = "000201"; //业务类型
            datas["signMethod"] = "01"; //签名方法
            datas["channelType"] = "08"; //渠道类型
            datas["accessType"] = "0"; //接入类型
            datas["frontUrl"] = p_ReturnUrl; //前台通知地址

            datas["backUrl"] = p_NotifyUrl; //后台通知地址
            datas["currencyCode"] = "156"; //交易币种

            datas["merId"] = this[MerId]; //商户号，请改自己的测试商户号，此处默认取demo演示页面传递的参数
            datas["orderId"] = p_OrderId; //商户订单号，8-32位数字字母，不能含“-”或“_”，此处默认取demo演示页面传递的参数，可以自行定制规则
            datas["txnTime"] =
                DateTime.UtcNow.AddHours(8)
                    .ToString("yyyyMMddHHmmss"); //订单发送时间，格式为YYYYMMDDhhmmss，取北京时间，此处默认取demo演示页面传递的参数，参考取法： .ToString("yyyyMMddHHmmss")
            datas["txnAmt"] = (p_Amount * 100).ToString("0"); //交易金额，单位分，此处默认取demo演示页面传递的参数

            //证书id
            datas["certId"] = Utils.RSACrypto.CertId(privatecert);

            //生成欲签名字符串
            var stringSignDigest =
                Utils.Core.SHA1(Utils.Core.LinkStr(datas));

            //得到签名内容
            var byteSign = Utils.RSACrypto.SignData(Utils.RSACrypto.Cert2Provider(privatecert, true),
                Utils.HASHCrypto.CryptoEnum.SHA1,
                Encoding.UTF8.GetBytes(
                    stringSignDigest)); //m_PrivateProvider.SignData(Utils.HASHCrypto.CryptoEnum.SHA1, Encoding.UTF8.GetBytes(stringSignDigest));

            //设置签名域值
            datas["signature"] = Convert.ToBase64String(byteSign);

            // var formhtml =
            //     new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
            //                       "' action='" +
            //                       GATEWAY + "' method='post' >");
            // foreach (var temp in datas)
            //     formhtml.Append("<input type='hidden' id='" + temp.Key + "' name='" + temp.Key + "' value='" +
            //                     temp.Value + "'/>");
            // formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
            // formhtml.Append("</form>");

            return new PayTicket()
            {
                PayType = PayChannnel.ePayType,
                Action = EAction.UrlPost,
                Uri = GATEWAY,
                Datas = datas
            };
        }
    }
}