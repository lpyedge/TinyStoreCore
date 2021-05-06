using System;
using System.Collections.Generic;
using System.Net;
using System.Text;
using System.Web;

namespace LPayments.Plartform.Smart2Pay
{
    
    [PayChannel(EChannel.CreditCard)]
    public class Smart2Pay : IPayChannel, IPay
    {
        public const string MerchantID = "MerchantID";
        public const string NotifyEmail = "NotifyEmail";

        public Smart2Pay() : base()
        {
            Platform = EPlatform.Smart2Pay;
        }

        public Smart2Pay(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = MerchantID, Description = "Smart2Pay的商家帐号", Regex = @"^[\w\.@]+$", Requied = true},
                new Setting {Name = NotifyEmail, Description = "接受付款通知的Email地址", Regex = @"^\w+$", Requied = false}
            };

            Currencies = new List<ECurrency>
            {
                ECurrency.USD,
                ECurrency.AUD,
                ECurrency.CAD,
                ECurrency.EUR,
                ECurrency.GBP,
                ECurrency.JPY,
                ECurrency.CHF,
                ECurrency.JMD
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

            var IsVlidate = string.Equals(form["FinalStatus"], "success", StringComparison.OrdinalIgnoreCase);
            if (IsVlidate)
            {
                result = new PayResult
                {
                    OrderName = form["OrderName"],
                    OrderID = form["orderID"],
                    Amount = double.Parse(form["Amount"]),
                    Tax = -1,
                    Currency = Utils.Core.Parse<ECurrency>(form["Currency"]),
                    Business = this[MerchantID],
                    TxnID = "",
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

            if (string.IsNullOrEmpty(this[MerchantID])) throw new ArgumentNullException("MerchantID");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            // var formhtml =
            //     new StringBuilder("<form id='Core.PaymentFormNam' name='Core.PaymentFormName" +
            //                       "' action='https://admin.smart2pay.com/payment/pay.cgi' method='post' >");
            // formhtml.Append(
            //     "<input type='hidden' name='card-allowed' value='Visa,Mastercard,Amex,Discover,Diners,JCB,EasyLink,Bermuda,IslandCard,Butterfield,KeyCard,MilStar,Solo,Switch' />");
            // formhtml.Append("<input type='hidden' name='comm-title' value='Comments' />");
            // formhtml.AppendFormat("<input type='hidden' name='comments' value='{0}' />", p_OrderName);
            // formhtml.Append("<input type='hidden' name='easycart' value='0' />");
            // formhtml.Append("<input type='hidden' name='shipinfo' value='0' />");
            // formhtml.AppendFormat("<input type='hidden' name='customname1' value='{0}' />", "OrderName");
            // formhtml.AppendFormat("<input type='hidden' name='customvalue1' value='{0}' />", p_OrderName);
            // formhtml.AppendFormat("<input type='hidden' name='customname2' value='{0}' />", "Amount");
            // formhtml.AppendFormat("<input type='hidden' name='customvalue2' value='{0}' />", p_Amount.ToString("0.##"));
            // formhtml.AppendFormat("<input type='hidden' name='customname3' value='{0}' />", "Currency");
            // formhtml.AppendFormat("<input type='hidden' name='customvalue3' value='{0}' />",
            //     p_Currency.ToString().ToLowerInvariant());
            //
            // formhtml.AppendFormat("<input type='hidden' name='currency' value='{0}' />",
            //     p_Currency.ToString().ToLowerInvariant());
            // formhtml.AppendFormat("<input type='hidden' name='orderID' value='{0}' />", p_OrderId);
            // formhtml.AppendFormat("<input type='hidden' name='order-id' value='{0}' />", p_OrderId);
            // formhtml.AppendFormat("<input type='hidden' name='publisher-name' value='{0}' />", this[MerchantID]);
            // formhtml.AppendFormat("<input type='hidden' name='publisher-email' value='{0}' />", this[NotifyEmail]);
            // formhtml.AppendFormat("<input type='hidden' name='card-amount' value='{0}' />", p_Amount.ToString("0.##"));
            // formhtml.AppendFormat("<input type='hidden' name='receipt-url' value='{0}' />",
            //     new Uri(p_ReturnUrl).Scheme + "://" + new Uri(p_ReturnUrl).Authority);
            // formhtml.AppendFormat("<input type='hidden' name='problem-link' value='{0}' />", p_CancelUrl);
            // formhtml.AppendFormat("<input type='hidden' name='return_url' value='{0}' />", p_ReturnUrl);
            // formhtml.AppendFormat("<input type='hidden' name='success-link' value='{0}' />", p_NotifyUrl);
            // formhtml.Append("<input type='submit' value='pay' style='display: none;'/>");
            // formhtml.Append("</form>");

            var datas = new Dictionary<string, string>()
            {
                ["publisher-name"]=this[MerchantID],
                ["publisher-email"]=this[NotifyEmail],
                ["card-allowed"] = "Visa,Mastercard,Amex,Discover,Diners,JCB,EasyLink,Bermuda,IslandCard,Butterfield,KeyCard,MilStar,Solo,Switch",
                ["comm-title"]="Comments",
                ["comments"]=p_OrderName,
                ["easycart"]="0",
                ["shipinfo"]="0",
                ["customname1"]="OrderName",
                ["customvalue1"]=p_OrderName,
                ["customname2"]="Amount",
                ["customvalue2"]= p_Amount.ToString("0.##"),
                ["customname3"]="Currency",
                ["customvalue3"]=p_Currency.ToString(),
                ["currency"]=p_Currency.ToString().ToLowerInvariant(),
                ["orderID"]=p_OrderId,
                ["order-id"]=p_OrderId,
                ["card-amount"]=p_Amount.ToString("0.##"),
                ["receipt-url"]=new Uri(p_ReturnUrl).Scheme + "://" + new Uri(p_ReturnUrl).Authority,
                ["problem-link"]=p_CancelUrl,
                ["return_url"]=p_ReturnUrl,
                ["success-link"]=p_NotifyUrl,
            };

            return new PayTicket()
            {
                Action = EAction.UrlPost,
                Uri = "https://admin.smart2pay.com/payment/pay.cgi",
                Datas = datas
            };
        }

        //    if (Params.Trim() == "")
        //    Uri uri = new Uri(Url);
        //        Url = "https://pay1.plugnpay.com/payment/pnpremote.cgi";
        //    if (Url.Trim() == "")
        //{
        //protected object PnPSend(string Params, string Url = "https://pay1.plugnpay.com/payment/pnpremote.cgi")
        //        return "pnpcom_err=No Params";
        //    //Checks to make sure Params is not empty
        //    //Checks to make sure that the URL is an HTTPS url.
        //    if (string.Equals(uri.Scheme, "HTTPS", StringComparison.OrdinalIgnoreCase))
        //    {
        //        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        //        ///'''''''''''''''''''''''''Create Connection Settings'''''''''''''''''''''''''''''''''''''
        //        System.Net.HttpWebRequest request = (System.Net.HttpWebRequest)System.Net.HttpWebRequest.Create(uri);
        //        request.Method = System.Net.WebRequestMethods.Http.Post;
        //        request.ContentLength = Params.Length;
        //        request.ContentType = "application/x-www-form-urlencoded";
        //        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        //        ///''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''''
        //        try
        //        {
        //            System.IO.StreamWriter writer = new System.IO.StreamWriter(request.GetRequestStream());
        //            //Open SSL Connection
        //            writer.Write(Params);
        //            //Send data to open connection
        //            writer.Close();
        //            System.Net.HttpWebResponse oResponse = (System.Net.HttpWebResponse)request.GetResponse();
        //            //Listen for response
        //            System.IO.StreamReader reader = new System.IO.StreamReader(oResponse.GetResponseStream(), System.Text.Encoding.UTF8);
        //            //Receive response
        //            string Reply = reader.ReadToEnd();
        //            //Read response
        //            oResponse.Close();
        //            System.Text.RegularExpressions.Regex ModeRegEx = new System.Text.RegularExpressions.Regex("mode=(?<Type>query_trans|auth|return)");
        //            //Regex statement to search for mode type
        //            string Mode = "";
        //            if (ModeRegEx.Match(Params).Success == true)
        //                Mode = ModeRegEx.Match(Params).Result("${Type}");
        //            //Check mode type
        //            switch (Mode)
        //            {
        //                //Switch on mode type
        //                case "query_trans":
        //                case "batchassemble":
        //                case "query_noc":
        //                case "list_members":
        //                case "query_billing":
        //                    string[] temparray = Reply.Split('&');
        //                    //break up entry into an array
        //                    string DecodeReply = "";
        //                    int i = 0;
        //                    System.Text.RegularExpressions.Regex TempRegex = new System.Text.RegularExpressions.Regex("^(a|dr)\\d+=");
        //                    //setup regex search for substrings
        //                    while (i < temparray.Length)
        //                    {
        //                        if (!(i == 0))
        //                            DecodeReply += "&";
        //                        //add back field seperator
        //                        if (!TempRegex.Match(temparray[i]).Success)
        //                        {
        //                            DecodeReply += System.Web.HttpUtility.UrlDecode(temparray[i]);
        //                        }
        //                        else
        //                        {
        //                            DecodeReply += temparray[i];
        //                            //add encoded information
        //                        }
        //                        i += 1;
        //                    }
        //                    return DecodeReply;
        //                default:
        //                    //Decode reply and send back
        //                    return System.Web.HttpUtility.UrlDecode(Reply);
        //            }
        //        }
        //        catch (Exception Ex)
        //        {
        //            return "npcom_err=" + System.Web.HttpUtility.UrlDecode(Ex.Message);
        //            //Report back an errors which occurred during the SSL connection
        //        }
        //    }
        //    else
        //    {
        //        return "pnpcom_err=Non HTTPS URL";
        //    }
        //}
    }
}