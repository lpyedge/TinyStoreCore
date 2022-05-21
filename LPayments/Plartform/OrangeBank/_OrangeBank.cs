using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Web;

namespace LPayments.Plartform.OrangeBank
{
    //out_no=testorder1
    //pmt_tag = AlipayCS
    //ord_name=TestGoods1
    //trade_amount = 50
    //original_amount=50
    //notify_url=http://www.4ksk.com/ok.html

    //待加密字符串
    //notify_url = http://www.4ksk.com/ok.html&ord_name=TestGoods1&original_amount=50&out_no=testorder1&pmt_tag=AlipayCS&trade_amount=50

    //aes加密后并做bin2hex的字符串
    //0xF380223E402E4C9F6305E78B2E9211C278ED5B57B6121DCC4460F0D53CB665C7864A4514C9DE5327B83E3A1D466812653B8422AAD41A4FDE73535E8BB8D35A8BDDED26C7900DBAAED0189A21D7D7E0401C16053E1BD37C5AD2BAD08DB107424EA853F2EEA6E0DCD30CC3DE827C5AB1AF59E50F897568A0D0FC4A4C8DC4E4EA3942858604989D590EAAC93CBCBEFDF88E

    //待签名的字符串
    //data = 0xF380223E402E4C9F6305E78B2E9211C278ED5B57B6121DCC4460F0D53CB665C7864A4514C9DE5327B83E3A1D466812653B8422AAD41A4FDE73535E8BB8D35A8BDDED26C7900DBAAED0189A21D7D7E0401C16053E1BD37C5AD2BAD08DB107424EA853F2EEA6E0DCD30CC3DE827C5AB1AF59E50F897568A0D0FC4A4C8DC4E4EA3942858604989D590EAAC93CBCBEFDF88E & open_id = 0e5368d956ed5215a7d0f165cb64392b&open_key=a629d0de586f17737712f29128e2ea8e&timestamp=1503052647

    //签名结果
    //3e1051ed49081cf0c883e5311462e3d4

    public abstract class _OrangeBank : IPayChannel, IPay
    {
        public const string MerchantId = "MerchantId";
        public const string OpenId = "OpenId";
        public const string SecretKey = "SecretKey";

        protected bool m_qrcode = true;
        protected string m_service = "";

        protected _OrangeBank() : base()
        {
            Platform = EPlatform.OrangeBank;
        }

        protected _OrangeBank(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = MerchantId, Description = "橙e网的商户号", Regex = @"^\w+$", Requied = true},
                new Setting {Name = OpenId, Description = "橙e网的OpenId", Regex = @"^\w+$", Requied = true},
                new Setting {Name = SecretKey, Description = "橙e网的密钥", Regex = @"^\w+$", Requied = true}
            };

