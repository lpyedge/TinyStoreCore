using System.Collections.Generic;

namespace LPayments.Plartform.Payssion
{
    [PayChannel(EChannel.iDeal)]
    public class iDeal : PayBase
    {
        public iDeal()
            : base()
        {
            m_pmid = "ideal_nl";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }

        public iDeal(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "ideal_nl";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }
    }
}