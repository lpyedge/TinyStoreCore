using System.Collections.Generic;

namespace LPayments.Plartform.Payssion
{
    [PayChannel(EChannel.Poli)]
    public class Poli : PayBase
    {
        public Poli()
            : base()
        {
            m_pmid = "polipayment";
            Currencies = new List<ECurrency>
            {
                ECurrency.AUD
            };
        }

        public Poli(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "polipayment";
            Currencies = new List<ECurrency>
            {
                ECurrency.AUD
            };
        }
    }
}