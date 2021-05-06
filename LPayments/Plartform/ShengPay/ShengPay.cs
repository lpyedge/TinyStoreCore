using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace LPayments.Plartform.ShengPay
{
   
    [PayChannel(EChannel.ChinaBanks)]
    public class ShengPay : IPayChannel, IPay
    {
        public const string PID = "PID";
        public const string Key = "Key";

        private const string GATEWAY = "https://mas.sdo.com/web-acquire-channel/cashier.htm";

        protected string m_PayChannel = "";
        protected string m_InstCode = "";

        public ShengPay() : base()
        {
            Platform = EPlatform.ShengPay;
        }

        public ShengPay(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = PID, Description = "盛付通的商户号", Regex = @"^[\w\.@]+$", Requied = true},
                new Setting {Name = Key, Description = "盛付通的安全密钥(Key)", Regex = @"^\w+$", Requied = true}
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
            if (form["Name"] != null)
                vstr.Append(form["Name"]);
            if (form["Version"] != null)
                vstr.Append(form["Version"]);
            if (form["Charset"] != null)
                vstr.Append(form["Charset"]);
            if (form["TraceNo"] != null)
                vstr.Append(form["TraceNo"]);
            if (form["MsgSender"] != null)
                vstr.Append(form["MsgSender"]);
            if (form["SendTime"] != null)
                vstr.Append(form["SendTime"]);
            if (form["InstCode"] != null)
                vstr.Append(form["InstCode"]);
            if (form["OrderNo"] != null)
                vstr.Append(form["OrderNo"]);
            if (form["OrderAmount"] != null)
                vstr.Append(form["OrderAmount"]);
            if (form["TransNo"] != null)
                vstr.Append(form["TransNo"]);
            if (form["TransAmount"] != null)
                vstr.Append(form["TransAmount"]);
            if (form["TransStatus"] != null)
                vstr.Append(form["TransStatus"]);
            if (form["TransType"] != null)
                vstr.Append(form["TransType"]);
            if (form["TransTime"] != null)
                vstr.Append(form["TransTime"]);
            if (form["MerchantNo"] != null)
                vstr.Append(form["MerchantNo"]);
            if (form["ErrorCode"] != null)
                vstr.Append(form["ErrorCode"]);
            if (form["ErrorMsg"] != null)
                vstr.Append(form["ErrorMsg"]);
            if (form["Ext1"] != null)
                vstr.Append(form["Ext1"]);
            if (form["SignType"] != null)
                vstr.Append(form["SignType"]);

            var IsVlidate = false;
            if (form["MerchantNo"] == this[PID] && form["TransStatus"] == "01")
            {
                vstr.Append(this[Key]);
                IsVlidate = string.Equals(Utils.Core.MD5(vstr.ToString()), form["SignMsg"],
                    StringComparison.OrdinalIgnoreCase);
                ;
            }

            if (IsVlidate)
            {
                result = new PayResult
                {
                    OrderName = form["Ext1"],
                    OrderID = form["OrderNo"],
                    Amount = double.Parse(form["OrderAmount"]),
                    Tax = -1,
                    Currency = ECurrency.CNY,
                    Business = form["MsgSender"],
                    TxnID = form["TransNo"],
                    PaymentName = Name + (form["InstCode"] ?? string.Empty),
                    PaymentDate = DateTime.UtcNow,

                    Message = "OK",

                    Customer = new PayResult._Customer
                    {
                        Business = form.ContainsKey("BankSerialNo") ? form["BankSerialNo"] : "",
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

            var datas = new Dictionary<string, string>();
            //构造签名参数数组,签名方法要求不可变动前后顺序
            datas.Add("Name", "B2CPayment");
            datas.Add("Version", "V4.1.1.1.1");
            datas.Add("Charset", "UTF-8");
            datas.Add("MsgSender", this[PID]);
            var timestamp = TimeStamp();
            if (!string.IsNullOrWhiteSpace(timestamp) && timestamp.Contains("SUCCESS") &&
                timestamp.Contains("timestamp"))
                datas.Add("SendTime", Utils.Json.Deserialize<dynamic>(timestamp).timestamp);
            datas.Add("OrderNo", p_OrderId);
            datas.Add("OrderAmount", p_Amount.ToString("0.##"));
            datas.Add("OrderTime", DateTime.UtcNow.AddHours(8).ToString("yyyyMMddHHmmss"));

            if (!string.IsNullOrWhiteSpace(m_PayChannel) && !string.IsNullOrWhiteSpace(m_InstCode))
            {
                datas.Add("PayType", "PT001");
                datas.Add("PayChannel", m_PayChannel);
                datas.Add("InstCode", m_InstCode);
            }

            datas.Add("PageUrl", p_ReturnUrl);
            datas.Add("BackUrl", p_ReturnUrl);
            datas.Add("NotifyUrl", p_NotifyUrl);
            datas.Add("ProductName", p_OrderName);
            datas.Add("BuyerIp", p_ClientIP.ToString());
            datas.Add("Ext1", p_OrderName);
            datas.Add("SignType", "MD5");
            
            var sign = Utils.Core.MD5(datas.Aggregate("", (c, p) => c + p.Value) + this[Key]);
            datas.Add("SignMsg", sign);

            // var formhtml =
            //     new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
            //                       "' action='" +
            //                       GATEWAY + "?_input_charset=utf-8' method='post' >");
            // foreach (var temp in datas)
            //     formhtml.Append("<input type='hidden' name='" + temp.Key + "' value='" + temp.Value + "'/>");
            // formhtml.Append("<input type='hidden' name='SignMsg' value='" +
            //                 Utils.Core.MD5(datas.Aggregate("", (c, p) => c + p.Value) + this[Key]) + "'/>");
            // formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
            // formhtml.Append("</form>");

            return new PayTicket()
            {
                Action = EAction.UrlPost,
                Uri = GATEWAY + "?_input_charset=utf-8",
                Datas = datas
            };
        }

        private string TimeStamp()
        {
            var resstr = "";
            try
            {
                var myRequest =
                    (HttpWebRequest)
                    WebRequest.Create(string.Format("http://api.shengpay.com/mas/v1/timestamp?merchantNo={0}",
                        this[PID]));
                myRequest.Method = "GET";
                using (var myResponse = (HttpWebResponse) myRequest.GetResponse())
                {
                    using (var resStream = myResponse.GetResponseStream())
                    {
                        using (var sr = new StreamReader(resStream, Encoding.UTF8))
                        {
                            resstr = sr.ReadToEnd();
                            sr.Close();
                        }
                    }
                }

                myRequest.Abort();
            }
            catch
            {
            }

            return resstr;
        }
    }
}