using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;

namespace Payments.Plartform.PaypalExpress
{
    [PayChannel(EChannel.PaypalExpress, PayType = EPayType.PC)]
    public class PaypalBase_Express : _PaypalExpress, IPay
    {
        public PaypalBase_Express() : base()
        {
        }

        public PaypalBase_Express(string p_SettingsJson) : base(p_SettingsJson)
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

            var encoding = Encoding.UTF8;
            try
            {
                encoding = Encoding.GetEncoding(form["charset"]);
            }
            catch
            {
            }

            var dictionary = Utils.Core.UnLinkStr(body);
            if (dictionary["token"] != null)
            {
                //21 GetExpressCheckoutDetails
                //22 DoExpressCheckoutPayment
                var token = dictionary["token"];
                var payerID = dictionary["PayerID"];
                if (head["Cookie"].Contains($"token={token};")
                ) //Cookie: lang=zh-CN; i_like_gogits=bbe2d5d859715cf7; i_like_gogs=b8d7663d9d8c8432
                {
                    var encoder = new NVPCodec();
                    encoder["METHOD"] = "GetExpressCheckoutDetails";
                    encoder["TOKEN"] = token;

                    var pStrrequestforNvp = encoder.Encode();
                    var pStresponsenvp = HttpCall(pStrrequestforNvp);

                    var decoder = new NVPCodec();
                    decoder.Decode(pStresponsenvp);
                    var strAck = decoder["ACK"];

                    //AccountCheck
                    var accountcheckres = true;
                    // if (this[AccountCheck].Parse<bool>(true))
                    //     accountcheckres = string.Equals(decoder["CUSTOM"], this[Account],
                    //         StringComparison.OrdinalIgnoreCase);

                    if (accountcheckres && strAck != null &&
                        (strAck.ToLower() == "success" || strAck.ToLower() == "successwithwarning"))
                    {
                        result = new PayResult
                        {
                            PaymentName = "", //重要 这种情况下不要设置付款类型，在实际程序使用中判断付款类型为空则进行特殊操作！
                            OrderID = decoder["INVNUM"],
                            OrderName = decoder["DESC"],
                            PaymentDate = DateTime.UtcNow,
                            Business = decoder["CUSTOM"],
                            TxnID = token,

                            Customer = new PayResult._Customer()
                            {
                                Business = payerID,
                                City = decoder.AllKeys.Contains("SHIPTOCITY") ? decoder["SHIPTOCITY"] : "",
                                Country = decoder.AllKeys.Contains("SHIPTOCOUNTRYCODE")
                                    ? decoder["SHIPTOCOUNTRYCODE"]
                                    : "",
                                Email = decoder.AllKeys.Contains("EMAIL")
                                    ? decoder["EMAIL"]
                                    : (decoder.AllKeys.Contains("BUSINESS") ? decoder["BUSINESS"] : ""),
                                Name = decoder.AllKeys.Contains("FIRSTNAME") &&
                                       decoder.AllKeys.Contains("LASTNAME")
                                    ? decoder["FIRSTNAME"] + " " + decoder["LASTNAME"]
                                    : "",
                                Phone = decoder.AllKeys.Contains("PHONENUM") ? decoder["PHONENUM"] : "",
                                State = decoder.AllKeys.Contains("SHIPTOSTATE") ? decoder["SHIPTOSTATE"] : "",
                                Status = decoder.AllKeys.Contains("PAYERSTATUS") &&
                                         decoder["PAYERSTATUS"] == "verified",
                                Street = decoder.AllKeys.Contains("SHIPTOSTREET")
                                    ? decoder["SHIPTOSTREET"]
                                    : "",
                                Zip = decoder.AllKeys.Contains("SHIPTOZIP") ? decoder["SHIPTOZIP"] : "",
                            }
                        };

                        encoder = new NVPCodec();
                        encoder["METHOD"] = "DoExpressCheckoutPayment";
                        encoder["TOKEN"] = token;
                        encoder["PAYERID"] = payerID;
                        encoder["PaymentAction"] = "Sale";
                        encoder["AMT"] = decoder["AMT"];
                        encoder["CURRENCYCODE"] = decoder["CURRENCYCODE"];
                        encoder["DESC"] = decoder["DESC"];
                        encoder["INVNUM"] = decoder["INVNUM"];

                        //todo 异步通知没有NotifyUrl 
                        //encoder["NOTIFYURL"] = Context.NotifyDomain + "/" + Core.Prefix + Name + Core.Suffix;

                        pStrrequestforNvp = encoder.Encode();
                        pStresponsenvp = HttpCall(pStrrequestforNvp);

                        decoder = new NVPCodec();
                        decoder.Decode(pStresponsenvp);

                        strAck = decoder["ACK"];
                        if (decoder["ACK"] != null &&
                            (strAck.ToLower() == "success" || strAck.ToLower() == "successwithwarning") &&
                            string.Equals(decoder["PAYMENTSTATUS"], "Completed", StringComparison.OrdinalIgnoreCase))
                        {
                            result.Amount = double.Parse(decoder["AMT"]);
                            result.Currency = Utils.Core.Parse<ECurrency>(decoder["CURRENCYCODE"]);
                            result.Tax = double.Parse(decoder["FEEAMT"]);
                        }
                        else
                        {
                            result.Status = PayResult.EStatus.Failed;
                            result.Message = "ErrorCode=" + decoder["L_ERRORCODE0"] + "&" +
                                             "Desc=" + decoder["L_SHORTMESSAGE0"] + "&" +
                                             "Desc2=" + decoder["L_LONGMESSAGE0"];
                        }
                    }
                    else
                    {
                        result.Status = PayResult.EStatus.Failed;
                        result.Message = "ErrorCode=" + decoder["L_ERRORCODE0"] + "&" +
                                         "Desc=" + decoder["L_SHORTMESSAGE0"] + "&" +
                                         "Desc2=" + decoder["L_LONGMESSAGE0"];
                    }
                }
            }
            else
            {
                //AccountCheck
                var accountcheckres = true;
                // if (this[AccountCheck].Parse<bool>(true))
                //     accountcheckres =
                //         string.Equals(dictionary["business"], this[Account], StringComparison.OrdinalIgnoreCase) |
                //         string.Equals(dictionary["receiver_email"], this[Account], StringComparison.OrdinalIgnoreCase);

                if (accountcheckres &&
                    string.Equals(ValidateStr(body), "VERIFIED",
                        StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(dictionary["payment_status"], "Completed", StringComparison.OrdinalIgnoreCase))
                {
                    result = new PayResult
                    {
                        OrderName = dictionary["item_name"],
                        OrderID =
                            dictionary.ContainsKey("item_number")
                                ? dictionary["item_number"]
                                : (dictionary.ContainsKey("invoice") ? dictionary["invoice"] : ""),
                        Amount = double.Parse(dictionary["mc_gross"]),
                        Tax = double.Parse(dictionary["mc_fee"]),
                        Currency = Utils.Core.Parse<ECurrency>(dictionary["mc_currency"]),
                        Business =
                            dictionary.ContainsKey("receiver_email")
                                ? dictionary["receiver_email"]
                                : dictionary["business"],
                        TxnID = dictionary["txn_id"],
                        PaymentName = Name,
                        PaymentDate = DateTime.UtcNow,

                        Message = "",

                        Customer = new PayResult._Customer()
                        {
                            Name = (dictionary.ContainsKey("address_name") ? dictionary["address_name"] : ""),
                            Street = (dictionary.ContainsKey("address_street") ? dictionary["address_street"] : ""),
                            City = (dictionary.ContainsKey("address_city") ? dictionary["address_city"] : ""),
                            State = (dictionary.ContainsKey("address_state") ? dictionary["address_state"] : ""),
                            Zip = dictionary.ContainsKey("address_zip") ? dictionary["address_zip"] : "",
                            Country = dictionary.ContainsKey("address_country")
                                ? dictionary["address_country"]
                                : (dictionary.ContainsKey("residence_country") ? dictionary["residence_country"] : ""),
                            Email = dictionary.ContainsKey("payer_email") ? dictionary["payer_email"] : "",
                            Phone = dictionary.ContainsKey("contact_phone") ? dictionary["contact_phone"] : "",
                            Business = dictionary.ContainsKey("payer_id") ? dictionary["payer_id"] : "",
                            Status = dictionary.ContainsKey("payer_status") && string.Equals(dictionary["payer_status"],
                                "verified", StringComparison.OrdinalIgnoreCase)
                        }
                    };
                }
            }

            return result;
        }

