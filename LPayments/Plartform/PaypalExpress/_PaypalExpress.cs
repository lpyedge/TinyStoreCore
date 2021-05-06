using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Web;
using LPayments.Utils;

namespace LPayments.Plartform.PaypalExpress
{
    public abstract class _PaypalExpress : IPayChannel
    {
        public readonly string Account = "Account";
        public readonly string Password = "Password";
        public readonly string Signature = "Signature";
        public readonly string Username = "Username";

        public _PaypalExpress() : base()
        {
            Platform = EPlatform.Paypal;
        }

        public _PaypalExpress(string p_SettingsJson) : this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {

            Settings = new List<Setting>
            {
                new Setting
                {
                    Name = Account,
                    Description = "Paypal的商家帐号(Email地址)",
                    Regex = @"^[\w\.@]+$",
                    Requied = true
                },
                new Setting
                {
                    Name = Username,
                    Description = "Paypal的API Username",
                    Regex = @"^[\w\.\-]+$",
                    Requied = true
                },
                new Setting
                {
                    Name = Password,
                    Description = "Paypal的API Password",
                    Regex = @"^[\w\.\-]+$",
                    Requied = true
                },
                new Setting
                {
                    Name = Signature,
                    Description = "Paypal的Signature",
                    Regex = @"^[\w\.\-]+$",
                    Requied = true
                }
            };

            Currencies = new List<ECurrency>
            {
                ECurrency.USD,
                ECurrency.AUD,
                ECurrency.BRL,
                ECurrency.CAD,
                ECurrency.CZK,
                ECurrency.DKK,
                ECurrency.EUR,
                ECurrency.HKD,
                ECurrency.HUF,
                ECurrency.ILS,
                ECurrency.JPY,
                ECurrency.MYR,
                ECurrency.MXN,
                ECurrency.NOK,
                ECurrency.NZD,
                ECurrency.PHP,
                ECurrency.PLN,
                ECurrency.GBP,
                ECurrency.RUB,
                ECurrency.SGD,
                ECurrency.SEK,
                ECurrency.CHF,
                ECurrency.TWD,
                ECurrency.THB,
                ECurrency.TRY
            };
        }

    }
}

//https://developer.paypal.com/webapps/developer/docs/classic/express-checkout/integration-guide/ECGettingStarted/

//DoExpressCheckoutPayment
//https://developer.paypal.com/webapps/developer/docs/classic/api/merchant/DoExpressCheckoutPayment_API_Operation_NVP/#usesessionpaymentdetails

//lpyedge-buyer@gmail.com  12345678