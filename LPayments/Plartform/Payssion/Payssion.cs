using System;
using System.Collections.Generic;
using System.Net;
using System.Web;

namespace LPayments.Plartform.Payssion
{
    [PayChannel(EChannel.Payssion)]
    public class PayBase : _Payssion
    {
        protected string m_pmid = "";

        public PayBase() : base()
        {
        }

        public PayBase(string p_SettingsJson) : base(p_SettingsJson)
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

            var IsVlidate =
                !string.IsNullOrWhiteSpace(form["pm_id"]) &&
                !string.IsNullOrWhiteSpace(form["transaction_id"]) &&
                !string.IsNullOrWhiteSpace(form["track_id"]) &&
                form["sub_track_id"] != null &&
                !string.IsNullOrWhiteSpace(form["amount"]) &&
                !string.IsNullOrWhiteSpace(form["currency"]) &&
                form["description"] != null &&
                !string.IsNullOrWhiteSpace(form["state"]) &&
                !string.IsNullOrWhiteSpace(form["notify_sig"]) &&
                string.Equals(form["notify_sig"],
                    Utils.Core.MD5(string.Join("|", this[ApiKey], form["pm_id"], form["amount"],
                        form["currency"], form["track_id"], form["sub_track_id"], form["state"],
                        this[SecretKey])), StringComparison.OrdinalIgnoreCase);
            //Utils.Core.MD5(string.Join("|", this[ApiKey], form["pm_id"], form["amount"],
            //    form["currency"], form["state"], this[SecretKey])), StringComparison.OrdinalIgnoreCase);

            if (IsVlidate && string.Equals(form["state"], "completed", StringComparison.OrdinalIgnoreCase))
            {
                result = new PayResult
                {
                    OrderName = form["description"],
                    OrderID = form["track_id"],
                    Amount = double.Parse(form["amount"]),
                    Tax =
                        form.ContainsKey("paid") && form.ContainsKey("net")
                            ? double.Parse(form["paid"]) - double.Parse(form["net"])
                            : -1,
                    Currency = Utils.Core.Parse<ECurrency>(form["currency"]),
                    Business = this[ApiKey],
                    TxnID = form["transaction_id"],
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

            if (string.IsNullOrEmpty(this[ApiKey])) throw new ArgumentNullException("ApiKey");
            if (string.IsNullOrEmpty(this[SecretKey])) throw new ArgumentNullException("SecretKey");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

           
            if (string.IsNullOrWhiteSpace(m_pmid))
            {
                var url =
                    string.Format(
                        "https://www.payssion.com/checkout/{0}?order_id={1}&logo={2}&description={3}&amount={4}&currency={5}&payer_email={6}&redirect_url={7}&notify_url={8}&api_sig={9}",
                        this[ApiKey], Utils.Core.UriDataEncode(p_OrderId), "",
                        Utils.Core.UriDataEncode(p_OrderName),
                        p_Amount.ToString("0.00"), p_Currency, "", Utils.Core.UriDataEncode(p_ReturnUrl),
                        Utils.Core.UriDataEncode(p_NotifyUrl),
                        Utils.Core.MD5(string.Join("|", this[ApiKey], p_Amount.ToString("0.00"), p_Currency, p_OrderId,
                            this[SecretKey])));
                // pt.Uri = url;
                // pt.FormHtml = "<script>location.href='" + url + "';</script>";
                return new PayTicket()
                {
                    Name = this.Name,
                    DataFormat = EPayDataFormat.Url,
                    DataContent = url,
                };
            }
            else
            {
                var dic = new Dictionary<string, string>();
                dic.Add("api_key", this[ApiKey]);
                dic.Add("pm_id", m_pmid);
                dic.Add("payer_ref", "");
                dic.Add("payer_name", "");
                dic.Add("payer_email", "");
                dic.Add("amount", p_Amount.ToString("0.00"));
                dic.Add("currency", p_Currency.ToString());
                dic.Add("description", p_OrderName);
                dic.Add("language", "en");
                dic.Add("api_sig",
                    Utils.Core.MD5(string.Join("|", this[ApiKey], m_pmid, p_Amount.ToString("0.00"), p_Currency,
                        p_OrderId, "", this[SecretKey])));
                dic.Add("track_id", p_OrderId);
                dic.Add("sub_track_id", "");
                dic.Add("notify_url", p_NotifyUrl);
                dic.Add("success_url", p_ReturnUrl);
                dic.Add("fail_url", p_CancelUrl);

                var uri = new Uri(
#if DEBUG
                    "http://sandbox.payssion.com/api/v1/payment/create"
#else
                    "https://www.payssion.com/api/v1/payment/create"
#endif
                );

                var res = _HWU.PostStringAsync(uri, Utils.Core.LinkStr( dic,encode:true)).Result;

                if (res.Contains("\"result_code\":200"))
                {
                    var json = Utils.DynamicJson.Parse(res);
                    // pt.Uri = (string) json.redirect_url;
                    // pt.FormHtml = "<script>location.href='" + (string) json.redirect_url + "';</script>";
                    return new PayTicket()
                    {
                        Name = this.Name,
                        DataFormat = EPayDataFormat.Url,
                        DataContent = (string) json.redirect_url,
                    };
                }

                return new PayTicket()
                {
                    Name = this.Name,
                    DataFormat = EPayDataFormat.Error,
                    Message = res
                };
            }
        }
    }
}

//merchant_id：VeyronSale

//veyronsale.com c462e19f

//gw2sale.com e5f04f7b