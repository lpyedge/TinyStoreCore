using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;

namespace LPayments.Plartform.iJuHePay
{
    public abstract class _PayBase : _iJuHePay
    {
        protected string m_ChannelId = "";

        public _PayBase() : base()
        {
        }

        public _PayBase(string p_SettingsJson) : base(p_SettingsJson)
        {
        }

        public PayResult Notify(IDictionary<string, string> form, IDictionary<string, string> query,
            IDictionary<string, string> head, string body, string notifyip)
        {
            PayResult result = new PayResult
            {
                Status = PayResult.EStatus.Pending,
                Message = "fail"
            };

            var signature =
                Utils.Core.MD5(query["P_UserId"] + "|" + query["P_OrderId"] + "|" + query["P_CardId"] + "|" +
                               query["P_CardPass"] + "|" +
                               query["P_FaceValue"] + "|" + query["P_ChannelId"] + "|" + query["P_OrderId_out"] + "|" +
                               this[SecretKey]);

            var IsVlidate = !string.IsNullOrWhiteSpace(signature) && !string.IsNullOrWhiteSpace(query["P_PostKey"]) &&
                            string.Equals(signature, query["P_PostKey"], StringComparison.OrdinalIgnoreCase);

            // query["P_FaceType"] 不返回货币名称
            if (IsVlidate && query.ContainsKey("P_ErrCode") && query["P_ErrCode"] == "0")
            {
                result = new PayResult
                {
                    OrderName = query["P_Subject"],
                    OrderID = query["P_OrderId"],
                    Amount = double.Parse(query["P_FaceValue"]),
                    Tax = -1,
                    Currency = ECurrency.CNY,
                    Business = this[MerchantId],
                    TxnID = query["P_OrderId_out"],
                    PaymentName = Name,
                    PaymentDate = DateTime.UtcNow,

                    Message = "P_ErrCode=0",
                };
            }

            return result;
        }

        public virtual PayTicket Pay(string p_OrderId, double p_Amount,
            ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
        {
            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");

            if (string.IsNullOrEmpty(this[MerchantId])) throw new ArgumentNullException("MerchantId");
            if (string.IsNullOrEmpty(this[SecretKey])) throw new ArgumentNullException("SecretKey");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            var dic = new Dictionary<string, string>();
            dic["P_UserId"] = this[MerchantId];
            dic["P_OrderId"] = p_OrderId;
            //dic["P_CardId"] = "";
            //dic["P_CardPass"] = "";
            dic["P_FaceValue"] = p_Amount.ToString("0.00");
            dic["P_FaceType"] = "CNY";
            dic["P_ChannelId"] = m_ChannelId;
            dic["P_PostKey"] = Utils.Core
                .MD5(dic["P_UserId"] + "|" + dic["P_OrderId"] + "|" + "|" + "|" + dic["P_FaceValue"] + "|" +
                     dic["P_FaceType"] + "|" + dic["P_ChannelId"] + "|" + this[SecretKey])
                .ToLowerInvariant(); //签名字符串,上面7个参数进行签名操作

            dic["P_Subject"] = p_OrderName;
            dic["P_Price"] = p_Amount.ToString("0.00");
            dic["P_Quantity"] = "1";

            dic["P_Result_URL"] = p_NotifyUrl;
            dic["P_Notify_URL"] = p_ReturnUrl;

            dic["P_APPID"] = this[PayUrl];

            var formhtml =
                new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
                                  "' action='https://ijuhepay.cn/GateWay/ReceiveOrder.aspx' method='post' >");
            foreach (var item in dic)
            {
                formhtml.AppendFormat("<input type='hidden' name='{0}' value='{1}' />", item.Key, item.Value);
            }

            formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
            formhtml.Append("</form>");

            var pt = new PayTicket();
            pt.FormHtml = formhtml.ToString();
            return pt;
        }
    }
}