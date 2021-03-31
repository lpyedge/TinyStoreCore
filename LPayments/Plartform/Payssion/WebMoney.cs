using System.Collections.Generic;

namespace LPayments.Plartform.Payssion
{
    [PayChannel(EChannel.WebMoney)]
    public class WebMoney : PayBase
    {
        public WebMoney()
            : base()
        {
            m_pmid = "webmoney";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD,
                ECurrency.RUB
            };
        }

        public WebMoney(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "webmoney";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD,
                ECurrency.RUB
            };
        }
    }
}