            Currencies = new List<ECurrency>
            {
                ECurrency.CNY,
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
            var sign = query["sign"];
            query.Remove("sign");
            query["open_key"] = this[SecretKey];
            var signtemp = Utils.Core.MD5(
                Utils.Core.LinkStr(
                    query.OrderBy(p => p.Key).ToDictionary(p => p.Key, p => p.Value)
                )
            );
            if (string.Equals(signtemp, sign, StringComparison.OrdinalIgnoreCase) && query["status"] == "1")
            {
                result = new PayResult
                {
                    OrderName = "",
                    OrderID = query["out_no"],
                    Amount = double.Parse(query["amount"]) / 100,
                    Tax = -1,
                    Currency = ECurrency.CNY,
                    Business = this[OpenId],
                    TxnID = query["ord_no"],
                    PaymentName = Name,
                    PaymentDate = DateTime.UtcNow,

                    Message = "notify_success",
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
            if (string.IsNullOrEmpty(this[MerchantId])) throw new ArgumentNullException("ClientID");
            if (string.IsNullOrEmpty(this[SecretKey])) throw new ArgumentNullException("SecretKey");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            var uri
#if DEBUG
                = new Uri("https://mixpayuat4.orangebank.com.cn/mct1/payorder");
#else
                = new Uri("https://api.orangebank.com.cn/mct1/payorder");
#endif
            //m_service = "weixin.scan";
            var dic = new Dictionary<string, string>
            {
                ["open_id"] = this[OpenId],
                ["timestamp"] = Utils.Core.GetTimeStampSeconds(),
                //["mch_id"] = this[MerchantId],
                ["open_key"] = this[SecretKey],
            };
            var data = new SortedDictionary<string, string>()
            {
                ["out_no"] = p_OrderId,
                ["pmt_tag"] = m_service,
                ["ord_name"] = p_OrderName,
                ["original_amount"] = (p_Amount * 100).ToString("0"),
                ["trade_amount"] = (p_Amount * 100).ToString("0"),
                ["notify_url"] = p_NotifyUrl,
            };

            //var datasignstr = data.Aggregate("",(x, y) => string.IsNullOrWhiteSpace(y.Value) ? x + "" : x + y.Key + "=" + y.Value + "&").TrimEnd('&');
            //data["sign"] = Utils.Core.MD5(Utils.HASHCrypto.Generate(Utils.HASHCrypto.CryptoEnum.SHA1).Encrypt(datasignstr));
            //data.Remove("open_key");
            var datastr = Utils.Json.Serialize(data);

            var aes = Utils.DESCrypto.Generate(this[SecretKey], "", Utils.DESCrypto.CryptoEnum.Rijndael,
                System.Security.Cryptography.CipherMode.ECB, System.Security.Cryptography.PaddingMode.PKCS7, 128);

            dic["data"] = Utils.HexCoding.Encode(Utils.DESCrypto.Encrypt2Byte(aes, datastr));

            var signstr = dic.OrderBy(p => p.Key).Aggregate("",
                (x, y) => string.IsNullOrWhiteSpace(y.Value) ? x + "" : x + y.Key + "=" + y.Value + "&").TrimEnd('&');
            dic["sign"] = Utils.Core.SHA1(signstr);
            dic.Remove("open_key");

            var res = _HWU.PostStringAsync(uri, Utils.Core.LinkStr( dic,encode:true)).Result;

            var resdic = Utils.Json.Deserialize<Dictionary<string, string>>(res);

            if (resdic["errcode"] == "0")
            {
                var sign = resdic["sign"];
                resdic.Remove("sign");
                resdic["open_key"] = this[SecretKey];
                var signtemp = Utils.Core.MD5(Utils.HASHCrypto.Encrypt(Utils.HASHCrypto.Generate(Utils.HASHCrypto.CryptoEnum.SHA1),
                    resdic.OrderBy(p => p.Key).Aggregate("",
                            (x, y) => string.IsNullOrWhiteSpace(y.Value) ? x + "" : x + y.Key + "=" + y.Value + "&")
                        .TrimEnd('&')));

                if (string.Equals(signtemp, sign, StringComparison.OrdinalIgnoreCase))
                {
                    byte[] cryptobytes =Utils.HexCoding.Decode(resdic["data"]);
                    var resdatadic =
                        Utils.Json.Deserialize<Dictionary<string, string>>(
                            Utils.DESCrypto.Decrypt(aes,cryptobytes));

                    if (m_qrcode)
                    {
                        return new PayTicket()
                        {
                            Name = this.Name,
                            DataFormat = EPayDataFormat.QrCode,
                            DataContent = resdatadic["trade_qrcode"]
                        };
                    }
                    else
                    {
                        return new PayTicket()
                        {
                            Name = this.Name,
                            DataFormat = EPayDataFormat.Form,
                            DataContent = uri.ToString()+"??" + Utils.Core.LinkStr(dic,encode:true),
                        };
                    }
                }
                else
                {
                    return new PayTicket()
                    {
                        Name = this.Name,
                        DataFormat = EPayDataFormat.Error,
                        Message = "验签失败!"
                    };
                }
            }
            else
            {
                return new PayTicket()
                {
                    Name = this.Name,
                    DataFormat = EPayDataFormat.Error,
                    Message = resdic["msg"]
                };
            }
        }
    }
}