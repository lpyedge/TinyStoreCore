using System.Collections.Generic;

namespace LPayments.Plartform.Payssion
{
    [PayChannel(EChannel.Bancontact)]
    public class Bancontact : PayBase
    {
        public Bancontact()
            : base()
        {
            m_pmid = "bancontact_be";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }

        public Bancontact(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "bancontact_be";
            Currencies = new List<ECurrency>
            {
                ECurrency.EUR
            };
        }
    }
}