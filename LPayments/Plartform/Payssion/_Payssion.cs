using System;
using System.Collections.Generic;

namespace LPayments.Plartform.Payssion
{
   
    public abstract class _Payssion : IPayChannel
    {
        public const string ApiKey = "ApiKey";
        public const string SecretKey = "SecretKey";

        public _Payssion():base()
        {
            Platform = EPlatform.Payssion;
        }

        public _Payssion(string p_SettingsJson):this()
        {
            if (!string.IsNullOrWhiteSpace(p_SettingsJson)) SettingsJson = p_SettingsJson;
        }

        protected override void Init()
        {
            Settings = new List<Setting>
            {
                new Setting {Name = ApiKey, Description = "Payssion的api_key", Regex = @"^\w+$", Requied = true},
                new Setting {Name = SecretKey, Description = "Payssion的secret_key", Regex = @"^\w+$", Requied = true}
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

//merchant_id：VeyronSale

//veyronsale.com c462e19f

//gw2sale.com e5f04f7b