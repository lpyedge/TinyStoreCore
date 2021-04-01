using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using LPayments.Utils;

namespace LPayments.Plartform.GPayTr
{
    [PayPlatformAttribute("GPayTr", "土耳其信用卡收款", SiteUrl = "https://gpay.com.tr/")]
    [PayChannel(EChannel.GPayTr)]
    public class GPayTr : IPayChannel, IPay
    {
        public const string Username = "Username"; //demohesap
        public const string Key = "Key"; //9aR3FjCI5

        //  {   Currency.ALL ,   8   }   ,
        private static readonly Dictionary<ECurrency, int> CurrencyCodes = new Dictionary<ECurrency, int>()
        {
            {ECurrency.AFN, 971},
            {ECurrency.EUR, 978},
            {ECurrency.DZD, 12},
            {ECurrency.USD, 840},
            {ECurrency.AOA, 973},
            {ECurrency.XCD, 951},
            {ECurrency.ARS, 32},
            {ECurrency.AMD, 51},
            {ECurrency.AWG, 533},
            {ECurrency.AUD, 36},
            {ECurrency.AZN, 944},
            {ECurrency.BSD, 44},
            {ECurrency.BHD, 48},
            {ECurrency.BDT, 50},
            {ECurrency.BBD, 52},
            {ECurrency.BZD, 84},
            {ECurrency.XOF, 952},
            {ECurrency.BMD, 60},
            {ECurrency.BTN, 64},
            {ECurrency.INR, 356},
            {ECurrency.BOB, 68},
            {ECurrency.BAM, 977},
            {ECurrency.BWP, 72},
            {ECurrency.NOK, 578},
            {ECurrency.BRL, 986},
            {ECurrency.BND, 96},
            {ECurrency.BGN, 975},
            {ECurrency.BIF, 108},
            {ECurrency.KHR, 116},
            {ECurrency.XAF, 950},
            {ECurrency.CAD, 124},
            {ECurrency.CVE, 132},
            {ECurrency.KYD, 136},
            {ECurrency.CLP, 152},
            {ECurrency.CNY, 156},
            {ECurrency.COP, 170},
            {ECurrency.KMF, 174},
            {ECurrency.CDF, 976},
            {ECurrency.NZD, 554},
            {ECurrency.CRC, 188},
            {ECurrency.HRK, 191},
            {ECurrency.ANG, 532},
            {ECurrency.CZK, 203},
            {ECurrency.DKK, 208},
            {ECurrency.DJF, 262},
            {ECurrency.DOP, 214},
            {ECurrency.EGP, 818},
            {ECurrency.ERN, 232},
            {ECurrency.ETB, 230},
            {ECurrency.FKP, 238},
            {ECurrency.FJD, 242},
            {ECurrency.XPF, 953},
            {ECurrency.GMD, 270},
            {ECurrency.GEL, 981},
            {ECurrency.GHS, 936},
            {ECurrency.GIP, 292},
            {ECurrency.GTQ, 320},
            {ECurrency.GBP, 826},
            {ECurrency.GNF, 324},
            {ECurrency.GYD, 328},
            {ECurrency.HTG, 332},
            {ECurrency.HNL, 340},
            {ECurrency.HKD, 344},
            {ECurrency.HUF, 348},
            {ECurrency.ISK, 352},
            {ECurrency.IDR, 360},
            {ECurrency.ILS, 376},
            {ECurrency.JMD, 388},
            {ECurrency.JPY, 392},
            {ECurrency.JOD, 400},
            {ECurrency.KZT, 398},
            {ECurrency.KES, 404},
            {ECurrency.KRW, 410},
            {ECurrency.KWD, 414},
            {ECurrency.KGS, 417},
            {ECurrency.LAK, 418},
            {ECurrency.LBP, 422},
            {ECurrency.ZAR, 710},
            {ECurrency.LRD, 430},
            {ECurrency.CHF, 756},
            {ECurrency.MOP, 446},
            {ECurrency.MKD, 807},
            {ECurrency.MGA, 969},
            {ECurrency.MWK, 454},
            {ECurrency.MYR, 458},
            {ECurrency.MVR, 462},
            {ECurrency.MRO, 478},
            {ECurrency.MUR, 480},
            {ECurrency.MXN, 484},
            {ECurrency.MDL, 498},
            {ECurrency.MNT, 496},
            {ECurrency.MAD, 504},
            {ECurrency.MZN, 943},
            {ECurrency.MMK, 104},
            {ECurrency.NAD, 516},
            {ECurrency.NPR, 524},
            {ECurrency.NIO, 558},
            {ECurrency.NGN, 566},
            {ECurrency.OMR, 512},
            {ECurrency.PKR, 586},
            {ECurrency.PAB, 590},
            {ECurrency.PGK, 598},
            {ECurrency.PYG, 600},
            {ECurrency.PEN, 604},
            {ECurrency.PHP, 608},
            {ECurrency.PLN, 985},
            {ECurrency.QAR, 634},
            {ECurrency.RON, 946},
            {ECurrency.RUB, 643},
            {ECurrency.RWF, 646},
            {ECurrency.SHP, 654},
            {ECurrency.WST, 882},
            {ECurrency.STD, 678},
            {ECurrency.SAR, 682},
            {ECurrency.RSD, 941},
            {ECurrency.SCR, 690},
            {ECurrency.SLL, 694},
            {ECurrency.SGD, 702},
            {ECurrency.SBD, 90},
            {ECurrency.SOS, 706},
            {ECurrency.LKR, 144},
            {ECurrency.SRD, 968},
            {ECurrency.SZL, 748},
            {ECurrency.SEK, 752},
            {ECurrency.SYP, 760},
            {ECurrency.TWD, 901},
            {ECurrency.TJS, 972},
            {ECurrency.TZS, 834},
            {ECurrency.THB, 764},
            {ECurrency.TOP, 776},
            {ECurrency.TTD, 780},
            {ECurrency.TND, 788},
            {ECurrency.TRY, 949},
            {ECurrency.UGX, 800},
            {ECurrency.UAH, 980},
            {ECurrency.AED, 784},
            {ECurrency.UYU, 858},
            {ECurrency.UZS, 860},
            {ECurrency.VUV, 548},
            {ECurrency.VEF, 937},
            {ECurrency.VND, 704},
            {ECurrency.YER, 886},
            {ECurrency.ZMW, 967},
        };

