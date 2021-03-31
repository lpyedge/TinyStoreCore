using System.Collections.Generic;

namespace LPayments.Plartform.Payssion
{
    [PayChannel(EChannel.PaySafeCard)]
    public class PaySafeCard : PayBase
    {
        public PaySafeCard()
            : base()
        {
            m_pmid = "paysafecard";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD,
                ECurrency.EUR,
                ECurrency.GBP
            };
        }

        public PaySafeCard(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "paysafecard";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD,
                ECurrency.EUR,
                ECurrency.GBP
            };
        }
    }
}