        public PayTicket Pay(string p_OrderId, double p_Amount,
            ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
        {
            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");

            if (string.IsNullOrEmpty(this[Account])) throw new ArgumentNullException("Account");
            if (string.IsNullOrEmpty(this[Username])) throw new ArgumentNullException("Username");
            if (string.IsNullOrEmpty(this[Password])) throw new ArgumentNullException("Password");
            if (string.IsNullOrEmpty(this[Signature])) throw new ArgumentNullException("Signature");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            var encoder = new NVPCodec();
            //encoder["METHOD"] = "SetExpressCheckout";
            //encoder["LOCALECODE"] = "en";
            //encoder["RETURNURL"] = p_NotifyUrl;
            //encoder["CANCELURL"] = p_CancelUrl;
            //encoder["AMT"] = p_Amount.ToString("0.##");
            //encoder["CURRENCYCODE"] = p_Currency.ToString();
            //encoder["DESC"] = p_OrderName;
            //encoder["INVNUM"] = p_OrderId;
            //encoder["CUSTOM"] = this[Account];

            encoder["METHOD"] = "SetExpressCheckout";
            encoder["PAYMENTREQUEST_0_AMT"] = p_Amount.ToString("0.##");
            encoder["RETURNURL"] = p_NotifyUrl;
            encoder["CANCELURL"] = p_CancelUrl;
            encoder["NOSHIPPING"] = "1";
            //Determines whether PayPal displays shipping address fields on the PayPal pages.For digital goods, this field is required, and you must set it to 1.Value is:
            //0 — PayPal displays the shipping address on the PayPal pages.
            //1 — PayPal does not display shipping address fields and removes shipping information from the transaction.
            //2 — If you do not pass the shipping address, PayPal obtains it from the buyer's account profile.
            encoder["CALLBACKVERSION"] = "61.0";
            //encoder["LOGOIMG"] = "";
            encoder["SOLUTIONTYPE"] = "Sole";
            encoder["PAYMENTREQUEST_0_PAYMENTREASON"] = "None";
            encoder["PAYMENTREQUEST_0_CURRENCYCODE"] = p_Currency.ToString();


            encoder["PAYMENTREQUEST_0_DESC"] = p_OrderName;
            encoder["PAYMENTREQUEST_0_INVNUM"] = p_OrderId;
            encoder["PAYMENTREQUEST_0_CUSTOM"] = this[Account];
            encoder["PAYMENTREQUEST_0_NOTIFYURL"] = p_NotifyUrl;

            var pStrrequestforNvp = encoder.Encode();
            var pStresponsenvp = HttpCall(pStrrequestforNvp);

            var decoder = new NVPCodec();
            decoder.Decode(pStresponsenvp);

            //tls12 加密
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

            var strAck = decoder["ACK"].ToLower();
            if (strAck != null && (strAck == "success" || strAck == "successwithwarning"))
            {
                var token = decoder["TOKEN"];

// #if DEBUG
//                 var formhtml =
//                     new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
//                                       "' action='" +
//                                       "https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=_express-checkout&useraction=commit&token=" +
//                                       token + "' method='post' >");
// #else
//                 var formhtml =
// new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" + "' action='" + "https://www.paypal.com/cgi-bin/webscr?cmd=_express-checkout&useraction=commit&token=" + token + "' method='post' >");
// #endif
//                 formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
//                 formhtml.Append("</form>");
//                 var pt = new PayTicket();
//                 pt.FormHtml = formhtml.ToString();
//                 pt.Uri = "https://www.paypal.com/cgi-bin/webscr?cmd=_express-checkout&useraction=commit&token=" + token;
//                 //前端通过Js操作Cookies写入 extra的内容 key为 token
//                 //Set-Cookie: redirect_to=; Path=/; Max-Age=0
//                 pt.Token = token;


                //https://www.paypal.com/cgi-bin/webscr?cmd=_express-checkout&useraction=commit&token=1232131231
                //https://www.paypal.com/cgi-bin/webscr?cmd=_express-checkout&token=1232131231
                //加上useraction=commit则用户在paypal的付款界面不展示收货地址和继续按钮，展示点击付款按钮，更适合虚拟产品的支付
                return new PayTicket()
                {
                    Name = this.Name,
                    DataFormat = EPayDataFormat.Url,
                    DataContent = 
                    
#if DEBUG
                        $"https://www.sandbox.paypal.com/cgi-bin/webscr?cmd=_express-checkout&useraction=commit&token={token}",
#else
                        $"https://www.paypal.com/cgi-bin/webscr?cmd=_express-checkout&useraction=commit&token={token}",
#endif
                };
            }

            return new PayTicket()
            {
                Name = this.Name,
                DataFormat = EPayDataFormat.Error,
                Message = "ErrorCode=" + decoder["L_ERRORCODE0"] + "&" + "Desc=" + decoder["L_SHORTMESSAGE0"] + "&" +
                          "Desc2=" + decoder["L_LONGMESSAGE0"]
            };
        }

        public string HttpCall(string NvpRequest) //CallNvpServer
        {
            //tls12 加密
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
#if DEBUG
            var url = "https://api-3t.sandbox.paypal.com/nvp";
#else
            string url = "https://api-3t.paypal.com/nvp";
#endif

            //To Add the credentials from the profile
            var strPost = NvpRequest + "&" + buildCredentialsNVPString();
            strPost = strPost + "&BUTTONSOURCE=" + HttpUtility.UrlEncode("PP-ECWizard");

            var objRequest = (HttpWebRequest) WebRequest.Create(url);
            objRequest.Timeout = 10000;
            objRequest.Method = "POST";
            objRequest.ContentLength = strPost.Length;

            try
            {
                using (var myWriter = new StreamWriter(objRequest.GetRequestStream()))
                {
                    myWriter.Write(strPost);
                }
            }
            catch
            {
                /*
                if (log.IsFatalEnabled)
                {
                    log.Fatal(e.Message, this);
                }
                */
            }

            //Retrieve the Response returned from the NVP API call to PayPal
            var objResponse = (HttpWebResponse) objRequest.GetResponse();
            string result;
            using (var sr = new StreamReader(objResponse.GetResponseStream()))
            {
                result = sr.ReadToEnd();
            }

            //Logging the response of the transaction
            /* if (log.IsInfoEnabled)
             {
                 log.Info("Result :" +
                           " Elapsed Time : " + (DateTime.Now - startDate).Milliseconds + " ms" +
                          result);
             }
             */

            return result;
        }

        private string buildCredentialsNVPString()
        {
            var codec = new NVPCodec();

            if (!string.IsNullOrWhiteSpace(this[Username]))
                codec["USER"] = this[Username];

            if (!string.IsNullOrWhiteSpace(this[Password]))
                codec["PWD"] = this[Password];

            if (!string.IsNullOrWhiteSpace(this[Signature]))
                codec["SIGNATURE"] = this[Signature];

            //if (!string.IsNullOrWhiteSpace(Subject))
            //	codec["SUBJECT"] = Subject;

            codec["VERSION"] = "112.0";

            return codec.Encode();
        }

        private static string ValidateStr(string body)
        {
            //tls12 加密
            System.Net.ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            string validateStr;

            var param = Encoding.ASCII.GetBytes(body + "&cmd=_notify-validate");
            var myRequest = (HttpWebRequest) WebRequest.Create("https://www.paypal.com/cgi-bin/webscr");
            myRequest.Method = "POST";
            myRequest.ContentType = "application/x-www-form-urlencoded";
            using (var reqStream = myRequest.GetRequestStream())
            {
                reqStream.Write(param, 0, param.Length);
            }

            using (var myResponse = (HttpWebResponse) myRequest.GetResponse())
            {
                using (var resStream = myResponse.GetResponseStream())
                {
                    using (var sr = new StreamReader(resStream, Encoding.ASCII))
                    {
                        validateStr = sr.ReadToEnd();
                        sr.Close();
                    }
                }
            }

            myRequest.Abort();
            return validateStr;
        }

        internal sealed class NVPCodec : NameValueCollection
        {
            private const string AMPERSAND = "&";
            private const string EQUALS = "=";
            private static readonly char[] AMPERSAND_CHAR_ARRAY = AMPERSAND.ToCharArray();
            private static readonly char[] EQUALS_CHAR_ARRAY = EQUALS.ToCharArray();

            /// <summary>
            ///     Returns the built NVP string of all name/value pairs in the Hashtable
            /// </summary>
            /// <returns></returns>
            public string Encode()
            {
                var sb = new StringBuilder();
                var firstPair = true;
                foreach (var kv in AllKeys)
                {
                    var name = HttpUtility.UrlEncode(kv);
                    var value = HttpUtility.UrlEncode(this[kv]);
                    if (!firstPair)
                        sb.Append(AMPERSAND);
                    sb.Append(name).Append(EQUALS).Append(value);
                    firstPair = false;
                }

                return sb.ToString();
            }

            /// <summary>
            ///     Decoding the string
            /// </summary>
            /// <param name="nvpstring"></param>
            public void Decode(string nvpstring)
            {
                Clear();
                foreach (var nvp in nvpstring.Split(AMPERSAND_CHAR_ARRAY))
                {
                    var tokens = nvp.Split(EQUALS_CHAR_ARRAY);
                    if (tokens.Length >= 2)
                    {
                        var name = HttpUtility.UrlDecode(tokens[0]);
                        var value = HttpUtility.UrlDecode(tokens[1]);
                        Add(name, value);
                    }
                }
            }

            #region Array methods

            public void Add(string name, string value, int index)
            {
                Add(GetArrayName(index, name), value);
            }

            public void Remove(string arrayName, int index)
            {
                Remove(GetArrayName(index, arrayName));
            }

            /// <summary>
            /// </summary>
            public string this[string name, int index]
            {
                get { return this[GetArrayName(index, name)]; }
                set { this[GetArrayName(index, name)] = value; }
            }

            private static string GetArrayName(int index, string name)
            {
                if (index < 0)
                    throw new ArgumentOutOfRangeException("index", "index can not be negative : " + index);
                return name + index;
            }

            #endregion Array methods
        }

        public class PayExtend
        {
            /// <summary>
            /// 发起支付请求的浏览器头信息
            /// </summary>
            public string UserAgent { get; set; }

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
            ///     当前用户注册时间
            /// </summary>
            public DateTime Createdate { get; set; }

            /// <summary>
            ///     当前用户总共交易次数
            /// </summary>
            public int Transactioncounttotal { get; set; }

            /// <summary>
            ///     当前用户三个月内交易次数
            /// </summary>
            public int Transactioncountthreemonths { get; set; }

            /// <summary>
            ///     当前用户国家
            /// </summary>
            public string Country { get; set; }
        }
    }
}

//https://developer.paypal.com/webapps/developer/docs/classic/express-checkout/integration-guide/ECGettingStarted/

//DoExpressCheckoutPayment
//https://developer.paypal.com/webapps/developer/docs/classic/api/merchant/DoExpressCheckoutPayment_API_Operation_NVP/#usesessionpaymentdetails

//lpyedge-buyer@gmail.com  12345678