        private static readonly string[] ipWhitelist = new string[]
        {
            "185.201.212.194", //dev
            "77.223.135.234", //product
        };

        public GPayTr() : base()
        {
        }

        public GPayTr(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting
                {
                    Name = Username, Description = "GPay.com.tr的Username", Regex = @"^[\w\-]+$", Requied = true
                },
                new Setting {Name = Key, Description = "GPay.com.tr的Key", Regex = @"^[\w\W]+$", Requied = true}
            };

            //  <option\s+value="(\d+)">[\w&;".’,'\s\(\)-]+?\((\w+)\)</option>

            Currencies = new List<ECurrency>();
            foreach (var name in Enum.GetNames(typeof(ECurrency)))
                Currencies.Add((ECurrency) Enum.Parse(typeof(ECurrency), name, true));
        }

        public PayResult Notify(IDictionary<string, string> form, IDictionary<string, string> query,
            IDictionary<string, string> head, string body, string notifyip)
        {
            PayResult result = new PayResult
            {
                Status = PayResult.EStatus.Pending,
                Message = "fail"
            };

            string md5Val = Utils.Core.MD5(Convert.ToBase64String(Encoding.ASCII.GetBytes(this[Key].Substring(0, 7) +
                form["siparis_id"].Substring(0, 5) + form["tutar"] + form["islem_sonucu"])));

            if (ipWhitelist.Contains(notifyip) && form["hash"] == md5Val && form["islem_sonucu"] == "2")
            {
                result = new PayResult
                {
                    OrderName = "",
                    OrderID = form["siparis_id"],
                    Amount = double.Parse(form["tutar"]),
                    Tax = -1,
                    Currency = Utils.Core.Parse<ECurrency>(form["currency"]),
                    Business = this[Username],
                    TxnID = form["siparis_id"],
                    PaymentName = Name,
                    PaymentDate = DateTime.UtcNow,

                    Message = "1",
                };
            }

            return result;
        }

        public PayTicket Pay(string p_OrderId, double p_Amount,
            ECurrency p_Currency, string p_OrderName, IPAddress p_ClientIP = null, string p_ReturnUrl = "",
            string p_NotifyUrl = "", string p_CancelUrl = "", dynamic extend_params = null)
        {
            if (p_OrderId == null) throw new ArgumentNullException("p_OrderId");

            if (string.IsNullOrEmpty(this[Username])) throw new ArgumentNullException("Username");
            if (string.IsNullOrEmpty(this[Key])) throw new ArgumentNullException("Key");
            if (!Currencies.Contains(p_Currency)) throw new ArgumentException("Currency is not allowed!");

            var datas = new Dictionary<string, string>();
            datas["username"] = this[Username]; // "demodemo";
            datas["key"] = this[Key]; // "CoEf547YT";
            datas["order_id"] = Convert.ToBase64String(Encoding.ASCII.GetBytes(p_OrderId));
            datas["amount"] = p_Amount.ToString("0.00");
            datas["currency"] = CurrencyCodes[p_Currency].ToString();
            datas["return_url"] = p_ReturnUrl;
            //dic["phone"] = "555xxxyyzz";  选填
            datas["selected_payment"] =
                "krediKarti"; //'havale' for bank transfer, 'gpay' for gpay wallet, 'krediKarti' for credit card, 'epin' for E-Pin
            //dic["selected_bank_id"] = //银行转账才设置此属性 'selected_payment' is 'havale' (bank transfer) you can select the bank id for transfer process.

            //opsiyonel
            //dic["products[0][product_name]"] = "Knight Online Chip";
            //dic["products[0][product_amount]"] = "3.33";
            //dic["products[0][product_currency]"] = "949";
            //dic["products[0][product_type]"] = "oyun";
            //dic["products[0][product_img]"] = "http://xxx.xxx.xxx/xxx.xx.jpg";

            //dic["products[1][product_name]"] = "Usb Kablo";
            //dic["products[1][product_amount]"] = "6.78";
            //dic["products[1][product_currency]"] = "949";
            //dic["products[1][product_type]"] = "alışveriş";
            //dic["products[1][product_img]"] = "http://xxx.xxx.xxx/xxx.xx.jpg";

            var uri = new Uri(
#if DEBUG
                "https://www.testgpay.com/ApiRequest"
#else
                "https://gpay.com.tr/ApiRequest"
#endif
            );

            var res = _HWU.Response(uri, HttpWebUtility.HttpMethod.Post, datas);

            if (res.Contains("\"state\":1"))
            {
                var json = Utils.Json.Deserialize<dynamic>(res);

                // pt.Uri = json.link;
                // pt.FormHtml = "<script>location.href='" + (string) json.redirect_url + "';</script>";
                
                return new PayTicket(false)
                {
                    Action = EAction.UrlGet,
                    Uri = (string) json.redirect_url,
                    Token = json
                };
            }
            else
            {
                return new PayTicket(false)
                {
                    Message = res
                };
            }
        }
    }
}

//Byrm:
//Www.testgpay.com
//Byrm:
//Id: Demohesap
//PW: demohesap