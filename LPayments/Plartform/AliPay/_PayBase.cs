using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace LPayments.Plartform.AliPay
{
    //PC支付
    //https://docs.open.alipay.com/270/105900/
    //https://docs.open.alipay.com/270/alipay.trade.page.pay
    //异步通知
    //https://docs.open.alipay.com/270/105902/

    //手机网站支付
    //https://docs.open.alipay.com/api_1/alipay.trade.wap.pay
    //异步通知
    //https://docs.open.alipay.com/203/105286/

    //单笔转账到支付宝账户
    //https://docs.open.alipay.com/api_28/alipay.fund.trans.toaccount.transfer

    //pc支付
    //https://docs.open.alipay.com/270/105900/
    //https://docs.open.alipay.com/270/alipay.trade.page.pay

    public abstract class _PayBase : _AliPay, IPay
    {
        protected string m_trade_type = "";
        protected string m_product_code = "";

        public _PayBase() : base()
        {
        }

        public _PayBase(string p_SettingsJson) : base(p_SettingsJson)
        {
        }


        public virtual PayResult Notify(IDictionary<string, string> form, IDictionary<string, string> query,
            IDictionary<string, string> head, string body, string notifyip)
        {
            PayResult result = new PayResult
            {
                Status = PayResult.EStatus.Pending,
                Message = "success"
            };
            var IsVlidate = false;
            if (form["app_id"] == this[APPID] &&
                (form["trade_status"] == "TRADE_SUCCESS" || form["trade_status"] == "TRADE_FINISHED"))
            {
                var signstr =
                    form.Where(p => p.Key != "sign" && p.Key != "sign_type").OrderBy(p => p.Key)
                        .Aggregate("", (x, y) => x + y.Key + "=" + y.Value + "&").TrimEnd('&');

                var encoding = Encoding.GetEncoding(form["charset"]);

                IsVlidate = Utils.RSACrypto.VerifyData(m_AlipayPublicProvider, Utils.HASHCrypto.CryptoEnum.SHA256,
                    Convert.FromBase64String(form["sign"]), encoding.GetBytes(signstr));
            }

            if (IsVlidate)
            {
                result = new PayResult
                {
                    OrderName = form["subject"],
                    OrderID = form["out_trade_no"],
                    Amount = double.Parse(form["total_amount"]),
                    Tax = -1,
                    Currency = ECurrency.CNY,
                    Business = form.ContainsKey("seller_id") ? form["seller_id"] : "",
                    TxnID = form["trade_no"],
                    PaymentName = Name + (form.ContainsKey("out_channel_inst") ? form["out_channel_inst"] : ""),
                    PaymentDate = DateTime.UtcNow,
                    Info = form.ContainsKey("body") ? form["body"] : "",
                    //Extend = extend,

                    Message = "success",

                    Customer = new PayResult._Customer
                    {
                        Business = form.ContainsKey("buyer_id") ? form["buyer_id"] : "",
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

            if (string.IsNullOrEmpty(this[APPID])) throw new ArgumentNullException(APPID);
            if (string.IsNullOrEmpty(this[APPPRIVATEKEY])) throw new ArgumentNullException(APPPRIVATEKEY);
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("p_Currency is not allowed!");
            if (p_OrderName.Length > 256) throw new ArgumentException("OrderName must less than 256!");

            dynamic biz = new Dictionary<String, Object>();
            if (!string.IsNullOrWhiteSpace(m_product_code))
                biz["product_code"] = m_product_code;
            biz["out_trade_no"] = p_OrderId;
            biz["subject"] = p_OrderName;
            //金额必须保留两位有效数字
            biz["total_amount"] = p_Amount.ToString("0.00");

            //biz["settle_info"] ="分润信息";

            var dic = PublicDic(m_trade_type, p_NotifyUrl, p_ReturnUrl);

            dic["biz_content"] = Utils.Json.Serialize(biz);
            dic["sign"] = Convert.ToBase64String(Utils.RSACrypto.SignData(m_AppPrivateProvider,
                Utils.HASHCrypto.CryptoEnum.SHA256,
                Encoding.GetEncoding(Charset).GetBytes(Utils.Core.LinkStr(dic))));


            if (m_trade_type == "alipay.trade.precreate")
            {
                var res = _HWU.Response(new Uri(GateWay + "?charset=utf-8"),
                    Utils.HttpWebUtility.HttpMethod.Post, dic);

                var json = Utils.Json.Deserialize<dynamic>(res);

                var pt = new PayTicket();
                if (res.Contains("\"sign\":") && res.Contains("\"alipay_trade_precreate_response\":"))
                {
                    var resdic = new Dictionary<string, string>();
                    resdic["code"] = json.alipay_trade_precreate_response.code.ToString();
                    resdic["msg"] = json.alipay_trade_precreate_response.msg.ToString();
                    resdic["out_trade_no"] = json.alipay_trade_precreate_response.out_trade_no.ToString();
                    resdic["qr_code"] = json.alipay_trade_precreate_response.qr_code.ToString();
                    if (resdic["code"] == "10000" && p_OrderId == resdic["out_trade_no"])
                    {
                        pt.Url = resdic["qr_code"];
                        //var imgbase64 = Core.QR(pt.Url, Core.APIconBase64);
                        //pt.FormHtml = Core.FormQR(imgbase64, p_OrderId, p_Amount, p_OrderName);
                        //pt.Extra = imgbase64;
                    }

                    string sign = json.sign.ToString();
                }

                return pt;
            }
            else if (m_trade_type == "alipay.trade.app.pay")
            {
                var pt = new PayTicket();
                pt.Extra = Utils.Core.LinkStr(dic.Where(p => (p.Key != "method" && p.Key != "version"))
                        .ToDictionary(p => p.Key, p => p.Value),
                    true, true);
                return pt;
            }
            else if (m_trade_type == "alipay.trade.wap.pay" && extend_params != null)
            {
                var pt = new PayTicket();

                Pay_Wap.Extend extend = Utils.Json.Deserialize<Pay_Wap.Extend>(Utils.Json.Serialize(extend_params));
                if (extend != null)
                {
                    string res = _HWU.Response(new Uri(GateWay + "?charset=utf-8"),
                        Utils.HttpWebUtility.HttpMethod.Post, dic);
                    var match = System.Text.RegularExpressions.Regex.Match(res,
                        @"\{""requestType"":""SafePay"",""fromAppUrlScheme"":""alipays"",""dataString"":""[""=&\w\\]+""\}");
                    if (match.Success)
                    {
                        if (string.Equals(extend.Method, "IOS", StringComparison.OrdinalIgnoreCase))
                        {
                            pt.Url = "alipay://alipayclient/?" + Utils.HttpWebUtility.UriDataEncode(match.Value);
                        }
                        //else if (string.Equals(extend.Method, "Android", StringComparison.OrdinalIgnoreCase))
                        //{
                        //    var data = Utils.Json.Deserialize<dynamic>(match.Value);
                        //    pt.Url = string.Format("alipays://platformapi/startApp?appId=20000125&orderSuffix={0}#Intent;scheme=alipays;package=com.eg.android.AlipayGphone;end", Core.UriDataEncode(data.dataString.ToString()));
                        //}
                        else
                        {
                            var data = Utils.Json.Deserialize<dynamic>(match.Value);
                            pt.Url = string.Format(
                                "alipays://platformapi/startApp?appId=20000125&orderSuffix={0}#Intent;scheme=alipays;package=com.eg.android.AlipayGphone;end",
                                Utils.HttpWebUtility.UriDataEncode(data.dataString.ToString()));
                        }
                    }
                }

                return pt;
            }
            else
            {
                StringBuilder formhtml = new StringBuilder();
                formhtml.Append("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" + "' action='" +
                                GateWay + "?charset=" + Charset + "' method='POST'>");
                foreach (KeyValuePair<string, string> temp in dic)
                {
                    formhtml.AppendFormat("<input type='hidden' name='{0}' value='{1}'/>", temp.Key, temp.Value);
                }

                formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
                formhtml.Append("</form>");

                var pt = new PayTicket();
                pt.FormHtml = formhtml.ToString();
                return pt;
            }
        }

        public class PayExtend
        {
            public PayExtend()
            {
                Royaltys = new List<RoyaltyDetail>();
            }

            /// <summary>
            /// 分润列表
            /// </summary>
            public List<RoyaltyDetail> Royaltys { get; set; }

            public class RoyaltyDetail
            {
                /// <summary>
                ///     平台名称
                /// </summary>
                public string AppName { get; set; }

                /// <summary>
                ///     收款账号或者2088开头的16位数字ID
                /// </summary>
                public string Account { get; set; }

                /// <summary>
                ///     收款金额(必须大于等于0.1)
                /// </summary>
                public double Amount { get; set; }

                /// <summary>
                ///     收款方姓名
                /// </summary>
                public string Name { get; set; }

                /// <summary>
                ///     分润说明
                /// </summary>
                public string Remark { get; set; }

                /// <summary>
                /// 交易id（分润成功时返回）
                /// </summary>
                public string TransactionId { get; set; }
            }
        }
    }
}