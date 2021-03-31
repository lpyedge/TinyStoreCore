using System.Collections.Generic;

namespace LPayments.Plartform.Payssion
{
    [PayChannel(EChannel.Openbucks)]
    public class Openbucks : PayBase
    {
        public Openbucks()
            : base()
        {
            m_pmid = "openbucks";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }

        public Openbucks(string p_SettingsJson)
            : base(p_SettingsJson)
        {
            m_pmid = "openbucks";
            Currencies = new List<ECurrency>
            {
                ECurrency.USD
            };
        }
    }
}