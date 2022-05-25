using System.Collections.Generic;

namespace Payments.Plartform.Payssion
{
    [PayChannel(EChannel.iDeal)]
    public class Payu : PayBase
    {
        public Payu()
            : base()
        {
            m_pmid = "payu_pl";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }

        public Payu(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "payu_pl";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }
    }
}